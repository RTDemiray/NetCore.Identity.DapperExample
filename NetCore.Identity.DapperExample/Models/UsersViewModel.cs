using Identity.Dapper.Entities;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace NetCore.Identity.DapperExample.Models
{
    public class UsersViewModel
    {
        public int Id { get; set; }
        [DisplayName("Kullanıcı adı")]
        [Required(ErrorMessage = "{0} alanı boş geçilemez!")]
        public string UserName { get; set; }

        [DisplayName("E-Mail")]
        [Required(ErrorMessage = "{0} alanı boş geçilemez!")]
        public string Email { get; set; }

        [DisplayName("Şifre")]
        [DataType(DataType.Password)]
        [Required(ErrorMessage = "{0} alanı boş geçilemez!")]
        public string Password { get; set; }

        [DisplayName("Şifre tekrar")]
        [DataType(DataType.Password)]
        [Required(ErrorMessage = "{0} alanı boş geçilemez!")]
        [Compare(nameof(Password),ErrorMessage = "Şifreler uyuşmuyor!")]
        public string PasswordConfirm { get; set; }

        public List<DapperIdentityRole> UserRole { get; set; }

        public string[] RoleName { get; set; }

        public int RoleId { get; set; }

        public bool IsActive { get; set; }

        public List<IdentityRoles> IdentityRoles { get; set; }
    }
}
