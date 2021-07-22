using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.WageCalculation
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки плана продаж",
		Nominative = "строка плана продаж")]
	public abstract class SalesPlanItem : PropertyChangedBase, IDomainObject
	{
		public virtual int Id { get; set; }
		private SalesPlan _salesPlan;
		private int _planDay;
		private int _planMonth;

		[Display(Name = "План день")]
		public virtual int PlanDay
		{
			get => _planDay;
			set => SetField(ref _planDay, value);
		}

		[Display(Name = "План месяц")]
		public virtual int PlanMonth
		{
			get => _planMonth;
			set => SetField(ref _planMonth, value);
		}

		[Display(Name = "План продаж")]
		public virtual SalesPlan SalesPlan
		{
			get => _salesPlan;
			set => SetField(ref _salesPlan, value);
		}
	}
}
