using System;
using System.Text.Json.Serialization;
using Gamma.Utilities;
using Vodovoz.Core.Domain.Goods;

namespace CustomerAppsApi.Library.V1.Dto.Goods
{
	/// <summary>
	/// Номенклатуры продающиеся в ИПЗ
	/// </summary>
	public class OnlineNomenclatureDto
	{
		private int? _length;
		private int? _width;
		private int? _height;
		private decimal? _weight;
		private decimal? _heatingProductivity;
		private decimal? _coolingProductivity;
		private int? _heatingPower;
		private int? _coolingPower;
		private PowerUnits? _heatingPowerUnits;
		private PowerUnits? _coolingPowerUnits;
		private ProductivityUnits? _heatingProductivityUnits;
		private ProductivityUnits? _coolingProductivityUnits;
		private ProductivityComparisionSign? _heatingProductivityComparisionSign;
		private ProductivityComparisionSign? _coolingProductivityComparisionSign;
		private int? _heatingTemperatureFrom;
		private int? _heatingTemperatureTo;
		private int? _coolingTemperatureFrom;
		private int? _coolingTemperatureTo;

		/// <summary>
		/// Id номенклатуры в ДВ
		/// </summary>
		public int ErpId { get; set; }
		/// <summary>
		/// Guid онлайн каталога в ИПЗ
		/// </summary>
		public Guid OnlineCatalogGuid { get; set; }
		/// <summary>
		/// Группа товара в ИПЗ
		/// </summary>
		public string OnlineGroup { get; set; }
		/// <summary>
		/// Тип товара в ИПЗ
		/// </summary>
		public string OnlineCategory { get; set; }
		/// <summary>
		/// Наименование товара в ИПЗ
		/// </summary>
		public string OnlineName { get; set; }
		/// <summary>
		/// Объем тары
		/// </summary>
		[JsonConverter(typeof(JsonStringEnumConverter))]
		public TareVolume? TareVolume { get; set; }
		/// <summary>
		/// Одноразовая тара
		/// </summary>
		public bool IsDisposableTare { get; set; }
		/// <summary>
		/// Новая бутыль
		/// </summary>
		public bool IsNewBottle { get; set; }
		/// <summary>
		/// Газированная вода
		/// </summary>
		public bool IsSparklingWater { get; set; }
		/// <summary>
		/// Тип установки оборудования(кулер, пурифайер)
		/// </summary>
		[JsonConverter(typeof(JsonStringEnumConverter))]
		public EquipmentInstallationType? EquipmentInstallationType { get; set; }
		/// <summary>
		/// Тип загрузки
		/// </summary>
		[JsonConverter(typeof(JsonStringEnumConverter))]
		public EquipmentWorkloadType? EquipmentWorkloadType { get; set; }
		/// <summary>
		/// Тип помпы
		/// </summary>
		[JsonConverter(typeof(JsonStringEnumConverter))]
		public PumpType? PumpType { get; set; }
		/// <summary>
		/// Тип крепления стаканодержателя
		/// </summary>
		[JsonConverter(typeof(JsonStringEnumConverter))]
		public CupHolderBracingType? CupHolderBracingType { get; set; }
		/// <summary>
		/// Нагрев
		/// </summary>
		public bool? HasHeating { get; set; }
		/// <summary>
		/// Мощность нагрева
		/// </summary>
		public int? HeatingPower
		{
			get => _heatingPower;
			set
			{
				_heatingPower = value;
				HeatingPowerString = GetPowerString(_heatingPower, _heatingPowerUnits);
			}
		}
		
		/// <summary>
		/// Единицы измерения мощности нагрева
		/// </summary>
		[JsonIgnore]
		public PowerUnits? HeatingPowerUnits
		{
			get => _heatingPowerUnits;
			set
			{
				_heatingPowerUnits = value;
				HeatingPowerString = GetPowerString(_heatingPower, _heatingPowerUnits);
			}
		}

