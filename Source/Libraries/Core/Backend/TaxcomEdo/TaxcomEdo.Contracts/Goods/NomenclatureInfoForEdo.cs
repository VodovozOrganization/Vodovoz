namespace TaxcomEdo.Contracts.Goods
{
	/// <summary>
	/// Информация о номенклатуре для ЭДО(электронного документооборота)
	/// </summary>
	public class NomenclatureInfoForEdo
	{
		/// <summary>
		/// Id номенклатуры
		/// </summary>
		public int Id { get; set; }
		/// <summary>
		/// Официальное название
		/// </summary>
		public string OfficialName { get; set; }
		/// <summary>
		/// Gtin для маркировки
		/// </summary>
		public string Gtin { get; set; }
		/// <summary>
		/// Тип номенклатуры
		/// </summary>
		public NomenclatureInfoCategory Category { get; set; }
		/// <summary>
		/// Информация о единице измерения <see cref="MeasurementUnitInfoForEdo"/>
		/// </summary>
		public MeasurementUnitInfoForEdo MeasurementUnitInfoForEdo { get; set; }
	}
}
