using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetCore.Identity.DapperExample.Models
{
    [Table("IdentityUser")]
    public class UsersWithRoles
    {
        [ExplicitKey]
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public List<IdentityRoles> UserRoles { get; set; }
    }
}
