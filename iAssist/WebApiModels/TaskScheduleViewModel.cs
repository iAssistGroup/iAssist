using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace iAssist.WebApiModels
{
    public class TaskScheduleModel
    {
        public int EventID { get; set; }
        public int TaskId { get; set; }
        public string Subject { get; set; }
        public string Description { get; set; }
        public DateTime Start { get; set; }
        public DateTime? End { get; set; }
        public string ThemeColor { get; set; }
        public bool IsFullDay { get; set; }
    }
}