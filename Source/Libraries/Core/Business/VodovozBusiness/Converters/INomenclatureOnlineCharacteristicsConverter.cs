using Vodovoz.Domain.Goods;

namespace Vodovoz.Converters
{
	public interface INomenclatureOnlineCharacteristicsConverter
	{
		string GetSizeString(int? length, int? width, int? height);
		string GetWeightString(decimal? weight);
		string GetPowerString(int? power, PowerUnits? powerUnits);

		string GetProductivityString(
			ProductivityComparisionSign? productivitySign,
			decimal? productivity,
			ProductivityUnits? productivityUnits);

		string GetTemperatureString(int? fromValue, int? toValue);
	}
}
