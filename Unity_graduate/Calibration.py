def test():
    from tkinter import messagebox
    messagebox.showinfo("알림창","딸랑딸랑")

def test2(msg):
    from tkinter import messagebox
    messagebox.showinfo("알림창",msg)

#test해보고 싶으면 주석 해제
#test()

import socket
import cv2
import atexit
import imutils as imutils
import numpy as np
import time


count = 0

teststring = "hi_there?"
temp = ""
receivedData = ""

def exit_handler():
    exitmsg = "bye bye!"
    test2(exitmsg)

    sock.close()
    


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


atexit.register(exit_handler)



host, port = "127.0.0.1", 25001
sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
sock.connect((host,port))

sock.setblocking(True)

sock.sendall(teststring.encode("UTF-8"))

while True:
    receivedData = sock.recv(1024).decode("UTF-8")
    #test2(int(receivedData[-5]))
    if receivedData != "" and temp!= receivedData:

        #time.sleep(0.1)

        receiveImgN = cv2.imread(receivedData + "N.png")
        receiveImgS = cv2.imread(receivedData + "S.png")
        receiveImgE = cv2.imread(receivedData + "E.png")
        receiveImgW = cv2.imread(receivedData + "W.png")

        #if(receiveImgE == None): test2("E")
        #if(receiveImgN == None): test2("N")
        #if(receiveImgS == None): test2("S")
        #if(receiveImgW == None): test2("W")

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
        inn = 18
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
        white_color = (255,255,255)
        mask = np.zeros((600,600,3),dtype = np.uint8)
        pt1 = np.array([[0,0],[300,240],[600,0],[360,300],[600,600],[300,360],[0,600],[240,300]], np.int32)
        mask = cv2.polylines(mask,[pt1], True, white_color)

        white_star = cv2.fillPoly(mask, [pt1], white_color)
        black_star = cv2.bitwise_not(white_star)
        #cv2.imshow('white', white_star)
        #cv2.imshow('black', black_star)

        dup_WE = cv2.bitwise_and(background_WE, white_star)
        dup_NS = cv2.bitwise_and(background_NS, white_star)

        #cv2.imshow('dup_WE', dup_WE)
        #cv2.imshow('dup_NS', dup_NS)

        # 가중치
        alpha = 0.5
        star = cv2.addWeighted(dup_WE, alpha, dup_NS, (1-alpha), 0)
        #cv2.imshow('star', star)

        # 겹치는 부분 제외
        x_star = cv2.bitwise_and(b, black_star)
        #cv2.imshow('x_star', x_star)

        # 기존 결과
        #cv2.imshow('b', b)

        # 가중치 합성결과
        dst_NS = cv2.bitwise_or(dup_NS, x_star)
        dst_WE = cv2.bitwise_or(dup_WE, x_star)

        # Fill 0 values in dst_NS with corresponding values from dst_WE
        dst_NS[dst_NS == 0] = dst_WE[dst_NS == 0]

        sendstring = "C:/Users/user/UnityGraduate/PythonStream/test" + str(count) +".png"

        cv2.imwrite(sendstring,dst_NS)
        sock.sendall(sendstring.encode("UTF-8"))
        temp = receivedData
        count = count + 1
    else :
        sock.sendall("same!!!".encode("UTF-8"))
