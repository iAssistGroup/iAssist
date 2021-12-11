using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace iAssist.Models
{
    [Authorize]
    public class RateandFeedback
    {
        public int WorkerId { get; set; }
        [Required]
        [Range(1d, 5d, ErrorMessage = "{0} must be in the range 1..5")]
        [RegularExpression("([1-9][0-9]*)", ErrorMessage = "Rate must be a natural number")]
        public int Rate { get; set; }
        [StringLength(4096, MinimumLength = 1, ErrorMessage = "{0} length must be in the range 1..4096")]
        [Display(Name = "Review message")]
        public string Feedback { get; set; }
        public string Username { get; set; }
        public int? taskid { get; set; }
        public int jobid { get; set; }
    }
}