using System;
using Vodovoz.Core.Data.Goods;
using Vodovoz.Domain.Goods;

namespace Vodovoz.Converters
{
	public class NomenclatureConverter : INomenclatureConverter
	{
		private readonly IMeasurementUnitConverter _measurementUnitConverter;

		public NomenclatureConverter(IMeasurementUnitConverter measurementUnitConverter)
		{
			_measurementUnitConverter = measurementUnitConverter ?? throw new ArgumentNullException(nameof(measurementUnitConverter));
		}
		
		public NomenclatureInfoForEdo ConvertNomenclatureToNomenclatureInfoForEdo(Nomenclature nomenclature)
		{
			var measurementUnitInfo = _measurementUnitConverter.ConvertMeasurementUnitToMeasurementUnitInfoForEdo(nomenclature.Unit);
			
			var nomenclatureInfo = new NomenclatureInfoForEdo
			{
				Id = nomenclature.Id,
				Category = nomenclature.Category,
				OfficialName = nomenclature.OfficialName,
				Gtin = nomenclature.Gtin,
				MeasurementUnitInfoForEdo = measurementUnitInfo
			};

			return nomenclatureInfo;
		}
	}
}
