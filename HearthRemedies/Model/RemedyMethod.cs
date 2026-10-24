using System.ComponentModel.DataAnnotations;

namespace HearthRemedies.Model
{
    public enum RemedyMethod
    {
        Drink,
        [Display(Name = "Food / Solid")] Solid,
        [Display(Name = "On the Skin")] Skin,
        Inhale,
        Bath,
        Other
    }
}