using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace iAssist.Models
{
    public class ProfileViewOfSkilledWorker
    {
        public int WorkerId { get; set; }
        public int Jobid { get; set; }
        public string Userid { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string ProfilePicture { get; set; }
        public string worker_overview { get; set; }
        public string Jobname { get; set; }
        public int? taskdet { get; set; }
    }
}