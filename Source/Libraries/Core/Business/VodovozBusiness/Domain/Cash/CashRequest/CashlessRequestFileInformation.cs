using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;
using VodovozBusiness.Domain.Common;

namespace VodovozBusiness.Domain.Cash.CashRequest
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "информация о прикрепляемых файлах заявок на оплату по безналу",
		Nominative = "информация о прикрепленном файле заявки на оплату по безналу")]
	public class CashlessRequestFileInformation : FileInformation
	{
		private int _cashlessRequestId;

		[Display(Name = "Идентификатор заявки на оплату по безналу")]
		public virtual int CashlessReqwuestId
		{
			get => _cashlessRequestId;
			set => SetField(ref _cashlessRequestId, value);
		}
	}
}
