using System.ComponentModel.DataAnnotations;

namespace Vodovoz.ViewModels.Journals.FilterViewModels.Enums
{
    public enum LevelsFilter
    {
        [Display(Name = "Уровень 1")]
        Level1 = 1,
        [Display(Name = "Уровень 2")]
        Level2 = 2,
        [Display(Name = "Уровень 3")]
        Level3 = 3,
        [Display(Name = "Уровень 4")]
        Level4 = 4,
        [Display(Name = "Все")]
        All = 5
    }
}