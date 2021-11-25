using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace iAssist.Models
{
    public class Bid
    {
        public int Id { get; set; }
        public decimal Bid_Amount { get; set; }
        public string Bid_Description { get; set; }
        public DateTime Created_at { get; set; }
        public DateTime Updated_at { get; set; }
        public int TaskDetId { get; set; }
        public virtual TaskDetails TaskDetails { get; set; }
        public int WorkerId { get; set; }
        public virtual Work Work { get; set; }
        public int bid_status { get; set; }
    }
}