using AccountServer.DB;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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

        public AccountController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        [Route("create")]
        public void CreateAccount([FromBody] CreateAccountPacketReq req)
        {

        }

        [HttpPost]
        [Route("login")]
        public void LoginAccount()
        {

        }

    }
}
