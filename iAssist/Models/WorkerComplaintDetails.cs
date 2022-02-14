using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace iAssist.Models
{
    public class WorkerComplaintDetails
    {
        public string WorkerFirstname { get; set; }
        public string WorkerUserID { get; set; }
        public string WorkerLastname { get; set; }
        public string WorkerUsername { get; set; }
        public IEnumerable<Complaint> complaints { get; set; }
        public DateTime? loclocoutdatetime { get; set; }
    }
}