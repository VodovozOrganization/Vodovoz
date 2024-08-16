using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Common;

namespace VodovozBusiness.Domain.Client
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "информация о прикрепляемых файлах контрагентов",
		Nominative = "информация о прикрепленном файле контрагента")]
	public class CounterpartyFileInformation : FileInformation
	{
		private int _counterpartyId;

		[Display(Name = "Идентификатор контрагента")]
		public virtual int CounterpartyId
		{
			get => _counterpartyId;
			set => SetField(ref _counterpartyId, value);
		}
	}
}
