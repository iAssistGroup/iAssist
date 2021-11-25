using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace iAssist.Models
{
    public class Job
    {
        public int Id { get; set; }
        [Required]
        public string JobName { get; set; }
        [Required]
        public string JobDescription { get; set; }
        public DateTime Created_At { get; set; }
        public DateTime Updated_At { get; set; }
    }
}