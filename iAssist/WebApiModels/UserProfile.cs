using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace iAssist.WebApiModels
{
    public class profileWebApi
    {
        public string userid { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string ProfilePicture { get; set; }
        public string Phonenumber { get; set; }
        //public string Address { get; set; }
        //public string Latitude { get; set; }
        //public string Longitude { get; set; }
        //public HttpPostedFileBase ImageFile { get; set; }
        public DateTime Created_At { get; set; }
        public DateTime Updated_At { get; set; }
        //public int jobid { get; set; }
        //public List<UsersWorkdet> userworkdet { get; set; }
        //public List<worskills> workerskills { get; set; }
        //public List<RateandFeedback> rateandFeedbacks { get; set; }
        public string Email { get; set; }
        //public int check { get; set; }
    }

    public class UserModel
    {
        public string Email { get; set; }
        public string userid { get; set; }
    }
}