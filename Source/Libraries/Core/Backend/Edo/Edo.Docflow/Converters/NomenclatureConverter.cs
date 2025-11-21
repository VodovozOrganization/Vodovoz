using System;
using TaxcomEdo.Contracts.Goods;
using Vodovoz.Converters;
using Vodovoz.Core.Domain.Goods;
using VodovozBusiness.Converters;

namespace Edo.Docflow.Converters
{
	public class NomenclatureConverter : INomenclatureConverter
	{
		private readonly IMeasurementUnitConverter _measurementUnitConverter;
		private readonly INomenclatureCategoryConverter _nomenclatureCategoryConverter;

		public NomenclatureConverter(
			IMeasurementUnitConverter measurementUnitConverter,
			INomenclatureCategoryConverter nomenclatureCategoryConverter)
		{
			_measurementUnitConverter = measurementUnitConverter ?? throw new ArgumentNullException(nameof(measurementUnitConverter));
			_nomenclatureCategoryConverter =
				nomenclatureCategoryConverter ?? throw new ArgumentNullException(nameof(nomenclatureCategoryConverter));
		}
		
		public NomenclatureInfoForEdo ConvertNomenclatureToNomenclatureInfoForEdo(NomenclatureEntity nomenclature)
		{
			var measurementUnitInfo = _measurementUnitConverter.ConvertMeasurementUnitToMeasurementUnitInfoForEdo(nomenclature.Unit);
			
			var nomenclatureInfo = new NomenclatureInfoForEdo
			{
				Id = nomenclature.Id,
				Category = _nomenclatureCategoryConverter.ConvertNomenclatureCategoryToNomenclatureInfoCategory(nomenclature.Category),
				OfficialName = nomenclature.OfficialName,
				Gtin = nomenclature.Gtin,
				MeasurementUnitInfoForEdo = measurementUnitInfo
			};

			return nomenclatureInfo;
		}
	}
}
