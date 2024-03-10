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

	public enum CallDirection
	{
		/// <summary>
		/// Звонок между двумя абонентами ВАТС
		/// </summary>
		[Display(Name = "Внутренний")]
		Internal = 0,

		/// <summary>
		/// Звонок от внешнего номера абоненту ВАТС
		/// </summary>
		[Display(Name = "Входящий")]
		Incoming = 1,

		/// <summary>
		/// Звонок от абонента ВАТС на внешний номер
		/// </summary>
		[Display(Name = "Исходящий")]
		Outcomming = 2,
	}

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

	public enum CallTransferType
	{
		[Display(Name = "Консультативный")]
		Consultative,

		[Display(Name = "Слепой")]
		Blind,

		[Display(Name = "Возврат слепого перевода")]
		ReturnBlind
	}
}
