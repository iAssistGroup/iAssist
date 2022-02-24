using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace iAssist.WebApiModels
{
    public class TaskDetailsViewModel
    {
        public int Id { get; set; }
        public string TaskTitle { get; set; }
        public string TaskDesc { get; set; }
        public DateTime taskdet_sched { get; set; }
        public DateTime taskdet_Created_at { get; set; }
        public DateTime taskdet_Updated_at { get; set; }
        public int JobId { get; set; }
        public IEnumerable<JobListModel> JobList { get; set; }
        public IEnumerable<SkillListModel> SkillList { get; set; }
        public IEnumerable<string> SelectedSkills { get; set; }
        public string UserId { get; set; }
        public string TaskImage { get; set; }
        [Required]
        public string Address { get; set; }
        [Required]
        public string Latitude { get; set; }
        [Required]
        public string Longitude { get; set; }
        public HttpPostedFileBase ImageFile { get; set; }
        public int? workerid { get; set; }



        public DateTime taskdet_time { get; set; }
        public decimal Budget { get; set; }
        //public int check { get; set; }
        //public IEnumerable<SelectListItem> Skilltasks { get; set; }

    }

    public class JobListModel
    {
        public int Id { get; set; }
        public string JobName { get; set; }
    }

    public class SkillListModel
    {
        public int Id { get; set; }
        public string Skillname { get; set; }
    }
}