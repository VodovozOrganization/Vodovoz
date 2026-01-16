using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Clients.Accounts
{
	public enum ExternalLegalCounterpartyActivationState
	{
		/// <summary>
		/// В процессе
		/// </summary>
		[Display(Name = "В процессе")]
		InProgress,
		/// <summary>
		/// Готово
		/// </summary>
		[Display(Name = "Готово")]
		Done
	}
}
