def test():
    from tkinter import messagebox
    messagebox.showinfo("알림창","딸랑딸랑")

def test2(msg):
    from tkinter import messagebox
    messagebox.showinfo("알림창",msg)

# test해보고 싶으면 주석 해제
# test()

# 서버와 calibration
import json
import socket
import cv2
import os
import torch
import math
import sys
import atexit
import imutils as imutils
import numpy as np
import time
from ultralytics import YOLO

import CRG as crg
from deximodel import DexiNed
import timeit

# YOLO 및 Airline
from ultralytics import YOLO

#절사 평균과 curve_fit 위한 라이브러리
from scipy import stats
from scipy.optimize import curve_fit

# #라벨링용
# import random
# import string

# curve fitting 돌릴 때 라이브러리 중복되도 넘어가게 하는 코드
os.environ["KMP_DUPLICATE_LIB_OK"]="TRUE"

# 클래스명
YOLO_CLASS = ["welding line", "paint border", "carrier border", "obstacle", "plate border", "hole"]

# 색상 지정
COLOR_SKY=(255,255,0) # 용접선 welding line
COLOR_YELLOW=(0,255,255) # 캐리어 carrier border
COLOR_RED=(0,0,255) # 부재 경계선 plate border

# 이미지 resizing 비율 상수
RESIZE_PARAMETER = 0.8

# 선의 최소 길이 비율
LENGTH_RATIO = 50

#===orientation detector setting===
THETARESOLUTION=6 # 방향의 갯수(6방향 선을 그을 수 있음)
KERNEL_SIZE=9 # 커널 크기 (커널의 사이즈로 성능이 결정됨)

# 폰트 및 폰트 컬러
FONT = cv2.FONT_HERSHEY_PLAIN
FONT_COLOR = (255,255,255)

teststring = "python start"
temp = ""
receivedData = ""


def exit_handler():
    exitmsg = "bye bye!"
    test2(exitmsg)
    server_socket.close()
    


def warpping(img): #원근변환 함수    
    # 이미지의 높이, 너비 픽셀 좌표
    h = img.shape[0]
    w = img.shape[1]
    
    #대입할 이미지 중심 설정
    shift_w = w/2
    shift_h = 0

    # 원본 이미지 좌표 [x,y] (좌상 좌하 우상 우하)
    #화각 87도 기준
    ori_coordinate = np.float32([[50, 0], [135, 172], [590, 0], [505, 172]])

    #대입할 이미지 좌표 (좌상 좌하 우상 우하) 한칸이 15
    warped_coordinate = np.float32([[shift_w-60, shift_h], [shift_w-60, shift_h+40], [shift_w+60, shift_h], [shift_w+60, shift_h+40]])

    #3X3 변환 행렬 생성
    Matrix = cv2.getPerspectiveTransform(ori_coordinate, warped_coordinate)

    #원근 변환
    warped_img = cv2.warpPerspective(img, Matrix, (w, h))
    return warped_img

# curve fitting을 위한 직선의 방정식 함수
def return_y(x,inc,y_interceptor):
    return inc*x+y_interceptor


#x좌표를 구하기 위한 함수
def return_x(y, inc, y_interceptor):
    return (y-y_interceptor)/inc

# 기울기 계산
def cal_inclination(array_xyxy):
    if array_xyxy[0, 1] == array_xyxy[1, 1]:
        return 573
    else :
        return (array_xyxy[1, 0]- array_xyxy[0,0])/(array_xyxy[1,1]-array_xyxy[0,1])

# y절편 계산
def cal_interceptor(inc, x, y):
    return y-inc*x

# 박스 안에 있는지 바운더리 체크하는 함수
def boundary_check(area, array_xyxy):
    if area[0] < array_xyxy[0,1] < area[2] and area[1] < array_xyxy[0,0] < area[3] and area[0] < array_xyxy[1,1] < area[2] and area[1] < array_xyxy[1,0] < area[3]:
        return True
    else:
        return False
    
