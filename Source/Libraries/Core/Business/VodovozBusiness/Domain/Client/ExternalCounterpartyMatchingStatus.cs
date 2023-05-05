using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Client
{
	public enum ExternalCounterpartyMatchingStatus
	{
		[Display(Name = "Ожидает обработки")]
		AwaitingProcessing,
		[Display(Name = "Обработан")]
		Processed
	}
}
