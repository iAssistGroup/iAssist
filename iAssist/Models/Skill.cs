using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace iAssist.Models
{
    public class Skill
    {
        public int Id { get; set; }
        public string Skillname { get; set; }
        public int Jobid { get; set; }
        public virtual Job Job { get; set; }
    }
}