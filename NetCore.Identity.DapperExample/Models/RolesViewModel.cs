using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace NetCore.Identity.DapperExample.Models
{
    public class RolesViewModel
    {
        public int Id { get; set; }
        [DisplayName("Rol adı")]
        [Required(ErrorMessage = "{0} alanı boş geçilemez!")]
        public string Name { get; set; }

        public bool IsActive { get; set; }
    }
}
