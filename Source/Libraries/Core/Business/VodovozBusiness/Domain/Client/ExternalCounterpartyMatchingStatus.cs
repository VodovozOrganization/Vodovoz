using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Client
{
	public enum ExternalCounterpartyMatchingStatus
	{
		[Display(Name = "Ожидает обработки")]
		AwaitingProcessing,
		[Display(Name = "Юр лицо")]
		LegalCounterparty,
		[Display(Name = "Обработан")]
		Processed
	}
}