		/// <summary>
		/// Производительность нагрева
		/// </summary>
		public decimal? HeatingProductivity
		{
			get => _heatingProductivity;
			set
			{
				_heatingProductivity = value;
				HeatingProductivityString =
					GetProductivityString(_heatingProductivityComparisionSign, _heatingProductivity, _heatingProductivityUnits);
			}
		}
		/// <summary>
		/// Единицы измерения производительности нагрева
		/// </summary>
		[JsonIgnore]
		public ProductivityUnits? HeatingProductivityUnits
		{
			get => _heatingProductivityUnits;
			set
			{
				_heatingProductivityUnits = value;
				HeatingProductivityString =
					GetProductivityString(_heatingProductivityComparisionSign, _heatingProductivity, _heatingProductivityUnits);
			}
		}
		/// <summary>
		/// Показатель производительности нагрева
		/// </summary>
		[JsonIgnore]
		public ProductivityComparisionSign? HeatingProductivityComparisionSign
		{
			get => _heatingProductivityComparisionSign;
			set
			{
				_heatingProductivityComparisionSign = value;
				HeatingProductivityString =
					GetProductivityString(_heatingProductivityComparisionSign, _heatingProductivity, _heatingProductivityUnits);
			}
		}
		/// <summary>
		/// Защита на кране горячей воды
		/// </summary>
		[JsonConverter(typeof(JsonStringEnumConverter))]
		public ProtectionOnHotWaterTap? ProtectionOnHotWaterTap { get; set; }
		/// <summary>
		/// Охлаждение
		/// </summary>
		public bool? HasCooling { get; set; }
		/// <summary>
		/// Мощность охлаждения
		/// </summary>
		public int? CoolingPower
		{
			get => _coolingPower;
			set
			{
				_coolingPower = value;
				CoolingPowerString = GetPowerString(_coolingPower, _coolingPowerUnits);
			}
		}
		/// <summary>
		/// Единицы измерения мощности охлаждения
		/// </summary>
		[JsonIgnore]
		public PowerUnits? CoolingPowerUnits
		{
			get => _coolingPowerUnits;
			set
			{
				_coolingPowerUnits = value;
				CoolingPowerString = GetPowerString(_coolingPower, _coolingPowerUnits);
			}
		}

		/// <summary>
		/// Производительность охлаждения
		/// </summary>
		public decimal? CoolingProductivity
		{
			get => _coolingProductivity;
			set
			{
				_coolingProductivity = value;
				CoolingProductivityString =
					GetProductivityString(_coolingProductivityComparisionSign, _coolingProductivity, _coolingProductivityUnits);
			}
		}
		/// <summary>
		/// Единицы измерения производительности охлаждения
		/// </summary>
		[JsonIgnore]
		public ProductivityUnits? CoolingProductivityUnits
		{
			get => _coolingProductivityUnits;
			set
			{
				_coolingProductivityUnits = value;
				CoolingProductivityString =
					GetProductivityString(_coolingProductivityComparisionSign, _coolingProductivity, _coolingProductivityUnits);
			}
		}
		/// <summary>
		/// Показатель производительности охлаждения
		/// </summary>
		[JsonIgnore]
		public ProductivityComparisionSign? CoolingProductivityComparisionSign
		{
			get => _coolingProductivityComparisionSign;
			set
			{
				_coolingProductivityComparisionSign = value;
				CoolingProductivityString =
					GetProductivityString(_coolingProductivityComparisionSign, _coolingProductivity, _coolingProductivityUnits);
			}
		}

		/// <summary>
		/// Тип охлаждения
		/// </summary>
		[JsonConverter(typeof(JsonStringEnumConverter))]
		public CoolingType? CoolingType { get; set; }
		/// <summary>
		/// Наличие шкафчика или холодильника
		/// </summary>
		[JsonConverter(typeof(JsonStringEnumConverter))]
		public LockerRefrigeratorType? LockerRefrigeratorType { get; set; }
		/// <summary>
		/// Объем шкафчика или холодильника
		/// </summary>
		public int? LockerRefrigeratorVolume { get; set; }
		/// <summary>
		/// Тип кранов
		/// </summary>
		[JsonConverter(typeof(JsonStringEnumConverter))]
		public TapType? TapType { get; set; }
		/// <summary>
		/// Тип стаканодержателя
		/// </summary>
		[JsonConverter(typeof(JsonStringEnumConverter))]
		public GlassHolderType? GlassHolderType { get; set; }
		/// <summary>
		/// Температура нагрева от
		/// </summary>
		public int? HeatingTemperatureFrom
		{
			get => _heatingTemperatureFrom;
			set
			{
				_heatingTemperatureFrom = value;
				HeatingTemperatureString = GetTemperatureString(_heatingTemperatureFrom, _heatingTemperatureTo);
			}
		}
		/// <summary>
		/// Температура нагрева до
		/// </summary>
		public int? HeatingTemperatureTo
		{
			get => _heatingTemperatureTo;
			set
			{
				_heatingTemperatureTo = value;
				HeatingTemperatureString = GetTemperatureString(_heatingTemperatureFrom, _heatingTemperatureTo);
			}
		}

