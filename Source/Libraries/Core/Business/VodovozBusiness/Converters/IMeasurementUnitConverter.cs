using QS.BusinessCommon.Domain;
using Vodovoz.Core.Data.Goods;

namespace Vodovoz.Converters
{
	public interface IMeasurementUnitConverter
	{
		/// <summary>
		/// Конвертация единиц измерения номенклатуры <see cref="MeasurementUnits"/>
		/// в информацию о них для ЭДО <see cref="MeasurementUnitInfoForEdo"/>
		/// </summary>
		/// <param name="measurementUnit">Единица измерения номенклатуры</param>
		/// <returns>Информация о ед. измерения для ЭДО</returns>
		MeasurementUnitInfoForEdo ConvertMeasurementUnitToMeasurementUnitInfoForEdo(MeasurementUnits measurementUnit);
	}
}
