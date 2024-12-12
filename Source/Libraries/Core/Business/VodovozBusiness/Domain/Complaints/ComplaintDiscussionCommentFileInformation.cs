using QS.DomainModel.Entity;
using QS.HistoryLog;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Common;
using Vodovoz.Domain.Complaints;

namespace VodovozBusiness.Domain.Complaints
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "информация о прикрепляемых файлах комментариев рекламации",
		Nominative = "информация о прикрепленном файле комментария рекламации")]
	[HistoryTrace]
	public class ComplaintDiscussionCommentFileInformation : FileInformation
	{
		private int _complaintDiscussionCommentId;

		[Display(Name = "Идентификатор комментария рекламации")]
		[HistoryIdentifier(TargetType = typeof(ComplaintDiscussionComment))]
		public virtual int ComplaintDiscussionCommentId
		{
			get => _complaintDiscussionCommentId;
			set => SetField(ref _complaintDiscussionCommentId, value);
		}
	}
}
