namespace TaxcomEdo.Contracts.Goods
{
	/// <summary>
	/// Информация о единице измерения для ЭДО(электронного документооборота)
	/// </summary>
	public class MeasurementUnitInfoForEdo
	{
		/// <summary>
		/// Id ед. измерения
		/// </summary>
		public int Id { get; set; }
		/// <summary>
		/// Название
		/// </summary>
		public string Name { get; set; }
		/// <summary>
		/// Количество знаков после запятой
		/// </summary>
		public short Digits { get; set; }
		/// <summary>
		/// Код ОКЕИ
		/// </summary>
		public string OKEI { get; set; }
	}
}
