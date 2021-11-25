using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace iAssist.Models
{
    public class JobCategory
    {
        [Required]
        [Display(Name = "Job Name")]
        public string Jobname { get; set; }
        [Required]
        [Display(Name = "Job Description")]
        public string JobDescription { get; set; }
    }
    public class Users
    {
        public string UserId { get; set; }
        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }
        [Required]
        [Display(Name = "Firstname")]
        public string Firstname { get; set; }
        [Required]
        [Display(Name = "Lastname")]
        public string Lastname { get; set; }
        [Display(Name = "Profile Picture")]
        public string ProfilePicture { get; set; }
        [Required]
        [Display(Name = "Account Created")]
        public string Created { get; set; }
        [DataType(DataType.Currency)]
        public decimal balance { get; set; }
        [Display(Name ="Role")]
        public string Rolename { get; set; }
        public DateTime? locoutdatetime { get; set; }
        public string Jobname { get; set; }
    }
    public class ManageSkilledWorker
    {
        public string Id { get; set; }
        [Display(Name ="Profile Picture")]
        public string Profile { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string JobName { get; set; }
        public int Jobid { get; set; }
        public int Worker_status { get; set; }
    }
    public class ViewDetailsRegSkilledWorker
    {
        public string Id { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string ProfilePicture { get; set; }
        public string Phonenumber { get; set; }
        public string EmailAdd { get; set; }
        public string workoverview { get; set; }
        public string JobName { get; set; }
        public string worker_status { get; set; }
        public int JobId { get; set; }
    }
    public class ViewFileRegSkilledWorker
    {
        public string FileName { get; set; }
    }
    public class SkilledWorkerList
    {
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string Jobname { get; set; }
        public string Work_status { get; set; }
        public string VerifiedDate { get; set; }
    }
}