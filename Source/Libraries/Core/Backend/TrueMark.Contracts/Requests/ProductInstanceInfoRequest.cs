namespace TrueMark.Contracts.Requests
{
	/// <summary>
	/// Запрос информации о продукте
	/// </summary>
	public class ProductInstanceInfoRequest
	{
		/// <summary>
		/// Токен для запроса в Честный знак
		/// </summary>
		public string Bearer { get; set; }

		/// <summary>
		/// Код продукта для проверки
		/// </summary>
		public string ProductCode { get; set; }
	}
}
