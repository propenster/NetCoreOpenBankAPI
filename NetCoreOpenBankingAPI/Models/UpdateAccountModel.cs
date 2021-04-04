using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace NetCoreOpenBankingAPI.Models
{
    public class UpdateAccountModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        [RegularExpression(@"^[0-9]{4}$/", ErrorMessage = "")]
        public string Pin { get; set; }

    }
}
