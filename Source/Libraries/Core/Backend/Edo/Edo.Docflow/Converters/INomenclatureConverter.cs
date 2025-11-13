using TaxcomEdo.Contracts.Goods;
using Vodovoz.Core.Domain.Goods;

namespace Edo.Docflow.Converters
{
	public interface INomenclatureConverter
	{
		/// <summary>
		/// Конвертация номенклатуры <see cref="NomenclatureEntity"/> в информацию о ней для ЭДО <see cref="NomenclatureInfoForEdo"/>
		/// </summary>
		/// <param name="nomenclature">Номенклатура</param>
		/// <returns>Информация о номенклатуре для ЭДО</returns>
		NomenclatureInfoForEdo ConvertNomenclatureToNomenclatureInfoForEdo(NomenclatureEntity nomenclature);
	}
}
