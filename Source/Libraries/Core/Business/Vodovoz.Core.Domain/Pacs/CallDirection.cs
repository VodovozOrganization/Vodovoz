using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Pacs
{
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
}
