using QS.BusinessCommon.Domain;
using Vodovoz.Core.Data.Goods;

namespace Vodovoz.Converters
{
	public class MeasurementUnitConverter : IMeasurementUnitConverter
	{
		public MeasurementUnitInfoForEdo ConvertMeasurementUnitToMeasurementUnitInfoForEdo(MeasurementUnits measurementUnit)
		{
			return new MeasurementUnitInfoForEdo
			{
				Id = measurementUnit.Id,
				Digits = measurementUnit.Digits,
				Name = measurementUnit.Name,
				OKEI = measurementUnit.OKEI
			};
		}
	}
}
