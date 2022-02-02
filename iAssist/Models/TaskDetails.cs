using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity.Spatial;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace iAssist.Models
{
    public class TaskDetails
    {
        public int Id { get; set; }
        [Required]
        public string taskdet_name { get; set; }
        [Required]
        public string taskdet_desc { get; set; }
        [Required]
        [DataType(DataType.Date)]
        public DateTime taskdet_sched { get; set; }
        [Required]
        [DataType(DataType.Time)]
        public DateTime taskdet_time { get; set; }
        public decimal Budget { get; set; }
        public DateTime taskdet_Created_at { get; set; }
        public DateTime taskdet_Updated_at { get; set; }
        public string TaskImage { get; set; }
        [Required]
        public string Loc_Address { get; set; }
        public DbGeography Geolocation { get; set; }
        [Required]
        public int JobId { get; set; }
        public virtual Job Job { get; set; }
        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }
    }
}