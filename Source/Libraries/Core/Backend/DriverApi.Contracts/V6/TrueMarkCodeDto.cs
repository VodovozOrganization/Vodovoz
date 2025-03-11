namespace DriverApi.Contracts.V6
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

		/// <summary>
		/// Уровень кода
		/// </summary>
		public DriverApiTruemarkCodeLevel Level { get; set; }

		/// <summary>
		/// Родительский код
		/// </summary>
		public string Parent { get; set; }
	}
}
