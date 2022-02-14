using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace iAssist.Models
{
    public class complaintViewModel
    {
        public string WorkerUserID { get; set; }
        public int WorkerWarning { get; set; }
        public int WorkerReports { get; set; }
        public string WorkerFirstname { get; set; }
        public string WorkerLastname { get; set; }
        public string Workerusername { get; set; }
        public DateTime? locoutdatetime { get; set; }
    }
}