		/// <summary>
		/// Температура охлаждения от
		/// </summary>
		public int? CoolingTemperatureFrom
		{
			get => _coolingTemperatureFrom;
			set
			{
				_coolingTemperatureFrom = value;
				CoolingTemperatureString = GetTemperatureString(_coolingTemperatureFrom, _coolingTemperatureTo);
			}
		}

		/// <summary>
		/// Температура охлаждения до
		/// </summary>
		public int? CoolingTemperatureTo
		{
			get => _coolingTemperatureTo;
			set
			{
				_coolingTemperatureTo = value;
				CoolingTemperatureString = GetTemperatureString(_coolingTemperatureFrom, _coolingTemperatureTo);
			}
		}

		/// <summary>
		/// Длина
		/// </summary>
		public int? Length
		{
			get => _length;
			set
			{
				_length = value;
				Size = GetSizeString(_length, _width, _height);
			}
		}
		/// <summary>
		/// Ширина
		/// </summary>
		public int? Width
		{
			get => _width;
			set
			{
				_width = value;
				Size = GetSizeString(_length, _width, _height);
			}
		}
		/// <summary>
		/// Высота
		/// </summary>
		public int? Height
		{
			get => _height;
			set
			{
				_height = value;
				Size = GetSizeString(_length, _width, _height);
			}
		}
		/// <summary>
		/// Вес
		/// </summary>
		public decimal? Weight
		{
			get => _weight;
			set
			{
				_weight = value;
				WeightString = GetWeightString(_weight);
			}
		}
		/// <summary>
		/// Строковое представление размеров
		/// </summary>
		public string Size { get; set; }
		/// <summary>
		/// Строковое представление веса
		/// </summary>
		public string WeightString { get; set; }
		/// <summary>
		/// Строковое представление производительности нагрева
		/// </summary>
		public string HeatingProductivityString { get; set; }
		/// <summary>
		/// Строковое представление мощности нагрева
		/// </summary>
		public string HeatingPowerString { get; set; }
		/// <summary>
		/// Строковое представление производительности охлаждения
		/// </summary>
		public string CoolingProductivityString { get; set; }
		/// <summary>
		/// Строковое представление мощности охлаждения
		/// </summary>
		public string CoolingPowerString { get; set; }
		/// <summary>
		/// Строковое представление температуры нагрева
		/// </summary>
		public string HeatingTemperatureString { get; set; }
		/// <summary>
		/// Строковое представление температуры охлаждения
		/// </summary>
		public string CoolingTemperatureString { get; set; }
		
		private string GetSizeString(int? length, int? width, int? height)
		{
			if(!length.HasValue || !width.HasValue || !height.HasValue)
			{
				return null;
			}
			
			return $"{length}*{width}*{height}(мм)";
		}
		
		private string GetWeightString(decimal? weight)
		{
			if(!weight.HasValue)
			{
				return null;
			}
			
			return $"{weight}кг";
		}
		
		private string GetPowerString(int? power, PowerUnits? powerUnits)
		{
			if(!power.HasValue)
			{
				return null;
			}
			
			var units = powerUnits?.GetEnumShortTitle();
			
			return $"{power}{units}";
		}

		private string GetProductivityString(
			ProductivityComparisionSign? productivitySign,
			decimal? productivity,
			ProductivityUnits? productivityUnits)
		{
			if(!productivity.HasValue)
			{
				return null;
			}
			
			var sign = productivitySign?.GetEnumTitle();
			var units = productivityUnits?.GetEnumShortTitle();
			
			return $"{sign} {productivity}{units}";
		}

		private string GetTemperatureString(int? fromValue, int? toValue)
		{
			if(fromValue.HasValue && toValue.HasValue)
			{
				return $"{fromValue}-{toValue}(\u00b0C)";
			}

			if(!fromValue.HasValue && toValue.HasValue)
			{
				return $"до {toValue}\u00b0C";
			}
		
			if(fromValue.HasValue && !toValue.HasValue)
			{
				return $"от {fromValue}\u00b0C";
			}

			return null;
		}
	}
}
