using System.ComponentModel.DataAnnotations;

namespace Caribbean.Models.Database
{
    public enum PrintStatus
    {
        [Display(Name = "Okänd")]
        Unknown,

        [Display(Name = "Pågående")]
        InProgress,

        [Display(Name = "Klar")]
        Completed,

        [Display(Name = "Beställd")]
        Ordered,

        [Display(Name = "Skickad")]
        Shipped,

        [Display(Name = "Mottagen")]
        Received,

        [Display(Name = "Arkiverad")]
        Archived
    }

    public enum JobStatus
    {
        Unknown,
        InProgress,
        Completed
    }
}