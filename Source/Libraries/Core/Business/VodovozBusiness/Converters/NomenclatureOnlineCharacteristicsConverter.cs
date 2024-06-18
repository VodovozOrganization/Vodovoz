using Gamma.Utilities;
using Vodovoz.Domain.Goods;

namespace Vodovoz.Converters
{
	public class NomenclatureOnlineCharacteristicsConverter : INomenclatureOnlineCharacteristicsConverter
	{
		public string GetSizeString(int? length, int? width, int? height)
		{
			if(!length.HasValue || !width.HasValue || !height.HasValue)
			{
				return null;
			}
			
			return $"{length}*{width}*{height}";
		}
		
		public string GetWeightString(decimal? weight)
		{
			if(!weight.HasValue)
			{
				return null;
			}
			
			return $"{weight}кг";
		}
		
		public string GetPowerString(int? power, PowerUnits? powerUnits)
		{
			if(!power.HasValue)
			{
				return null;
			}
			
			var units = powerUnits?.GetEnumTitle();
			
			return $"{power}{units}";
		}

		public string GetProductivityString(
			ProductivityComparisionSign? productivitySign,
			decimal? productivity,
			ProductivityUnits? productivityUnits)
		{
			if(!productivity.HasValue)
			{
				return null;
			}
			
			var sign = productivitySign?.GetEnumTitle();
			var units = productivityUnits?.GetEnumTitle();
			
			return $"{sign} {productivity}{units}";
		}

		public string GetTemperatureString(int? fromValue, int? toValue)
		{
			if(fromValue.HasValue && toValue.HasValue)
			{
				return $"{fromValue}-{toValue}C";
			}

			if(!fromValue.HasValue && toValue.HasValue)
			{
				return $"до {toValue}C";
			}
		
			if(fromValue.HasValue && !toValue.HasValue)
			{
				return $"от {fromValue}C";
			}

			return null;
		}
	}
}
