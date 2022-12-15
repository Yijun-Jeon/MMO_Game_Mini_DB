using AccountServer.DB;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        public AccountController(AppDbContext context)
        {
            _context = context;
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

                // TODO 서버 목록
                res.ServerList = new List<ServerInfo>()
                { 
                    new ServerInfo() {Name = "데포르쥬", Ip = "127.0.0.1", CrowdedLevel = 0},
                    new ServerInfo() {Name = "아툰", Ip = "127.0.0.1", CrowdedLevel = 3}
                };
            }

            return res;
        }
    }
}
