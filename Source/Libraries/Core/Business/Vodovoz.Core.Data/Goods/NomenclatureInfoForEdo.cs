using Vodovoz.Core.Domain.Goods;

namespace Vodovoz.Core.Data.Goods
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
		/// Категория
		/// </summary>
		public NomenclatureCategory Category { get; set; }
		/// <summary>
		/// Информация о единице измерения <see cref="MeasurementUnitInfoForEdo"/>
		/// </summary>
		public MeasurementUnitInfoForEdo MeasurementUnitInfoForEdo { get; set; }
	}
}
