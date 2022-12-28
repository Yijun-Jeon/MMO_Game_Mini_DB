# MMO_Game_Mini_DB
<img src="https://user-images.githubusercontent.com/89140546/203554531-c7bd8978-1f70-4a77-8940-1bec449af4ae.png" width="30%" height="30%">

## 프로젝트 개요
> Inflearn - '[C#과 유니티로 만드는 MMORPG 게임 개발 시리즈] Part9: MMO 컨텐츠 구현 (DB연동 + 대형 구조 + 라이브 준비)'를 참고한 프로젝트

* 클라이언트 : Unity 2021.3.13f, C#
* 서버 : .Net Core 3.1, C#
* DB : EFCore

## 계정생성 & 로그인
<img src="https://user-images.githubusercontent.com/89140546/209460831-009dbd04-7c46-4522-a27e-baea2cba4a13.gif" width="50%" height="50%">

* `Create new user` 버튼을 누르면 DB에 새로운 계정이 생성된다.
* DB에 저장되있는 계정의 Id와 Password를 입력하고 `로그인` 버튼을 누르면 서버를 고를 수 있고 고른 서버의 주소로 입장하게 된다. 

## 인게임
<img src="https://user-images.githubusercontent.com/89140546/209460949-6ae7a0b4-db5f-4d6d-b78e-f4db16a2b370.gif" width="50%" height="50%">

* 플레이어와 몬스터들은 모두 재접속하거나 죽으면 랜덤 위치로 리스폰된다.
* `스페이스바` 를 누르면 화살을 날릴 수 있으며 몬스터를 한 방에, 다른 플레이어는 여러 방에 죽일 수 있다.
* 몬스터들은 `A* 알고리즘`을 통해 자동으로 플레이어를 찾아서 공격한다.

<img src="https://user-images.githubusercontent.com/89140546/209460833-a899b7d3-3910-4e41-894a-ced634469362.gif" width="50%" height="50%">

* `C` 를 누르면 플레이어의 아이템 착용 정보와 스탯 창을 볼 수 있다.
  * 플레이어의 스탯은 레벨과 아이탬의 스탯과 연동된다.
* `I` 를 누르면 플레이어가 보유한 아이템들이 있는 인벤토리창을 볼 수 있다.
  * 아이템은 몬스터를 잡았을 때 랜덤 확률로 얻을 수 있다.

---

# Reference
* https://www.inflearn.com/course/%EC%9C%A0%EB%8B%88%ED%8B%B0-mmorpg-%EA%B0%9C%EB%B0%9C-part9
