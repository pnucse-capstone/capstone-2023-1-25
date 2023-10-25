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

### 3.3. 도장 로봇의 제어 신호 처리 과정

<img width="480" alt="image" src="https://github.com/pnucse-capstone/capstone-2023-1-25/assets/48705640/72ea2614-d771-4025-aaeb-6b4b100e5cbe">


## 4. 소개 및 시연 영상

[![영상 넣을곳](https://img.youtube.com/vi/eXbTZrWUw1k/0.jpg)](https://www.youtube.com/watch?v=eXbTZrWUw1k)

## 5. 설치 및 사용법

본 프로젝트는 Ubuntu 20.04 버전에서 개발되었으며 함께 포함된 다음의 스크립트를 수행하여 
관련 패키지들의 설치와 빌드를 수행할 수 있습니다.
```
$ ./install_and_build.sh
```
