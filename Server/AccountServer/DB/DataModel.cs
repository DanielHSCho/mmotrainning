using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace AccountServer.DB
{
    [Table("Account")]
    public class AccountDb
    {
        public int AccountDbId { get; set; }
        public string AccountName { get; set; }

        // TODO : 이메일 주소 / 이메일 인증 여부 / 비밀번호는 해쉬로
        public string Password { get; set; }
    }

}
