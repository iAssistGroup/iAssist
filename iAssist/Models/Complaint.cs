using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace iAssist.Models
{
    public class Complaint
    {
        public int Id { get; set; }
        public string ComplaintTitle { get; set; }
        public string Desc { get; set; }
        public DateTime Created_at { get; set; }
        public DateTime Updated_at { get; set; }
        public string compimage { get; set; }
        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }
        public int WorkerId { get; set; }
        public virtual Work works { get; set; }
    }
}