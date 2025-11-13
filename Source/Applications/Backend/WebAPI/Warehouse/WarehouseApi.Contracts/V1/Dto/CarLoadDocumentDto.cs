namespace WarehouseApi.Contracts.V1.Dto
{
	/// <summary>
	/// Документ погрузки авто
	/// </summary>
	public class CarLoadDocumentDto
	{
		/// <summary>
		/// Id талона погрузки авто
		/// </summary>
		public int Id { get; set; }

		/// <summary>
		/// Водитель
		/// </summary>
		public string Driver { get; set; }

		/// <summary>
		/// Авто
		/// </summary>
		public string Car { get; set; }

		/// <summary>
		/// Приоритет погрузки
		/// </summary>
		public int LoadPriority { get; set; }

		/// <summary>
		/// Состояние процесса погрузки номенклатур по документу
		/// </summary>
		public LoadOperationStateEnumDto State { get; set; }
	}
}
