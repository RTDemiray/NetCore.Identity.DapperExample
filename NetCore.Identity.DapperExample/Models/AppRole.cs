using Identity.Dapper.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetCore.Identity.DapperExample.Models
{
    public class AppRole : DapperIdentityRole
    {
        public AppRole()
        {
           
        }

        public bool IsActive { get; set; }

        public AppRole(string roleName) : base(roleName) { }
    }
}
