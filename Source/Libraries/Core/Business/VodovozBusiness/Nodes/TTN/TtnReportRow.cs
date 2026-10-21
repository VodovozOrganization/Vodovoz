namespace VodovozBusiness.Nodes.TTN
{
	/// <summary>
	/// Строка табличной части ТТН
	/// </summary>
	public class TtnReportRow
	{
		/// <summary>
		/// Идентификатор номенклатуры
		/// </summary>
		public string Code { get; set; }

		/// <summary>
		/// Наименование номенклатуры
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Количество единиц номенклатуры в позиции
		/// </summary>
		public decimal Count { get; set; }

		/// <summary>
		/// Масса позиции (вес единицы × количество) в килограммах
		/// </summary>
		public decimal Weight { get; set; }

		/// <summary>
		/// Сумма стоимости позиции, руб
		/// </summary>
		public decimal Sum { get; set; }
	}
}
