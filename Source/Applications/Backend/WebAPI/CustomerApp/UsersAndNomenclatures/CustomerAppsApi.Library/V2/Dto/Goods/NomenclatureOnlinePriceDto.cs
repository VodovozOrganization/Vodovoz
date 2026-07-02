namespace CustomerAppsApi.Library.V2.Dto.Goods
{
	/// <summary>
	/// Данные по цене
	/// </summary>
	public class NomenclatureOnlinePriceDto
	{
		/// <summary>
		/// Идентификатор цены
		/// </summary>
		public int Id { get; set; }
		/// <summary>
		/// Идентификатор онлайн параметров
		/// </summary>
		public int NomenclatureOnlineParametersId { get; set; }
		/// <summary>
		/// Минимальное количество
		/// </summary>
		public int MinCount { get; set; }
		/// <summary>
		/// Цена
		/// </summary>
		public decimal Price { get; set; }
		/// <summary>
		/// Цена без скидки
		/// </summary>
		public decimal? PriceWithoutDiscount { get; set; }
	}
}
