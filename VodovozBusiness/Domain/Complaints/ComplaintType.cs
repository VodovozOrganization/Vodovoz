using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Complaints
{
	public enum ComplaintType
	{
		[Display(Name = "Внутренняя")]
		Inner,
		[Display(Name = "Клиентская")]
		Client
	}

	public class ComplaintTypeStringType : NHibernate.Type.EnumStringType
	{
		public ComplaintTypeStringType() : base(typeof(ComplaintType))
		{
		}
	}
}
