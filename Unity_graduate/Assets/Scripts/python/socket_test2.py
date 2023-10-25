import socket
import cv2
import time
import pickle

testImg = cv2.imread('./image/red3.jpg')
resizeImg = cv2.resize(testImg,(550,719))
host, port = "127.0.0.1", 25001
sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
sock.connect((host, port))

while True:
    time.sleep(0.5)
    retval, frame = cv2.imencode('.jpg',resizeImg,[cv2.IMWRITE_JPEG_QUALITY, 90])
    frame = pickle.dumps(frame)

    print(frame)

    sock.sendall(frame)

    receivedData = sock.recv(1024).decode("UTF-8")
    print(receivedData)