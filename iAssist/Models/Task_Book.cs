using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace iAssist.Models
{
    public class Task_Book
    {
        public int Id { get; set; }
        public string Taskbook_Type { get; set; }
        public int Taskbook_Status { get; set; }
        public DateTime Taskbook_Created_at { get; set; }
        public DateTime Taskbook_Updated_at { get; set; }
        public int? workerId { get; set; }
        public int TaskDetId { get; set; }
        public virtual TaskDetails TaskDetails { get; set; }
    }
}