using QS.DomainModel.Entity;
using QS.HistoryLog;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Common;

namespace Vodovoz.Core.Domain.Clients
{
	/// <summary>
	/// Класс для хранения информации о прикрепляемых файлах контрагентов
	/// </summary>
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "информация о прикрепляемых файлах контрагентов",
		Nominative = "информация о прикрепленном файле контрагента")]
	[HistoryTrace]
	public class CounterpartyFileInformation : FileInformation
	{
		private int _counterpartyId;

		/// <summary>
		/// Идентификатор контрагента
		/// </summary>
		[Display(Name = "Идентификатор контрагента")]
		[HistoryIdentifier(TargetType = typeof(CounterpartyEntity))]
		public virtual int CounterpartyId
		{
			get => _counterpartyId;
			set => SetField(ref _counterpartyId, value);
		}
	}
}
