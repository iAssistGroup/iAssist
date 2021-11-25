using System.ComponentModel.DataAnnotations;

namespace iAssist.WebApiModels
{
    public class ComplaintModel
    {
        [Required]
        [Display(Name = "Complain Type / Title")]
        public string ComplainType { get; set; }
        [Required]
        [Display(Name = "Explain Why you report him/her")]
        public string Description { get; set; }
        public string image { get; set; }
        public int Workerid { get; set; }
    }
}