# 점과 직선사이의 거리
def distance_between_point_line(inc, y_inter, px, py):
    numerator = abs(inc * px -py + y_inter)
    denominator = np.sqrt(inc ** 2 + 1)
    perpendicular_distance = numerator / denominator
    return perpendicular_distance

# Airline 초기화 함수
def init_OD(THETARESOLUTION,KERNEL_SIZE):
    # CNN 레이어 정리(?)
    OD=torch.nn.Conv2d(1,THETARESOLUTION,KERNEL_SIZE,1,KERNEL_SIZE//2,bias=False).cuda()
    # THETARESOLUTION에 따라 방향나누고, 커널 사이즈대로 구성
    for i in range(THETARESOLUTION):
        kernel=np.zeros((KERNEL_SIZE,KERNEL_SIZE))
        angle=i*180/THETARESOLUTION
        x=(np.cos(angle/180*3.1415926)*50).astype(np.int32)
        y=(np.sin(angle/180*3.1415926)*50).astype(np.int32)

        cv2.line(kernel,(KERNEL_SIZE//2-x,KERNEL_SIZE//2-y),(KERNEL_SIZE//2+x,KERNEL_SIZE//2+y),1,1)
        OD.weight.data[i]=torch.tensor(kernel)
    return OD

# Airline 방향 잡기
OrientationDetector=init_OD(THETARESOLUTION,KERNEL_SIZE)

def Aroundview(receiveImgN,receiveImgW,receiveImgS,receiveImgE):
    warp_N = warpping(receiveImgN)
    warp_W = warpping(receiveImgW)
    warp_S = warpping(receiveImgS)
    warp_E = warpping(receiveImgE)

    #이미지 방향에 맞게 회전시키기
    warp_W = imutils.rotate_bound(warp_W, 270)
    warp_N = imutils.rotate_bound(warp_N, 180)
    warp_E = imutils.rotate_bound(warp_E, 90)

    h1, w1, c1 = warp_W.shape
    
    #상하 이미지의 높이, 너비
    #480 640
    h2, w2, c2 = warp_N.shape

    #이격 20
    #검정색 배경 이미지 생성
    inn = 20
    b_w = 960+inn*2
    b_h = 960+inn*2
    background1 = np.zeros((b_w, b_h, 3), dtype=np.uint8)*255
    background2 = np.zeros((b_w, b_h, 3), dtype=np.uint8)*255

    # 배경 이미지에 좌우 이미지 합성
    # 이미지 크기 640*480
    # 480-320 : 480+320
    background1[160+inn:800+inn, 0:w1] = warp_E
    background1[160+inn:800+inn, w1+inn*2:b_w] = warp_W

    background_WE = background1[200:800, 200:800]
    #cv2.imshow('backwe1', background_WE)

    b = background1.copy()

    # or연산을 이용하여 배경이미지에 상하 이미지 합성
    background2[0:h2, 160+inn:800+inn] = warp_N
    background2[h2+inn*2:b_h, 160+inn:800+inn] = warp_S
    background_NS = background2[200:800, 200:800]

    # 기존 방식
    roiN = b[0:h2, 160+inn:800+inn]
    roiS = b[h2+inn*2:b_h, 160+inn:800+inn]

    bit_n = cv2.bitwise_or(roiN, warp_N)
    bit_s = cv2.bitwise_or(roiS, warp_S)

    b[0:h2, 160+inn:800+inn] = bit_n
    b[h2+inn*2:b_h, 160+inn:800+inn] = bit_s
    b = b[200:800, 200:800]

    #cv2.imshow('backwe', background_WE)
    #cv2.imshow('backns', background_NS)

    # 겹치는 부분
    white_COLOR = (255,255,255)
    mask = np.zeros((600,600,3),dtype = np.uint8)
    pt1 = np.array([[0,0],[300,240],[600,0],[360,300],[600,600],[300,360],[0,600],[240,300]], np.int32)
    mask = cv2.polylines(mask,[pt1], True, white_COLOR)

    white_star = cv2.fillPoly(mask, [pt1], white_COLOR)
    black_star = cv2.bitwise_not(white_star)

    dup_WE = cv2.bitwise_and(background_WE, white_star)
    dup_NS = cv2.bitwise_and(background_NS, white_star)

    # 겹치는 부분 제외
    x_star = cv2.bitwise_and(b, black_star)

    # 기존 결과
    #cv2.imshow('b', b)

    # 가중치 합성결과
    dst_NS = cv2.bitwise_or(dup_NS, x_star)
    dst_WE = cv2.bitwise_or(dup_WE, x_star)

    # Fill 0 values in dst_NS with corresponding values from dst_WE
    dst_NS[dst_NS == 0] = dst_WE[dst_NS == 0]

    dst_NS[280:320, 280:320] = np.full((40,40,3),(0,255,0),dtype=np.uint8)
    
    #라벨링용 2줄
    # cnt = ''.join(random.choice(string.digits) for _ in range(10))
    # cv2.imwrite(r"C:\Users\user\UnityGraduate\lastlabel\lastimg_N"+cnt+".jpg",warp_N)
    # cv2.imwrite(r"C:\Users\user\UnityGraduate\lastlabel\lastimg_W"+cnt+".jpg",warp_W)
    # cv2.imwrite(r"C:\Users\user\UnityGraduate\lastlabel\lastimg_S"+cnt+".jpg",warp_S)
    # cv2.imwrite(r"C:\Users\user\UnityGraduate\lastlabel\lastimg_E"+cnt+".jpg",warp_E)
    # cv2.imwrite(r"C:\Users\user\UnityGraduate\lastlabel\lastimg_"+ cnt+".jpg",dst_NS)

    return dst_NS, 600

def boundary_overlap(box1, box2):
    if not box1[2] < box2[0]*RESIZE_PARAMETER or box1[0] > box2[2]*RESIZE_PARAMETER or box1[1] > box2[3]*RESIZE_PARAMETER or box1[3] < box2[1]*RESIZE_PARAMETER:
        return True
    return False

atexit.register(exit_handler)


if __name__ == "__main__":
    # YOLO 모델 경로
    yolomodel = YOLO(r'F:\graduate2\train15\weights\best.pt')

    # dexiNed 모델 로딩
    Premodel=DexiNed().cuda(0)
    Premodel.load_state_dict(torch.load("checkpoints//dexi2.pth"))

    host, port = "127.0.0.1", 25002
    # 소켓 생성
    server_socket = socket.socket(socket.AF_INET,socket.SOCK_STREAM)

    # 소켓을 호스트와 포트에 바인딩합니다.
    server_socket.bind((host,port))

    # 소켓을 블로킹 모드로 설정
    server_socket.setblocking(True)

    # 클라이언트 연결을 대기합니다.
    server_socket.listen()

    # 받은 데이터 저장소
    receivedData = ""

    

    # client_socket, client_addr = server_socket.accept()
    # print(f"클라이언트가 연결되었습니다. 주소: {client_addr}")
    
    while True:
        try:
            # 대기 중
            print("대기중")
            client_socket, client_addr = server_socket.accept()
            print(f"클라이언트가 연결되었습니다. 주소: {client_addr}")
            break  # 클라이언트가 연결되면 루프 종료
        except BlockingIOError:
            print(".", end="", flush=True)  # 1초 간격으로 점 출력
            time.sleep(1)
        except Exception as e:
            print(f"예외 발생: {e}")

    while True:

        # 클라이언트로부터 데이터를 받습니다.
        data = client_socket.recv(1024)

        
        if not data and data == temp:
            continue
        elif data == "bye!":
            break
        else:
            temp = data
            receivedData = data.decode('utf-8')
            
            print(receivedData)

            # 이미지 읽어오기 (예외 처리 추가)
        receiveImgN = None
        failcounter = 0
        while receiveImgN is None:
            receiveImgN = cv2.imread(receivedData + "N.png")
            failcounter = failcounter + 1
            if failcounter > 2:
                break
        if failcounter > 2:
            continue

        # 나머지 이미지도 동일하게 처리합니다.
        receiveImgS = None
        failcounter = 0
        while receiveImgS is None:
            receiveImgS = cv2.imread(receivedData + "S.png")
            failcounter = failcounter + 1
            if failcounter > 2:
                break
        if failcounter > 2:
            continue

        receiveImgE = None
        failcounter = 0
        while receiveImgE is None:
            receiveImgE = cv2.imread(receivedData + "E.png")
            failcounter = failcounter + 1
            if failcounter > 2:
                break
        if failcounter > 2:
            continue

        receiveImgW = None
        failcounter = 0
        while receiveImgW is None:
            receiveImgW = cv2.imread(receivedData + "W.png")
            failcounter = failcounter + 1
            if failcounter > 2:
                break
        if failcounter > 2:
            continue
        
        Image, imagesize = Aroundview(receiveImgN,receiveImgW,receiveImgS,receiveImgE)

        # Airline으로 detection된 선의 수
        linenum=0

        #딕셔너리 value 값을 저장하기 위한 리스트 정의 및 초기화
        msg = dict()
        degree_msg = []
        distance_msg = []
        cls_msg = []
        point_msg = []

        # YOLO 수행 및 결과 저장
        result_YOLO = yolomodel(Image,conf=0.1, iou=0.6)
        # 바운딩 박스들 가져오기
        boxes = result_YOLO[0].boxes

        boundary_obstacle_list = []
        boundary_list = [] # yolo 박스 좌표들을 담는 리스트 [x1 y1 x2 y2]

        for box in boxes:
            list_plus = True
            box_horizontal = False
            # xyxy형태로 가져오기
            bounding_box = box.xyxy[0]
            box_width = (bounding_box[2]-bounding_box[0])*RESIZE_PARAMETER
            box_height = (bounding_box[3]-bounding_box[1])*RESIZE_PARAMETER
            if box_height < box_width:
                box_horizontal = True
            area = box_height*box_width
            cls = box.cls
            if len(boundary_list) == 0:
                pass
            else:
                if cls == 0 or cls == 2 or cls == 4:
                    for box024 in boundary_list:
                        if box024[4] == cls and boundary_overlap(box024, bounding_box) and box024[5] == box_horizontal:
                            if box024[6] <= area:
                                boundary_list.remove(box024)
                            else:
                                list_plus = False
                                pass
                else:
                    pass
            # 바운딩 박스 저장
            if cls == 0 or cls == 2 or cls == 4:
                if list_plus:
                    boundary_list.append([int(RESIZE_PARAMETER * bounding_box[0]), int(RESIZE_PARAMETER * bounding_box[1]), int(RESIZE_PARAMETER * bounding_box[2]), int(RESIZE_PARAMETER * bounding_box[3]), cls, box_horizontal,area]) # 비율에 맞게 좌표 append
            else :
                boundary_obstacle_list.append([int(RESIZE_PARAMETER * bounding_box[0]), int(RESIZE_PARAMETER * bounding_box[1]), int(RESIZE_PARAMETER * bounding_box[2]), int(RESIZE_PARAMETER * bounding_box[3]), cls, box_horizontal, area]) # 비율에 맞게 좌표 append

         # 이미지 대비
        lab = cv2.cvtColor(Image, cv2.COLOR_BGR2LAB)
        l, a, b = cv2.split(lab)
        clahe = cv2.createCLAHE(clipLimit=2.0,tileGridSize=(8,8))
        l = clahe.apply(l)
        lab = cv2.merge((l,a,b))
        cont_dst = cv2.cvtColor(lab, cv2.COLOR_LAB2BGR)
        Image = cont_dst

        Image=cv2.resize(Image,dsize=(0,0),fx=RESIZE_PARAMETER,fy=RESIZE_PARAMETER)

        res = 16
        dscale = 1
        # 이미지 크기 16의 배수로 맞추는 작업
        Image=cv2.resize(Image,(Image.shape[1]//dscale//res*res,Image.shape[0]//dscale//res*res))

        if len(Image.shape)==2:
            Image=cv2.cvtColor(Image,cv2.COLOR_GRAY2RGB)
        elif Image.shape[2]==3:
            pass
        elif Image.shape[2]==4:
            Image=cv2.cvtColor(Image,cv2.COLOR_RGBA2RGB)

        # 배열을 연속된 배열로 변환
        Image=np.ascontiguousarray(Image)

        # Airline용 Image
        Image_airline=Image

        # Image to tensor
        Image_airline = torch.tensor(Image_airline).cuda()/255

        Image_airline=Image_airline.permute(2,0,1)
        lineDetection1=Premodel(Image_airline.unsqueeze(0))
        THETADes=OrientationDetector(lineDetection1)
        imggradient=torch.cat([Image_airline.unsqueeze(0)-Image_airline.roll(1,1).unsqueeze(0),Image_airline.unsqueeze(0)-Image_airline.roll(0,1).unsqueeze(0)])
        THETADes=torch.nn.functional.normalize(THETADes-THETADes.mean(1), p=2.0, dim=1)


        edgeNp=lineDetection1.detach().cpu().numpy()[0,0]

        outMap=np.zeros_like(edgeNp,dtype=np.uint8)
        outMap=np.expand_dims(outMap,2).repeat(3,2)
        out=np.zeros((30000,2,2),dtype=np.float32)
        tempMem=np.zeros((50000,2),dtype=np.int32) #buffer needed for C++ program
        tempMem2=np.zeros((2,300000,2),dtype=np.int32) #buffer needed for C++ program
        tempMem3=np.zeros((30000,2,2),dtype=np.float32)
        mask=lineDetection1[:,0]>0.1

        
        edgeNp=(edgeNp>0.5).astype(np.uint8)*255#1.5

        # linenum=crg.desGrow(outMap,edgeNp,THETADes[0].detach().cpu().numpy(),out,0.94,40,tempMem,tempMem2,tempMem3,THETARESOLUTION)#lineDetection1.detach().cpu().numpy()[0,0]
        linenum=crg.desGrow(outMap,edgeNp,THETADes[0].detach().cpu().numpy(),out,0.9,10,tempMem,tempMem2,tempMem3,THETARESOLUTION)#lineDetection1.detach().cpu().numpy()[0,0]

        puredetection=np.zeros_like(Image[:,:,0],dtype=np.uint8)

        # 그레이스케일 체크 부분
        # Image=cv2.cvtColor(Image,cv2.COLOR_RGB2GRAY)
        # Image=cv2.cvtColor(Image,cv2.COLOR_GRAY2RGB)
        
        out=(out).astype(np.int32)
        Image_airline=Image_airline.detach().cpu().numpy()*255
        Image_airline=Image_airline.astype(np.uint8)

        for boundary in boundary_list: # 한 프레임에 나온 바운더리들
            height = abs(boundary[1] - boundary[3]) # 박스 길이
            width = abs(boundary[0] - boundary[2]) # 박스 너비
            boundary_center = [(boundary[0]+boundary[2])/2,(boundary[1]+boundary[3])/2] #박스 중심좌표

            point_list = [] #각 선들이 시작점, 끝점 저장위한 리스트

            horizontal = False # 박스 비율 체크
            # 기준 길이 정하기 if문
            standard_length = 0 # 기준 길이
            if height > width: 
                if boundary[2] < imagesize*0.02 or boundary[0] > imagesize*0.98:
                    continue
                standard_length = height
            else:
                if boundary[1] < imagesize*0.02 or boundary[3] > imagesize*0.98:
                    continue
                standard_length = width
                horizontal = True

            if boundary[4] == 0:
                color = COLOR_SKY
            elif boundary[4] == 2:
                color = COLOR_YELLOW
            else:
                color = COLOR_RED

            y_list = []    # 바운더리 안에 들어가는 y좌표만 넣을 리스트
            x_list = []    # 바운더리 안에 들어가는 x좌표만 넣을 리스트
            
            #curve fit 함수의 결과(기울기, y절편)를 제한하기 위한 bounds 리스트
            bounds = [[float('-inf'),float('-inf')],[float('inf'),float('inf')]]
            #bounds를 계산하기 위한 절사평균을 만들기 위한 각도 리스트
            inc_degree_list = []
            inc_degree_list_all = []

            for i in range(linenum):
                length = np.sqrt((out[i, 0, 0] - out[i, 1, 0]) ** 2 + (out[i, 0, 1] - out[i, 1, 1]) ** 2) # 선 길이 측정
                if length > standard_length//LENGTH_RATIO: # 일정 길이 미만 선 거름
                    if boundary_check(boundary, out[i]): # yolo 박스 안에 있으면
                        inc = cal_inclination(out[i]) # 기울기 계산
                        #if (not horizontal and abs(inc) > abs(height / width)) or (horizontal and abs(inc) < abs(height / width)):
                        y_inter = cal_interceptor(inc, out[i, 0, 1], out[i, 0, 0])
                        distance = distance_between_point_line(inc, y_inter, boundary_center[0], boundary_center[1])
                        #박스 중심과 airline선 사이의 거리가 일정 이상인 선들은 빼기
                        if distance > width*0.2 or distance > height*0.2:
                            continue
                        # curve fitting 용 점 저장
                        x_list.append(out[i, 0, 1])
                        x_list.append(out[i, 1, 1])
                        y_list.append(out[i, 0, 0])
                        y_list.append(out[i, 1, 0])

                        # 점 증강 및 선의 각도를 inc_degree_list에 저장(일정 길이 이상인 선들 기준)
                        if length > standard_length//6:
                            inc_degree = math.degrees(math.atan2(inc, 1))   #기울기를 각도로 변환
                            if not horizontal:                                  #조건에 따라 음수 각도를 양수로 바꿔주기
                                if inc_degree <= 0:
                                    inc_degree += 180
                            inc_degree_list.append(inc_degree)
                            #일정한 간격으로 점 증강시키기
                            for boundary_step in np.arange(boundary[0], boundary[2], 0.2):
                                tempX = boundary_step
                                tempY = tempX * inc + y_inter
                                if boundary[1] <= tempY <= boundary[3]:
                                    x_list.append(tempX)
                                    y_list.append(tempY)

                #점 개수가 10개 미만이면 패스
            if len(y_list) < 10:
                continue
            #각도의 절사평균 구하기
            inc_degree_list.sort()
            inc_degree_trim = stats.trim_mean(inc_degree_list, 0.2) #0.2는 전체 길이 중 얼마나 잘라낼것인지 0.2라면 좌우 20%씩
            #조건에 따라 각도를 음수로 바꿔주기
            if not horizontal:
                if inc_degree_trim > 90:
                    inc_degree_trim = inc_degree_trim-180

            #각도 값을 다시 기울기로 변환
            inc_trim = math.tan(math.pi * inc_degree_trim/180)

            if inc_trim == 0 or len(inc_degree_list)==0:   #기울기가 0이면 bounds로 제한하지 않기
                popt, _ = curve_fit(return_y, x_list, y_list)
            else:               #기울기가 0이 아닐경우 bounds로 제한하기
                boundLate = 0.1
                if inc_trim < 0:
                    bounds = [[inc_trim * (1 + boundLate), float('-inf')], [inc_trim * (1 - boundLate), float('inf')]]
                else:
                    bounds = [[inc_trim * (1 - boundLate), float('-inf')], [inc_trim * (1 + boundLate), float('inf')]]
                popt, _ = curve_fit(return_y, x_list, y_list, bounds=bounds)  # curve fitting으로 하나의 선 찾기

            inc_mean = popt[0]
            y_mean = popt[1]

            if horizontal:
                cv2.line(Image, (int(boundary[0]), int(return_y(boundary[0], inc_mean, y_mean))),
                         (int(boundary[2]), int(return_y(boundary[2], inc_mean, y_mean))), color, 2)
                point_list = [[boundary[0], return_y(boundary[0], inc_mean, y_mean)],
                              [boundary[2], return_y(boundary[2], inc_mean, y_mean)]]
            elif not horizontal:
                cv2.line(Image, (int(return_x(boundary[1], inc_mean, y_mean)), int(boundary[1])),
                         (int(return_x(boundary[3], inc_mean, y_mean)), int(boundary[3])), color, 2)
                point_list = [[return_x(boundary[1], inc_mean, y_mean), boundary[1]],
                              [return_x(boundary[3], inc_mean, y_mean), boundary[3]]]

            degree = math.degrees(math.atan2(inc_mean, 1))
            # 각도 이미지에 출력 부분
            # if int(boundary[4]) == 0:
            #     cv2.line(Image, (30,20), (40,20), COLOR_SKY, 2)
            #     if not horizontal:
            #         cv2.putText(Image, str(round(degree, 4)), (30, 40), FONT, 1, FONT_COLOR, 1, cv2.LINE_AA)  # 각도 이미지에 출력
            #     else:
            #         cv2.putText(Image, str(round(degree, 4)), (30, 60), FONT, 1, FONT_COLOR, 1,cv2.LINE_AA)  # 각도 이미지에 출력
            # elif int(boundary[4]) == 4:
            #     cv2.line(Image, (30, 70), (40, 70), COLOR_RED, 2)
            #     if not horizontal:
            #         cv2.putText(Image, str(round(degree, 4)), (30, 90), FONT, 1, FONT_COLOR, 1, cv2.LINE_AA)  # 각도 이미지에 출력
            #     else:
            #         cv2.putText(Image, str(round(degree, 4)), (30, 110), FONT, 1, FONT_COLOR, 1,cv2.LINE_AA)  # 각도 이미지에 출력

            #이미지의 중심에서 선까지 거리 Cm단위로 변환
            Image_height, Image_width, _ = Image.shape
            distance_line = distance_between_point_line(inc_mean, y_mean, Image_width / 2, Image_height / 2)
            distanceToCm = round(distance_line * 2500 / 579)

            degree_msg.append(degree)                       # 기준선의 각도
            distance_msg.append(distanceToCm)               # 이미지 중심과 기준선 사이의 거리 -> Cm로 변환
            cls_msg.append(YOLO_CLASS[int(boundary[4])])    # 바운더리박스 클래스
            point_msg.append(point_list)                    # 선의 시작점과 끝점 좌표

        #각도, 거리, 타입, 점 딕셔너리
        msg = {'degree': degree_msg, 'distance': distance_msg, 'cls': cls_msg, "point": point_msg}

        image_list = receivedData.split("Image")
        
        number = (image_list[-1])
        pathstring = "C:/Users/user/UnityGraduate/PythonStream/test" + number
        sendstring = "C:/Users/user/UnityGraduate/PythonStream/test" + number +".png"

                # 텍스트 파일에 데이터 쓰기
        with open(pathstring +".txt", 'w') as file:
            for key, value in msg.items():
                file.write(f'{key}: {value}\n')

        control_signal = 1

        send_data = {
            "image_path" : sendstring,
            "control_signal" : control_signal
        }

        json_data = json.dumps(send_data)

        cv2.imwrite(sendstring,Image)
        #client_socket.sendall(json_data.encode())
        client_socket.send(pathstring.encode('utf-8'))

        

 