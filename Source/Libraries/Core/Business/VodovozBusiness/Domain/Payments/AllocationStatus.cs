using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Payments
{
	public enum AllocationStatus
	{
		[Display(Name = "Распределено")]
		Accepted,
		[Display(Name = "Распределение отменено")]
		Cancelled
	}
	
	public class AllocationStatusStringType : NHibernate.Type.EnumStringType
	{
		public AllocationStatusStringType() : base(typeof(AllocationStatus))
		{
		}
	}
}
