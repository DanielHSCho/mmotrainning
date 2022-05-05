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
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        AppDbContext _context;
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

            AccountDb account = _context.Accounts
                .AsNoTracking()
                .Where(a => a.AccountName == req.AccountName)
                .FirstOrDefault();

            // 계정 생성 가능
            if(account == null) {
                _context.Accounts.Add(new AccountDb() {
                    AccountName = req.AccountName,
                    // TODO : 요청보낸걸 이부분에서 암호화 해서 저장해야함
                    Password = req.Password
                });
                bool success = _context.SaveChangesEx();
                res.CreateOk = success;
            } else {
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

            if(account == null) {
                res.LoginOk = false;
            } else {
                res.LoginOk = true;

                

                // TODO : 서버 목록
                res.ServerList = new List<ServerInfo>() {
                    new ServerInfo(){Name = "헬레나", Ip = "127.0.0.1", CrowdedLevel = 0},
                    new ServerInfo(){Name = "다니엘", Ip = "127.0.0.1", CrowdedLevel = 3}
                };
            }

            return res;
        }

    }
}
