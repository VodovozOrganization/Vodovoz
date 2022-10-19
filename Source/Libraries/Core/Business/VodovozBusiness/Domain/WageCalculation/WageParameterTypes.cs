using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.WageCalculation
{
	public enum WageParameterTypes
	{
		[Display(Name = "Для сотрудников")]
		ForEmployee
	}
	
	public class WageParameterTypesStringType : NHibernate.Type.EnumStringType
	{
		public WageParameterTypesStringType() : base(typeof(WageParameterTypes)) { }
	}
}