using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace iAssist.WebApiModels
{
    public class SearchNearSkilledWorkerView
    {
        [Required]
        public string Address { get; set; }
        [Required]
        public string Longitude { get; set; }
        [Required]
        public string Latitude { get; set; }
        [Display(Name = "Category")]
        public int JobId { get; set; }
        public IEnumerable<JobListModel> JobList { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string Profile { get; set; }
        public string nearaddress { get; set; }
        public string Jobname { get; set; }
        public string UserId { get; set; }
        public int WorkerId { get; set; }
        public string distance { get; set; }
        public int Taskdet { get; set; }
        public double? Rate { get; set; }
    }
}