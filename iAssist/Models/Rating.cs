using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace iAssist.Models
{
    public class Rating
    {
        public int Id { get; set; }
        public int Rate { get; set; }
        public string Feedback { get; set; }
        public string UsernameFeedback { get; set; }
        public int WorkerID { get; set; }
        public virtual Work Works { get; set; }
        public int Jobid { get; set; }
    }
}