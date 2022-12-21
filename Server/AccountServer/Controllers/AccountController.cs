using AccountServer.DB;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AccountServer.Controllers
{
    // 아파트의 동 주소 느낌
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        AppDbContext _context;
        // 공유 DB
        SharedDbContext _shared;

        public AccountController(AppDbContext context, SharedDbContext shared)
        {
            _context = context;
            _shared = shared;
        }

        [HttpPost]
        [Route("create")]
        public CreateAccountPacketRes CreateAccount([FromBody] CreateAccountPacketReq req)
        {
            CreateAccountPacketRes res = new CreateAccountPacketRes();

            // 이름 중복 확인
            AccountDb account = _context.Accounts
                                .AsNoTracking()
                                .Where(a => a.AccountName == req.AccountName)
                                .FirstOrDefault();

            // 생성 가능
            if(account == null)
            {
                _context.Accounts.Add(new AccountDb()
                { 
                    AccountName = req.AccountName,
                    Password = req.Password
                });

                bool sucess = _context.SaveChangesEx();
                res.CreateOk = sucess;
            }
            else
            {
                res.CreateOk = false;
            }

            return res;
        }

        [HttpPost]
        [Route("login")]
        public LoginAccountPacketRes LoginAccount([FromBody] LoginAccountPacketReq req)
        {
            LoginAccountPacketRes res = new LoginAccountPacketRes();

            AccountDb account = _context.Accounts
                .AsNoTracking()
                .Where(a => a.AccountName == req.AccountName && a.Password == req.Password)
                .FirstOrDefault();

            if(account == null)
            {
                res.LoginOk = false;
            }
            else
            {
                res.LoginOk = true;

                // 토큰 발급
                DateTime expired = DateTime.UtcNow; // 절대 시간
                expired.AddSeconds(600); // 600초 후 만료

                TokenDb tokenDb = _shared.Tokens.Where(t => t.AccountDbId == account.AccountDbId).FirstOrDefault();
                // 이미 계정에 발급된 토큰이 있다면 토큰 수정
                if(tokenDb != null)
                {
                    // 랜덤 값으로 토큰 부여
                    tokenDb.Token = new Random().Next(Int32.MinValue, Int32.MaxValue);
                    tokenDb.Expired = expired;
                    _shared.SaveChangesEx();
                }
                else
                {
                    tokenDb = new TokenDb()
                    {
                        AccountDbId = account.AccountDbId,
                        Token = new Random().Next(Int32.MinValue, Int32.MaxValue),
                        Expired = expired
                    };
                    _shared.Add(tokenDb);
                    _shared.SaveChangesEx();
                }

                res.AccountId = account.AccountDbId;
                res.Token = tokenDb.Token;
                res.ServerList = new List<ServerInfo>();

                // 서버 목록 정보
                foreach(ServerDb serverDb in _shared.Servers)
                {
                    res.ServerList.Add(new ServerInfo()
                    {
                        Name = serverDb.Name,
                        IpAddress = serverDb.IpAddress,
                        Port = serverDb.Port,
                        BusyScore = serverDb.BusyScore
                    });
                }
            }

            return res;
        }
    }
}
