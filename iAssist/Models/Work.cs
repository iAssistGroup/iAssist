using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace iAssist.Models
{
    public class Work
    {
        public int Id { get; set; }
        [Required]
        public string worker_overview { get; set; }
        [Required]
        public int worker_status { get; set; }
        [Required]
        public DateTime Created_At { get; set; }
        [Required]
        public DateTime Updated_At { get; set; }
        public string Verified_At { get; set; }
        public virtual ApplicationUser User { get; set; }
        public string Userid { get; set; }
        public virtual Job job { get; set; }
        public int JobId { get; set; }

    }
}