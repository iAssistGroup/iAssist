using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace iAssist.Models
{
    public class BidViewModel
    {
        public int Bidid { get; set; }
        [Required]
        [DataType(DataType.Currency)]
        public decimal Bid_Amount { get; set; }
        [Required]
        [StringLength(4096, MinimumLength = 30, ErrorMessage = "{0} length must be in the range 30..4096")]
        public string Bid_Description { get; set; }
        public DateTime BidTimeExp { get; set; }
        public int TaskdetId { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string ProfilePicture { get; set; }
        public string Tasktitle { get; set; }
        public int? user { get; set; }
        public int? workerid { get; set; }
        public string Username { get; set; }
        public int? bookstatus { get; set; }
        public double? Rate { get; set; }
    }
}