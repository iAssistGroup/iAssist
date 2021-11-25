using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace iAssist.Models
{
    public class ComplainViews
    {
        [Required]
        [Display(Name ="Complain Type / Title")]
        public string ComplainType { get; set; }
        [Required]
        [Display(Name ="Explain Why you report him/her")]
        public string Description { get; set; }
        public string image { get; set; }
        public HttpPostedFileBase ImageFile { get; set; }
        public int Workerid { get; set; }



    }
}