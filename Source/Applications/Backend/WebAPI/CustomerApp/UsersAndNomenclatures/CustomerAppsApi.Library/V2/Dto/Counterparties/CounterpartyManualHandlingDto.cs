using System;
using Vodovoz.Domain.Client;

namespace CustomerAppsApi.Library.V2.Dto.Counterparties
{
	/// <summary>
	/// Данные о ручной обработке
	/// </summary>
	public class CounterpartyManualHandlingDto
	{
		public CounterpartyManualHandlingDto(CounterpartyIdentificationDto counterpartyIdentificationDto,
			ExternalCounterpartyMatching externalCounterpartyMatching)
		{
			CounterpartyIdentificationDto =
				counterpartyIdentificationDto ?? throw new ArgumentNullException(nameof(counterpartyIdentificationDto));
			ExternalCounterpartyMatching =
				externalCounterpartyMatching ?? throw new ArgumentNullException(nameof(externalCounterpartyMatching));
		}
		
		/// <summary>
		/// Информация об идентификации клиента <see cref="CounterpartyIdentificationDto"/>
		/// </summary>
		public CounterpartyIdentificationDto CounterpartyIdentificationDto { get; }
		/// <summary>
		/// Данные по сопоставлению клиента из ИПЗ <see cref="ExternalCounterpartyMatching"/>
		/// </summary>
		public ExternalCounterpartyMatching ExternalCounterpartyMatching { get; }
	}
}
