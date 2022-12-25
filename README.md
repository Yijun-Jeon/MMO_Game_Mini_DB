# MMO_Game_Mini_DB
<img src="https://user-images.githubusercontent.com/89140546/203554531-c7bd8978-1f70-4a77-8940-1bec449af4ae.png" width="30%" height="30%">

## 프로젝트 개요
> Inflearn - '[C#과 유니티로 만드는 MMORPG 게임 개발 시리즈] Part9: MMO 컨텐츠 구현 (DB연동 + 대형 구조 + 라이브 준비)'를 참고한 프로젝트

* 클라이언트 : Unity 2021.3.13f, C#
* 서버 : .Net Core 3.1, C#

## 계정생성 & 로그인
![](https://i.imgur.com/VozIXD2.gif)

* 'Create new user' 버튼을 누르면 DB에 새로운 계정이 생성된다.
* DB에 저장되있는 계정의 Id와 Password를 입력하고 '로그인' 버튼을 누르면 서버를 고를 수 있고 고른 서버의 주소로 입장하게 된다. 

## 인게임
<img src="https://velog.velcdn.com/images/gkqls813/post/b67a2849-efe8-4f3a-93a4-bbe33a5a0b56/image.gif" width="100%" height="100%">

* 플레이어와 몬스터들은 모두 재접속하거나 죽으면 랜덤 위치로 리스폰된다.
* 스페이스바를 누르면 화살을 날릴 수 있으며 몬스터를 한 방에, 다른 플레이어는 여러 방에 죽일 수 있다.
* 몬스터들은 A* 알고리즘을 통해 자동으로 플레이어를 찾아서 공격한다.

---

# Reference
* https://www.inflearn.com/course/%EC%9C%A0%EB%8B%88%ED%8B%B0-mmorpg-%EA%B0%9C%EB%B0%9C-part9
