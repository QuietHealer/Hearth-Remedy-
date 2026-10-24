using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace HearthRemedies.Model
{
    public static class EnumExtensions
    {
        // RemedyType.ColdAndFlu -> "Cold & Flu"; falls back to the enum name if there's no [Display]
        public static string GetDisplayName(this Enum value) =>
            value.GetType()
                 .GetMember(value.ToString())
                 .FirstOrDefault()?
                 .GetCustomAttribute<DisplayAttribute>()?
                 .GetName()
            ?? value.ToString();
    }
}