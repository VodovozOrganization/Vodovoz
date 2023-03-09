using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Complaints
{
	public enum ComplaintArrangementResultCommentType
	{
		[Display(Name = "Мероприятие")]
		Arrangement,
		[Display(Name = "Результат")]
		Result
	}

	public class ComplaintArrangementResultCommentStringType : NHibernate.Type.EnumStringType
	{
		public ComplaintArrangementResultCommentStringType() : base(typeof(ComplaintArrangementResultCommentType))
		{
		}
	}
}
