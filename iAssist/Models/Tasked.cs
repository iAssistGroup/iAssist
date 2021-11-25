using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace iAssist.Models
{
    public class Tasked
    {
        public int Id { get; set; }
        public string TaskType { get; set; }
        public Decimal TaskPayable { get; set; }
        public int TaskStatus { get; set; }
        public DateTime TaskCompletionTime { get; set; }
        public DateTime TaskCreated_at { get; set; }
        public DateTime TaskUpdated_at { get; set; }
        public int WorkerId { get; set; }
        public virtual Work Work { get; set; }
        public int TaskDetId { get; set; }
        public virtual TaskDetails TaskDetails{ get; set; }
    }
}