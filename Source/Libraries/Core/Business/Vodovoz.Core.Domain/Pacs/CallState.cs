using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Pacs
{
	public enum CallState
	{
		/// <summary>
		/// Звонок в режиме дозвона
		/// </summary>
		[Display(Name = "Дозвон")]
		Appeared,

		/// <summary>
		/// Соединен с оператором
		/// </summary>
		[Display(Name = "Соединен")]
		Connected,

		/// <summary>
		/// Звонок на удержании
		/// </summary>
		[Display(Name = "На удержании")]
		OnHold,

		/// <summary>
		/// Звонок завершен
		/// </summary>
		[Display(Name = "Завершен")]
		Disconnected
	}
}
