using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace iAssist.Models
{
    public class NotificationModel
    {
        public int Id { get; set; }
        public string Details { get; set; }
        public string Title { get; set; }
        public string DetailsURL { get; set; }
        public virtual ApplicationUser User { get; set; }
        public string Receiver { get; set; }
        public DateTime Date { get; set; }
        public bool IsRead { get; set; }
    }
}