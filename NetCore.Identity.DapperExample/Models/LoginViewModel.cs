using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace NetCore.Identity.DapperExample.Models
{
    public class LoginViewModel
    {
        [DisplayName("Kullanıcı adı")]
        [Required(ErrorMessage = "{0} alanı boş geçilemez!")]
        public string UserName { get; set; }

        [DisplayName("Şifre")]
        [DataType(DataType.Password)]
        [Required(ErrorMessage = "{0} alanı boş geçilemez!")]
        public string Password { get; set; }

        [DisplayName("Beni hatırla?")]
        public bool RememberMe { get; set; }
    }
}
