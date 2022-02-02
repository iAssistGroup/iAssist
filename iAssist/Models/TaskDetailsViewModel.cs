using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace iAssist.Models
{
    public class TaskDetailsViewModel
    {
        public int Id { get; set; }
        [Required]
        [Display(Name ="Task Title")]
        public string TaskTitle{ get; set; }
        [Required]
        [Display(Name = "Task Description")]
        [StringLength(4096, MinimumLength = 30, ErrorMessage = "{0} length must be in the range 30..4096")]
        public string TaskDesc { get; set; }
        [Required]
        [DataType(DataType.Date)]
        [Display(Name ="Schedule Date")]
        public DateTime taskdet_sched { get; set; }
        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Schedule Time")]
        public DateTime taskdet_time { get; set; }
        public DateTime taskdet_Created_at { get; set; }
        public DateTime taskdet_Updated_at { get; set; }
        [Required]
        [Display(Name = "Job Category")]
        public int JobId { get; set; }
        [Required]
        [DataType(DataType.Currency)]
        [Display(Name = "Price/Budget")]
        public decimal Budget { get; set; }
        public int check { get; set; }
        public SelectList JobList { get; set; }
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
        public IEnumerable<SelectListItem> Skilltasks { get; set; }

    }
}