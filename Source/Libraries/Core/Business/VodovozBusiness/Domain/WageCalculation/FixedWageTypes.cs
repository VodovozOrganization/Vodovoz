using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.WageCalculation
{
	public enum FixedWageTypes
	{
		[Display(Name = "За маршрутный лист")]
		RouteList
	}

	public class FixedWageTypesStringType : NHibernate.Type.EnumStringType
	{
		public FixedWageTypesStringType() : base(typeof(FixedWageTypes)) { }
	}
}
