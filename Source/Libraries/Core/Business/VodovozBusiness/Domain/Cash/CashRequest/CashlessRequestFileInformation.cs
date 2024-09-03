using QS.DomainModel.Entity;
using QS.HistoryLog;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Common;
using Vodovoz.Domain.Cash;

namespace VodovozBusiness.Domain.Cash.CashRequest
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "информация о прикрепляемых файлах заявок на оплату по безналу",
		Nominative = "информация о прикрепленном файле заявки на оплату по безналу")]
	[HistoryTrace]
	public class CashlessRequestFileInformation : FileInformation
	{
		private int _cashlessRequestId;

		[Display(Name = "Идентификатор заявки на оплату по безналу")]
		[HistoryIdentifier(TargetType = typeof(CashlessRequest))]
		public virtual int CashlessReqwuestId
		{
			get => _cashlessRequestId;
			set => SetField(ref _cashlessRequestId, value);
		}
	}
}
