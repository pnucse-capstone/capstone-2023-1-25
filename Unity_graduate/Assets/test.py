import cv2
from tkinter import messagebox
#import UnityEngine as ue

# print("Hello, C#!")

# def Start():
#     UnityEngine.Debug.Log("Python script is running!")
# Start()


#messagebox.showinfo("알림창","딸랑딸랑")
test = cv2.imread('C:/Users/user/Documents/GitHub/Unity_graduate/Assets/Color/fukujatsu_shashin.png',cv2.IMREAD_GRAYSCALE)
#test2 = cv2.imread('Color//N.jpg')
#UnityEngine.Debug.Log("imread success!")
#cv2.imshow('test',test)
#cv2.imshow('test',test)
cv2.imwrite('F:/testing/test.png',test)
#cv2.imwrite('Resources/test.png',test2)

# objects = ue.Object.FindObjectsOfType(ue.GameObject)

# for go in objects:
#     ue.Debug.Log(go.name)