namespace DriverApi.Contracts.V5
{
	/// <summary>
	/// Строка передачи товара
	/// </summary>
	public class TransferItemDto
	{
		/// <summary>
		/// Название номенклатуры
		/// </summary>
		public string NomenclatureTitle { get; set; }

		/// <summary>
		/// Количество
		/// </summary>
		public decimal Amount { get; set; }
	}
}
