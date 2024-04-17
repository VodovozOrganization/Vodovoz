using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Sale
{
    public enum DistrictWaterPrice
    {
        [Display(Name = "По прайсу")]
        Standart,
        [Display(Name = "Специальная цена")]
        FixForDistrict,
        [Display(Name = "По расстоянию")]
        ByDistance,
    }
	
    public enum WeekDayName
    {
        [Display(Name = "Сегодня")]
        Today = 0,
        [Display(Name = "Понедельник")]
        Monday = 1,
        [Display(Name = "Вторник")]
        Tuesday = 2,
        [Display(Name = "Среда")]
        Wednesday = 3,
        [Display(Name = "Четверг")]
        Thursday = 4,
        [Display(Name = "Пятница")]
        Friday = 5,
        [Display(Name = "Суббота")]
        Saturday = 6,
        [Display(Name = "Воскресенье")]
        Sunday = 7
    }
}
