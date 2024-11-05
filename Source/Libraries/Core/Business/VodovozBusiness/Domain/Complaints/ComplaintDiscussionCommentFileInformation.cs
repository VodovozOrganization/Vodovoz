using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;
using VodovozBusiness.Domain.Common;

namespace VodovozBusiness.Domain.Complaints
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "информация о прикрепляемых файлах комментариев рекламации",
		Nominative = "информация о прикрепленном файле комментария рекламации")]
	public class ComplaintDiscussionCommentFileInformation : FileInformation
	{
		private int _complaintDiscussionCommentId;

		[Display(Name = "Идентификатор комментария рекламации")]
		public virtual int ComplaintDiscussionCommentId
		{
			get => _complaintDiscussionCommentId;
			set => SetField(ref _complaintDiscussionCommentId, value);
		}
	}
}
