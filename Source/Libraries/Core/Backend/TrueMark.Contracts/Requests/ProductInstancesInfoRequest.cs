using System.Collections.Generic;

namespace TrueMark.Contracts.Requests
{
	/// <summary>
	/// Запрос информации о продукте
	/// </summary>
	public class ProductInstancesInfoRequest
	{
		/// <summary>
		/// Токен для запроса в Честный знак
		/// </summary>
		public string Bearer { get; set; }

		/// <summary>
		/// Коды продуктов для проверки
		/// </summary>
		public IEnumerable<string> ProductCodes { get; set; }
	}
}
