using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace iAssist.Models
{
    public class ServiceViewModel
    {
        public int Id { get; set; }
        public int Jobid { get; set; }
        [Required]
        [Display(Name = "Service Name")]
        public string Skillname { get; set; }
    }
}