using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace iAssist.Models
{
    public class users
    {
        public int Id { get; set; }
        [Required]
        public string Firstname { get; set; }
        [Required]
        public string Lastname { get; set; }
        public string ProfilePicture { get; set; }
        [NotMapped]
        public HttpPostedFileBase ImageFile { get; set; }
        [Required]
        public DateTime Created_At { get; set; }
        [Required]
        public DateTime Updated_At { get; set; }
        public virtual ApplicationUser User { get; set; }
        [Required]
        public string Userid { get; set; }
    }
}