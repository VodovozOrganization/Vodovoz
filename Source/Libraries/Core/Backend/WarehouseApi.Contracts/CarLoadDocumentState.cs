namespace WarehouseApi.Contracts
{
	public enum CarLoadDocumentState
	{
		/// <summary>
		/// Погрузка не начата
		/// </summary>
		NotStarted,
		/// <summary>
		/// В процессе погрузки
		/// </summary>
		InProgress,
		/// <summary>
		/// Погрузка завершена
		/// </summary>
		Done
	}
}
