namespace TaxcomEdo.Contracts.Goods
{
	/// <summary>
	/// Информация о спец номенклатуре для ЭДО(электронного документооборота)
	/// </summary>
	public class SpecialNomenclatureInfoForEdo
	{
		/// <summary>
		/// Id спец номенклатуры
		/// </summary>
		public int Id { get; set; }
		/// <summary>
		/// Id клиента
		/// </summary>
		public int CounterpartyId { get; set; }
		/// <summary>
		/// Id номенклатуры
		/// </summary>
		public int NomenclatureId { get; set; }
		/// <summary>
		/// Код ТМЦ
		/// </summary>
		public int SpecialId { get; set; }
	}
}
