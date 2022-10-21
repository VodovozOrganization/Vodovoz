using System.ComponentModel.DataAnnotations;

namespace Vodovoz.CommonEnums
{
    public enum Month
    {
        [Display(Name = "Январь")] 	 Jan = 1,
        [Display(Name = "Февраль")]  Feb,
        [Display(Name = "Март")] 	 Mar,
        [Display(Name = "Апрель")] 	 Apr,
        [Display(Name = "Май")] 	 May,
        [Display(Name = "Июнь")] 	 Jun,
        [Display(Name = "Июль")] 	 Jul,
        [Display(Name = "Август")] 	 Aug,
        [Display(Name = "Сентябрь")] Sep,
        [Display(Name = "Октябрь")]  Oct,
        [Display(Name = "Ноябрь")] 	 Nov,
        [Display(Name = "Декабрь")]  Dec,
    }
}