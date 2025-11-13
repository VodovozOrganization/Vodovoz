using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;

namespace Vodovoz.Domain.WageCalculation.AdvancedWageParameters
{
	public interface IAdvancedWageParameter : IWageHierarchyNode, IDomainObject
	{
		IAdvancedWageParameter ParentParameter { get; }
		WageRate WageRate { get; set; }
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
