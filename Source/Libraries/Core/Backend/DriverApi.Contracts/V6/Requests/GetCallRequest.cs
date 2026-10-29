using System.ComponentModel.DataAnnotations;

namespace DriverApi.Contracts.V6.Requests
{
	/// <summary>
	/// Запрос на созвон с клиентом
	/// </summary>
	public class GetCallRequest
	{
		private const string _phoneNumberPattern = @"^7\d{10}$";

		/// <summary>
		/// Номер маршрутного листа
		/// </summary>
		[Required]
		public int RouteListId { get; set; }

		/// <summary>
		/// Номер телефона клиента
		/// </summary>
		[Required]
		[RegularExpression(_phoneNumberPattern, ErrorMessage = "Номер телефона должен начинаться с 7 и содержать 11 цифр")]
		public string Number { get; set; }
	}
}
