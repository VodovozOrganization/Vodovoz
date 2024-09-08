using QS.DomainModel.Entity;
using QS.HistoryLog;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Common;
using Vodovoz.Domain.Client;

namespace VodovozBusiness.Domain.Client
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "информация о прикрепляемых файлах контрагентов",
		Nominative = "информация о прикрепленном файле контрагента")]
	[HistoryTrace]
	public class CounterpartyFileInformation : FileInformation
	{
		private int _counterpartyId;

		[Display(Name = "Идентификатор контрагента")]
		[HistoryIdentifier(TargetType = typeof(Counterparty))]
		public virtual int CounterpartyId
		{
			get => _counterpartyId;
			set => SetField(ref _counterpartyId, value);
		}
	}
}
