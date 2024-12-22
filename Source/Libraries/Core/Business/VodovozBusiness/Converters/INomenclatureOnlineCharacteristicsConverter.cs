using Vodovoz.Core.Domain.Goods;

namespace Vodovoz.Converters
{
	/// <summary>
	/// Конвертер числовых значений характеристик номенклатуры в строку
	/// </summary>
	public interface INomenclatureOnlineCharacteristicsConverter
	{
		/// <summary>
		/// Получение строкового значения размеров
		/// </summary>
		/// <param name="length">длина</param>
		/// <param name="width">ширина</param>
		/// <param name="height">высота</param>
		/// <returns>Строка в формате: Д*Ш*В(мм)</returns>
		string GetSizeString(int? length, int? width, int? height);
		/// <summary>
		/// Получение строкового значения веса
		/// </summary>
		/// <param name="weight">вес</param>
		/// <returns>Строка в формате: Вес кг</returns>
		string GetWeightString(decimal? weight);
		/// <summary>
		/// Получение строкового мощности нагрева/охлаждения
		/// </summary>
		/// <param name="power">мощность</param>
		/// <param name="powerUnits">ед измерения</param>
		/// <returns>Строка в формате: Мощность ед. изм</returns>
		string GetPowerString(int? power, PowerUnits? powerUnits);
		/// <summary>
		/// Получение строкового значения производительности нагрева/охлаждения
		/// </summary>
		/// <param name="productivitySign">показатель</param>
		/// <param name="productivity">производительность</param>
		/// <param name="productivityUnits">ед измерения</param>
		/// <returns>Строка в формате: Показатель Производительность ед. изм</returns>
		string GetProductivityString(
			ProductivityComparisionSign? productivitySign,
			decimal? productivity,
			ProductivityUnits? productivityUnits);
		/// <summary>
		/// Получение строкового значения температуры нагрева/охлаждения
		/// </summary>
		/// <param name="fromValue">значение от</param>
		/// <param name="toValue">значение до</param>
		/// <returns>
		/// Строка в формате
		/// Если заполнены оба параметра: от-до гр Цельсия
		/// Если заполнено только От: от ЗначениеОт гр Цельсия
		/// Если заполнено только До: до ЗначениеДо гр Цельсия
		/// Иначе null
		/// </returns>
		string GetTemperatureString(int? fromValue, int? toValue);
	}
}
