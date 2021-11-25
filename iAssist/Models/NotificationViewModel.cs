using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace iAssist.Models
{
    public class NotificationViewModel
    {
        public string Details { get; set; }
        public string Title { get; set; }
        public string DetailsURL { get; set; }
        public string Receiver { get; set; }
        public DateTime Date { get; set; }
        public bool IsRead { get; set; }
    }
}