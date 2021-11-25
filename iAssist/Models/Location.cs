using System;
using System.Collections.Generic;
using System.Data.Entity.Spatial;
using System.Linq;
using System.Web;

namespace iAssist.Models
{
    public class Location
    {
        public int Id { get; set; }
        public int? JobId { get; set; }
        public string Loc_Address { get; set; }
        public DateTime Created_At { get; set; }
        public DateTime Updated_At { get; set; }
        public DbGeography Geolocation { get; set; }
        public virtual ApplicationUser User { get; set; }
        public string UserId { get; set; }

    }
}