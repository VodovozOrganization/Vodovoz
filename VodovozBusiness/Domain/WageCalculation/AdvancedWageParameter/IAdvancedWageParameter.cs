using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.WageCalculation.AdvancedWageParameter
{
	public interface IAdvancedWageParameter
	{
		int Id { get; set; }
		AdvancedWageParameter ParentParameter { get; set; }
		WageRateTypes? WageRateType { get; set; }
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
}