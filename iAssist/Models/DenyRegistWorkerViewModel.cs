using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace iAssist.Models
{
    public class DenyRegistWorkerViewModel
    {
        public string UserId { get; set; }
        [Required]
        [StringLength(4096, MinimumLength = 30, ErrorMessage = "{0} length must be in the range 30..4096")]
        public string Feedback { get; set; }
    }
}