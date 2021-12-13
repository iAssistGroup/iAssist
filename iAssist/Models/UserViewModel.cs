using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity.Spatial;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace iAssist.Models
{
    public class profile
    {
        public string userid { get; set; }
        [Required]
        public string Firstname { get; set; }
        [Required]
        public string Lastname { get; set; }
        public string ProfilePicture { get; set; }
        [DataType(DataType.PhoneNumber)]
        [RegularExpression(@"^\(?(09|\+639)\d{9}$", ErrorMessage = "Not a valid phone number")]
        [Required]
        public string Phonenumber { get; set; }
        public string Address { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public HttpPostedFileBase ImageFile { get; set; }
        public DateTime Created_At { get; set; }
        public DateTime Updated_At { get; set; }
        public int jobid { get; set; }
        public List<UsersWorkdet> userworkdet { get; set; }
        public List<worskills> workerskills { get; set; }
        public List<RateandFeedback> rateandFeedbacks { get; set; }
        [Required]
        public string Email { get; set; }
        public int check { get; set; }
    }
    public class UsersWorkdet
    {
        public int workid { get; set; }
        [Required]
        public string Overview { get; set; }
        [Required]
        public string Address { get; set; }
        [Required]
        [Display(Name = "Job Category")]
        public int JobId { get; set; }
        public string jobname { get; set; }
    }
    public class UserJobs
    {
        public int Jobid { get; set; }
        public string Jobname { get; set; }
    }
    public class RegisterSkilledWorker
    {
        [Required]
        [RegularExpression(@"^[0-9a-zA-Z''-'\s]{1,40}$", ErrorMessage = "Special Characters are not  allowed.")]
        public string Firstname { get; set; }
        [Required]
        [RegularExpression(@"^[0-9a-zA-Z''-'\s]{1,40}$", ErrorMessage = "Special Characters are not  allowed.")]
        public string Lastname { get; set; }
        public string ProfilePicture { get; set; }
        [Required]
        [DataType(DataType.PhoneNumber)]
        [RegularExpression(@"^\(?(09|\+639)\d{9}$", ErrorMessage = "Not a valid phone number")]
        public string Phonenumber { get; set; }
        public HttpPostedFileBase ImageFile { get; set; }
        [Required]
        public string Overview { get; set; }
        [Required]
        public string Address { get; set; }
        public string Longitude { get; set; }
        public string Latitude { get; set; }
        [Required]
        [Display(Name = "Job Category")]
        public int JobId { get; set; }
        public SelectList JobList { get; set; }
        public IEnumerable<SelectListItem> Skills { get; set; }
        public List<worskills> workerskills { get; set; }
        public IEnumerable<SelectListItem> workskill { get; set; }

    }
    public class SkilledWorkerFileImage
    {
        public int id { get; set; }
        [Display(Name = "File")]
        public string FileName { get; set; }
    }
    public class SkilledSkillWorker
    {
        [Required]
        public string Firstname { get; set; }
        [Required]
        public string Lastname { get; set; }
        public string ProfilePicture { get; set; }
        [Required]
        [DataType(DataType.PhoneNumber)]
        [RegularExpression(@"^\(?(09|\+639)\d{9}$", ErrorMessage = "Not a valid phone number")]
        public string Phonenumber { get; set; }
        [Required]
        public string Overview { get; set; }
        [Required]
        public string Address { get; set; }
        public string Longitude { get; set; }
        public string Latitude { get; set; }
        [Required]
        [Display(Name = "Job Category")]
        public int JobId { get; set; }
        public List<worskills> workerskills { get; set; }
        public List<SkilledWorkerFileImage> SkilledWorkerImageFile { get; set; }
    }
    public class ViewSubmittedWorker
    {
        [Required]
        public string Firstname { get; set; }
        [Required]
        public string Lastname { get; set; }
        public string ProfilePicture { get; set; }
        [Required]
        [DataType(DataType.PhoneNumber)]
        [RegularExpression(@"^\(?(09|\+639)\d{9}$", ErrorMessage = "Not a valid phone number")]
        public string Phonenumber { get; set; }
        [Required]
        public string Overview { get; set; }
        [Required]
        public string Address { get; set; }
        public string Longitude { get; set; }
        public string Latitude { get; set; }
        [Required]
        [Display(Name = "Job Category")]
        public int JobId { get; set; }
        public string Jobname { get; set; }
        public string Workerstatus { get; set; }
        public DateTime Created_at { get; set; }
        public DateTime Updated_at { get; set; }
        public IEnumerable<worskills> workerskills { get; set; }
        public IEnumerable<SkilledWorkerFileImage> SkilledWorkerImageFile { get; set; }

    }
    public class SubmittedFile
    {
        public string FileName { get; set; }
    }
    public class Task_Details
    {
        public string Taskdet_name { get; set; }
        public string Taskdet_dec { get; set; }
        public DateTime Taskdet_sched { get; set; }
    }
}