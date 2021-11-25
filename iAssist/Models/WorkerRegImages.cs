using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace iAssist.Models
{
    public class WorkerRegImages
    {
        public int Id { get; set; }
        [Required]
        public string FileName { get; set; }
        public virtual ApplicationUser User { get; set; }
        [Required]
        public string Userid { get; set; }
        public int Jobid { get; set; }
        public virtual Job Job { get; set; }
    }
}