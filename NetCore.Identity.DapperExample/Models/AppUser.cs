using Identity.Dapper.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetCore.Identity.DapperExample.Models
{
    public class AppUser : DapperIdentityUser
    {
        public AppUser()
        {
            
        }
        public bool IsActive { get; set; }

        public AppUser(string userName) : base(userName) { }
    }
}
