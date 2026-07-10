using Vodovoz.Domain.Goods;

namespace VodovozBusiness.Domain.Orders
{
	public interface INomenclatureCount
	{
		/// <summary>
		/// Количество
		/// </summary>
		decimal Count { get; }
		/// <summary>
		/// Номенклатура <see cref="Vodovoz.Domain.Goods.Nomenclature"/>
		/// </summary>
		Nomenclature Nomenclature { get; }
	}
}
