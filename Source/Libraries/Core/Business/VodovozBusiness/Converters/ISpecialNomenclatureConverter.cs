using TaxcomEdo.Contracts.Goods;
using Vodovoz.Core.Domain.Clients;

namespace Vodovoz.Converters
{
	public interface ISpecialNomenclatureConverter
	{
		/// <summary>
		/// Конвертация спец номенклатуры <see cref="SpecialNomenclature"/>
		/// в информацию о ней для ЭДО <see cref="SpecialNomenclatureInfoForEdo"/>
		/// </summary>
		/// <param name="specialNomenclature">Спец номенклатура</param>
		/// <returns>Информация о спец номенклатуре для ЭДО</returns>
		SpecialNomenclatureInfoForEdo ConvertSpecialNomenclatureToSpecialNomenclatureInfoForEdo(
			SpecialNomenclature specialNomenclature);
	}
}
