using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.WageCalculation
{
	public enum WageParameterTargets
	{
		[Display(Name = "Для автомобилей компании")]
		ForOurCars,
		[Display(Name = "Для автомобилей наемных водителей")]
		ForMercenariesCars
	}

	public class WageParameterTargetsStringType : NHibernate.Type.EnumStringType
	{
		public WageParameterTargetsStringType() : base(typeof(WageParameterTargets)) { }
	}
}
