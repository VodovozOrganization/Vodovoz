using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Sale
{
    public class WeekDayDistrictRuleItem : DistrictRuleItemBase
    {
        WeekDayName weekDay;
        [Display(Name = "День недели")]
        public virtual WeekDayName WeekDay {
            get => weekDay;
            set => SetField(ref weekDay, value, () => WeekDay);
        }
    }
}