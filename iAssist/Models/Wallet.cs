using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace iAssist.Models
{
    public class Wallet
    {
        public int Id { get; set; }

        [Required]
        [DataType(DataType.Currency)]
        public decimal Money { get; set; }

        [Required] public string UserId { get; set; }

        public ApplicationUser User { get; set; }
    }
}