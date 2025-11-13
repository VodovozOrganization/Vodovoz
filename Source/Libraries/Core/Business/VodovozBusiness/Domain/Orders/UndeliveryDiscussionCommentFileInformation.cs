using QS.DomainModel.Entity;
using QS.HistoryLog;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Common;
using Vodovoz.Domain.Orders;

namespace VodovozBusiness.Domain.Orders
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "информация о прикрепляемых файлах комментариев к обсужденим недовозов",
		Nominative = "информация о прикрепленном файле комментария к обсуждению недовоза")]
	[HistoryTrace]
	public class UndeliveryDiscussionCommentFileInformation : FileInformation
	{
		private int _undeliveryDiscussionCommentId;

		[Display(Name = "Идентификатор комментария к обсуждению недовоза")]
		[HistoryIdentifier(TargetType = typeof(UndeliveryDiscussionComment))]
		public virtual int UndeliveryDiscussionCommentId
		{
			get => _undeliveryDiscussionCommentId;
			set => SetField(ref _undeliveryDiscussionCommentId, value);
		}
	}
}
