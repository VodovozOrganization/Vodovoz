using TaxcomEdo.Contracts.Goods;
using Vodovoz.Domain.Client;

namespace Vodovoz.Converters
{
	public class SpecialNomenclatureConverter : ISpecialNomenclatureConverter
	{
		public SpecialNomenclatureInfoForEdo ConvertSpecialNomenclatureToSpecialNomenclatureInfoForEdo(
			SpecialNomenclature specialNomenclature)
		{
			return new SpecialNomenclatureInfoForEdo
			{
				Id = specialNomenclature.Id,
				SpecialId = specialNomenclature.SpecialId,
				NomenclatureId = specialNomenclature.Nomenclature.Id,
				CounterpartyId = specialNomenclature.Counterparty.Id
			};
		}
	}
}
