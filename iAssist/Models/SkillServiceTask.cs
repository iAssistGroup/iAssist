using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace iAssist.Models
{
    public class SkillServiceTask
    {
        public int Id { get; set; }
        public string Skillname { get; set; }
        public int Taskdet { get; set; }
        public string UserId { get; set; }
        public int Jobid { get; set; }
    }
}