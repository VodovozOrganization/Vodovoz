using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Common;

namespace VodovozBusiness.Domain.Complaints
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "информация о прикрепляемых файлах рекламаций",
		Nominative = "информация о прикрепленном файле рекламации")]
	public class ComplaintFileInformation : FileInformation
	{
		private int _complaintId;

		[Display(Name = "Идентификатор рекламации")]
		public virtual int ComplaintId
		{
			get => _complaintId;
			set => SetField(ref _complaintId, value);
		}
	}
}
