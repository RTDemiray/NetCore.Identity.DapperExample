using Identity.Dapper.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetCore.Identity.DapperExample.Models
{
    [Dapper.Contrib.Extensions.Table("IdentityRole")]
    public class IdentityRoles
    {
        [Dapper.Contrib.Extensions.ExplicitKey]
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
    }
}
