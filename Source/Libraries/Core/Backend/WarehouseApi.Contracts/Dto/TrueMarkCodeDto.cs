namespace WarehouseApi.Contracts.Dto
{
	/// <summary>
	/// Код маркировки ЧЗ
	/// </summary>
	public class TrueMarkCodeDto
	{
		/// <summary>
		/// Порядковый номер
		/// </summary>
		public int SequenceNumber { get; set; }

		/// <summary>
		/// Код честного знака
		/// </summary>
		public string Code { get; set; }
	}
}
