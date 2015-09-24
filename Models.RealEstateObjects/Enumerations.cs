using System.ComponentModel.DataAnnotations;

namespace Caribbean.Models.RealEstateObjects
{
    public enum ObjectStatus
    {
        [Display(Name = "Kommande")]
        Coming,

        [Display(Name = "Till försäljning")]
        ForSale,

        [Display(Name = "Referensobjekt")]
        Reference,

        [Display(Name = "Okänd")]
        Unknown
    }
}