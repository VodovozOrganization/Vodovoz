using System.ComponentModel.DataAnnotations;

namespace DriverApi.Contracts.V7.Requests
{
	/// <summary>
	/// Запрос выбора адреса для следующей точки маршрута
	/// </summary>
	public class SelectAddressRequest
	{
		/// <summary>
		/// Id выбранного адреса МЛ
		/// </summary>
		[Required]
		public int NextAddressId { get; set; }

		/// <summary>
		/// Id предыдущего адреса МЛ, который не был завершен
		/// </summary>
		public int? PreviousUncompletedAddressId { get; set; }
	}
}
