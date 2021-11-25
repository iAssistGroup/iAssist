using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace iAssist.Models
{
    public class SkillsOfWorker
    {
        public int Id { get; set; }
        public int Jobid { get; set; }
        public virtual Job Job { get; set; }
        public string Skillname { get; set; }
        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }
    }
}