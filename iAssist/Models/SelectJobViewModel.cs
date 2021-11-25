using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace iAssist.Models
{
    public class SelectJobViewModel
    {
        [Display(Name = "Select a Job")]
        public int JobId { get; set; }
        public SelectList JobList { get; set; }
    }
}