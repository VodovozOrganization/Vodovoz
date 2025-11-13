using QS.DomainModel.Entity;
using QS.HistoryLog;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Common;
using Vodovoz.Domain.Complaints;

namespace VodovozBusiness.Domain.Complaints
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "информация о прикрепляемых файлах рекламаций",
		Nominative = "информация о прикрепленном файле рекламации")]
	[HistoryTrace]
	public class ComplaintFileInformation : FileInformation
	{
		private int _complaintId;

		[Display(Name = "Идентификатор рекламации")]
		[HistoryIdentifier(TargetType = typeof(Complaint))]
		public virtual int ComplaintId
		{
			get => _complaintId;
			set => SetField(ref _complaintId, value);
		}
	}
}
