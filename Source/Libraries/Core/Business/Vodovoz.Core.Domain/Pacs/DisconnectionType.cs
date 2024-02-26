using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Pacs
{
	public enum DisconnectionType
	{
		[Display(Name = "Нет")]
		None,
		[Display(Name = "Вручную оператором")]
		Manual,
		[Display(Name = "По таймауту из-за отсутствия активности")]
		InactivityTimeout
	}
}
