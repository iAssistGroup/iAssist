using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace iAssist.Models
{
    public class WithDrawRequest
    {
        public int Id { get; set; }
        [Required]
        public decimal Money { get; set; }
        [Required]
        [Display(Name ="Paypal Email")]
        public string Email { get; set; }
        [Required]
        public string Username { get; set; }
        [Required]
        public bool status { get; set; }
        [Required]
        public string UserId { get; set; }
    }
}