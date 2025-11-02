using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Pacs
{
	public enum CallEntryResult
	{
		/// <summary>
		/// Звонок пропущен, разговор не состоялся
		/// </summary>
		[Display(Name = "Пропущен")]
		Missed = 0,

		/// <summary>
		/// Звонок успешен и разговор состоялся
		/// </summary>
		[Display(Name = "Успешен")]
		Sucess = 1
	}
}
