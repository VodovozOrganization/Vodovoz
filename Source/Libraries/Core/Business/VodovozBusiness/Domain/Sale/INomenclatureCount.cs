using Vodovoz.Domain.Goods;

namespace VodovozBusiness.Domain.Sale
{
	public interface INomenclatureCount : ISetCount
	{
		/// <summary>
		/// Номенклатура <see cref="Vodovoz.Domain.Goods.Nomenclature"/>
		/// </summary>
		Nomenclature Nomenclature { get; }
	}
}
