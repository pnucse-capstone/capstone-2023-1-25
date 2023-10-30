# OpenCV/CNN기반 페인팅 로봇 작업경로 생성

## 1. 프로젝트 소개

* 최근 조선업계는 기술자의 부족으로 어려움을 겪고 있다. 이 문제를 해결하기 위해 조선 3사는 자동화 기술을 도입하고 있으며, 스마트 야드 개념을 도입하여 선박 생산과 관리 영역에 IT 기술을 활용하고 있지만 도장 작업의 자동화 비율은 아직 낮은 상태다.

* 이 같은 상황에 도장 작업이 무인화를 이룬다면 작업 효율성 증대, 작업 안정성 강화 및 자동화를 통한 산업재해 감소 또한 기대해 볼 수 있다.

* 따라서 본 과제에서는 도장 작업의 자동화를 위해 비전 인식과 보정 기술을 이용한 블록 하부 도장 작업 자동화를 진행해 보고자 한다.

* 이 연구의 목표는 RGB 카메라 영상을 딥러닝 모델과 OpenCV를 활용하여 하부 도장 로봇의 작업에 필요한 제어 신호를 생성하고 Unity로 시뮬레이터를 구현해 적용해 봄으로써 실제 도장 작업에 사용할 수 있는 해결 방안을 제시하는 것을 목표로 한다.

## 2. 팀소개

|이름|이메일|역할|
|---|---|------|
| 김정호 |201824451@pusan.ac.kr| • Unity 시뮬레이션 구현 </br> • 소켓 통신 구현 </br> • Unity 신호 제어 구현|
| 정제영 |aia1235@pusan.ac.kr| • 데이터 수집 및 전처리 </br> • YOLO 모델 학습 및 적용 </br> • YOLO 데이터 증강 및 성능 개선|
| 최성렬 |littie123@naver.com| • Around view 구현 </br> • Edge detection 모델 적용 및 성능 개선 </br> • 직선 검출 알고리즘 구현|

## 3. 시스템 구성도

### 3.1. 전체 시스템 구성도

<img width="480" alt="image" src="https://github.com/pnucse-capstone/capstone-2023-1-25/assets/48705640/4167b124-ae19-4a86-95cf-91e6d54292cd">

<br>

 - Unity와 Python의 소켓 통신을 기반으로 한다.

<br>

### 3.2. 하부 도장 영상 처리 과정

<img width="600" alt="image" src="https://github.com/pnucse-capstone/capstone-2023-1-25/assets/48705640/69db59a0-b2e6-404b-9678-8bdb5539c85c">

<br><br>

1. 로봇에 있는 4개의 카메라를 이용해 도장면 하부를 촬영한다.
2. 상하좌우 4개의 이미지를 각각 Calibration한다.
3. 4개의 이미지를 하나의 이미지로 Stitching 한다.
4. 직선 검출을 위해 이미지에 Airline Model을 적용한다.
5. 제어 신호의 필요한 직선만 검출하기 위해 YOLOv8 모델을 적용한다.
6. YOLO의 bounding box를 기반으로 기준선을 탐지한다.
7. 정확한 직선 검출을 위해 직선의 noise를 제거한다.
8. 도장 로봇 제어에 필요한 정보들을 출력한다.

<br>

### 3.3. 도장 로봇 유한 상태 머신

<img width="480" alt="image" src="https://github.com/pnucse-capstone/capstone-2023-1-25/assets/48705640/72ea2614-d771-4025-aaeb-6b4b100e5cbe">


## 4. 소개 및 시연 영상

[![영상 넣을곳](https://img.youtube.com/vi/EavNrpU36Gg/0.jpg)](https://www.youtube.com/watch?v=EavNrpU36Gg)

## 5. 설치 및 사용법

본 프로젝트는 Ubuntu 20.04 버전에서 개발되었으며 함께 포함된 다음의 스크립트를 수행하여 
관련 패키지들의 설치와 빌드를 수행할 수 있습니다.

먼저 폴더를 다음과 같이 구성해야 하며, 빌드 된 Unity 시뮬레이터의 경우 아무 경로에 위치하여도 상관 없습니다.

```
C:
 └── User
      └── 사용자명
              └── UnityGraudate
                        ├── checkpoints
                        │   └── unet.pth
                        ├── UnityStream2
                        ├── PythonStream
                        ├── ultimate.py
                        ├── dexinedmodel.py
                        ├── CRG.pyd
                        └── CRG
                             └── ...
F:
 └── graduate
         └── train15
                └── weights
                        └── best.pt
```
YOLO 모델(best.pt)의 경우 ultimate 의 모델 로딩 부분의 코드를 고쳐 원하는 경로로 변경하여도 상관 없습니다.

```
# YOLO 모델 경로
    yolomodel = YOLO(r'원하는 경로')
```

이미지 프로세싱 서버 역할을 하는 ultimate.py의 실행을 위해서는 requirements.txt를 이용한 파이썬 패키지 설치가 필요합니다.

dexi.pth 모델의 경우 해당 링크에서 다운로드 받을 수 있습니다.
```
https://drive.google.com/drive/folders/11TQxJjpBoZVKcQ0Pmb6GH3h0qQk4CnNe
```
실행 방법은 간단합니다.

1. Unity 프로젝트를 로딩 후 빌드한다. 이 때, Unity 버전은 다음과 같다.(2021.3.19.f1)
2. ultimate.py 를 실행 후 모델이 로딩될 때 까지 기다린다.
3. 빌드 된 Unity 시뮬레이터의 Start 버튼을 클릭한다.

해당 과정을 통하여, YOLO/Airline을 이용하여 이미지 프로세싱을 하고 해당 정보를 통해 자율주행하는 도장로봇을 시뮬레이션 할 수 있습니다.

