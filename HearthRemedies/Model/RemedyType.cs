using System.ComponentModel.DataAnnotations;

namespace HearthRemedies.Model
{
    public enum RemedyType
    {
        [Display(Name = "Cold & Flu")] ColdAndFlu,
        Throat,
        Stomach,
        Headache,
        Skin,
        Sleep,
        [Display(Name = "Stress & Anxiety")] Stress,
        Other
    }
}