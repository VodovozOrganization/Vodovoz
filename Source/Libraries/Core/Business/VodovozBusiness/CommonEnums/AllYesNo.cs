using System.ComponentModel.DataAnnotations;

namespace Vodovoz.CommonEnums
{
    public enum AllYesNo
    {
        [Display(Name = "Все")]
        All,
        [Display(Name = "Да")]
        Yes,
        [Display(Name = "Нет")]
        No
    }
}
