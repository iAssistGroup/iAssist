using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity.Spatial;
using System.Linq;
using System.Web;

namespace iAssist.Models
{
    public class TaskPostListView
    {
        public int Id { get; set; }
        public int Taskbook_Status { get; set; }
        public string taskdet_name { get; set; }
        public string taskdet_desc { get; set; }
        [DataType(DataType.Date)]
        public DateTime taskdet_sched { get; set; }
        [DataType(DataType.Time)]
        public DateTime taskdet_time { get; set; }
        public DateTime taskdet_Created_at { get; set; }
        public DateTime taskdet_Updated_at { get; set; }
        public string TaskImage { get; set; }
        public string Loc_Address { get; set; }
        public DbGeography Geolocation { get; set; }
        public string Jobname { get; set; }
        public int jobid { get; set; }
        public string UserId { get; set; }
        public string Username { get; set; }
        public int? workerid { get; set; }
        public int? bid { get; set; }
        public int? taskedstatus { get; set; }
        [DataType(DataType.Currency)]
        public decimal? taskedTaskPayable { get; set; }
        public string taskedWorkerfname { get; set; }
        public string taskedWorkerlname { get; set; }
        public int? taskedid { get; set; }
        public string Tasktype { get; set; }
        public int? specificworkerid { get; set; }
    }
    public class taskViewPost
    {
        public IEnumerable<TaskPostListView> Taskpostlistview { get; set; }
        public List<SkillServiceTask> TaskViewPost { get; set; }
    }
}