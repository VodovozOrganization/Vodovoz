using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Clients
{
	/// <summary>
	/// Состояние связки юр лица и физика для заказа в ИПЗ
	/// </summary>
	public enum ConnectedCustomerConnectState
	{
		/// <summary>
		/// Заблокирована
		/// </summary>
		[Display(Name = "Доступ заблокирован")]
		Blocked,
		/// <summary>
		/// Активна
		/// </summary>
		[Display(Name = "Доступ активен")]
		Active
	}
}
