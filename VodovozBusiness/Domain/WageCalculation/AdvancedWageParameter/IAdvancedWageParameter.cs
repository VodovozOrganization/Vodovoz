using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.WageCalculation.AdvancedWageParameters
{
	public interface IAdvancedWageParameter
	{
		IAdvancedWageParameter ParentParameter { get; }
		WageRate WageRate { get; set; }
		decimal Wage { get; set; }
		bool HasConflicWith(IAdvancedWageParameter advancedWageParameter);
		AdvancedWageParameterType AdvancedWageParameterType { get; }
	}

	public enum AdvancedWageParameterType
	{
		[Display(Name = "По времени доставки")]
		DeliveryTime,
		[Display(Name = "По количеству бутылей")]
		BottlesCount
	}

	public class AdvancedWageParameterStringType : NHibernate.Type.EnumStringType
	{
		public AdvancedWageParameterStringType() : base(typeof(AdvancedWageParameterType))
		{
		}
	}
}