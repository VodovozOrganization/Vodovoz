using System.ComponentModel.DataAnnotations;

namespace DriverApi.Contracts.V6.Requests
{
	/// <summary>
	/// Запрос на созвон с клиентом
	/// </summary>
	public class GetCallRequest
	{
		/// <summary>
		/// Номер маршрутного листа
		/// </summary>
		[Required]
		public int RouteListId { get; set; }

		/// <summary>
		/// Номер телефона клиента
		/// </summary>
		[Required]
		public string Number { get; set; }
	}
}
