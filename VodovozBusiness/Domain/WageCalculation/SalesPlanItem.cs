using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Gamma.Utilities;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using Vodovoz.Domain.Goods;

namespace Vodovoz.Domain.WageCalculation
{
	public abstract class SalesPlanItem : PropertyChangedBase, IDomainObject//, IValidatableObject
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



		#region IValidatableObject implementation

		/*public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			return null;
		}*/
		
		#endregion IValidatableObject implementation*/

		/*public enum SalesPlanType
		{
			[Display(Name = "План продаж номенклатуры")]
			Nomenclature,
			[Display(Name = "План продаж вида оборудования")]
			EquipmentKind,
			[Display(Name = "План продаж типа оборудования")]
			EquipmentType
		}*/
	}
}
