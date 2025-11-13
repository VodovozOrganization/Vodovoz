using TaxcomEdo.Contracts.Goods;
using Vodovoz.Domain.Goods;

namespace Vodovoz.Converters
{
	public interface INomenclatureConverter
	{
		/// <summary>
		/// Конвертация номенклатуры <see cref="Nomenclature"/> в информацию о ней для ЭДО <see cref="NomenclatureInfoForEdo"/>
		/// </summary>
		/// <param name="nomenclature">Номенклатура</param>
		/// <returns>Информация о номенклатуре для ЭДО</returns>
		NomenclatureInfoForEdo ConvertNomenclatureToNomenclatureInfoForEdo(Nomenclature nomenclature);
	}
}
