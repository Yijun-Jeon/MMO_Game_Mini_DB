using Google.Protobuf.Protocol;
using Microsoft.EntityFrameworkCore;
using Server.DB;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Server
{
    // PreGame - Game에 들어가기 이전 상태 관리
    public partial class ClientSession : PacketSession
    {
        public void HandlerLogin(C_Login loginPacket)
        {
			// TODO : 이런 저런 보안 체크
			if (ServerState != PlayerServerState.ServerStateLogin)
				return;

			// 유효한 아이디인지 확인
			using (AppDbContext db = new AppDbContext())
			{
				AccountDb findAccount = db.Accounts
					.Include(a => a.Players)
					.Where(a => a.AccountName == loginPacket.UniqueId).FirstOrDefault();

				if (findAccount != null)
				{
					S_Login loginOk = new S_Login() { LoginOk = 1 };
					Send(loginOk);
				}
				else
				{
					// 아이디가 없으면 생성해 줌
					AccountDb newAccount = new AccountDb() { AccountName = loginPacket.UniqueId };
					db.Accounts.Add(newAccount);
					db.SaveChanges(); // ToDO : Exception

					S_Login loginOk = new S_Login() { LoginOk = 1 };
					Send(loginOk);
				}
			}
		}
    }
}
