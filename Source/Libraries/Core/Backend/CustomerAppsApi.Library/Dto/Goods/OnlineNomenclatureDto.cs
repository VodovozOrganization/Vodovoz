using System;
using System.Text.Json.Serialization;
using Vodovoz.Converters;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Nodes;

namespace CustomerAppsApi.Library.Dto.Goods
{
	/// <summary>
	/// Номенклатуры продающиеся в ИПЗ
	/// </summary>
	public class OnlineNomenclatureDto
	{
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
		public int? HeatingPower { get; set; }
		/// <summary>
		/// Производительность нагрева
		/// </summary>
		public decimal? HeatingProductivity { get; set; }
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
		public int? CoolingPower { get; set; }
		/// <summary>
		/// Производительность охлаждения
		/// </summary>
		public decimal? CoolingProductivity { get; set; }
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
		public int? HeatingTemperatureFrom { get; set; }
		/// <summary>
		/// Температура нагрева до
		/// </summary>
		public int? HeatingTemperatureTo { get; set; }
		/// <summary>
		/// Температура охлаждения от
		/// </summary>
		public int? CoolingTemperatureFrom { get; set; }
		/// <summary>
		/// Температура охлаждения до
		/// </summary>
		public int? CoolingTemperatureTo { get; set; }
		/// <summary>
		/// Длина
		/// </summary>
		public int? Length { get; set; }
		/// <summary>
		/// Ширина
		/// </summary>
		public int? Width { get; set; }
		/// <summary>
		/// Высота
		/// </summary>
		public int? Height { get; set; }
		/// <summary>
		/// Вес
		/// </summary>
		public decimal? Weight { get; set; }
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

		public static OnlineNomenclatureDto Create(
			INomenclatureOnlineCharacteristicsConverter onlineCharacteristicsConverter, OnlineNomenclatureNode node)
		{
			return new OnlineNomenclatureDto
			{
				ErpId = node.ErpId,
				OnlineCatalogGuid = node.OnlineCatalogGuid,
				OnlineGroup = node.OnlineGroup,
				OnlineCategory = node.OnlineCategory,
				OnlineName = node.OnlineName,
				TareVolume = node.TareVolume,
				IsDisposableTare = node.IsDisposableTare,
				IsNewBottle = node.IsNewBottle,
				IsSparklingWater = node.IsSparklingWater,
				EquipmentInstallationType = node.EquipmentInstallationType,
				EquipmentWorkloadType = node.EquipmentWorkloadType,
				PumpType = node.PumpType,
				CupHolderBracingType = node.CupHolderBracingType,
				HasHeating = node.HasHeating,
				HeatingPower = node.HeatingPower,
				HeatingProductivity = node.HeatingProductivity,
				ProtectionOnHotWaterTap = node.ProtectionOnHotWaterTap,
				HasCooling = node.HasCooling,
				CoolingPower = node.CoolingPower,
				CoolingProductivity = node.CoolingProductivity,
				CoolingType = node.CoolingType,
				LockerRefrigeratorType = node.LockerRefrigeratorType,
				LockerRefrigeratorVolume = node.LockerRefrigeratorVolume,
				TapType = node.TapType,
				GlassHolderType = node.GlassHolderType,
				HeatingTemperatureFrom = node.HeatingTemperatureFrom,
				HeatingTemperatureTo = node.HeatingTemperatureTo,
				CoolingTemperatureFrom = node.CoolingTemperatureFrom,
				CoolingTemperatureTo = node.CoolingTemperatureTo,
				Length = node.Length,
				Width = node.Width,
				Height = node.Height,
				Weight = node.Weight,
				Size = onlineCharacteristicsConverter.GetSizeString(node.Length, node.Width, node.Height),
				WeightString = onlineCharacteristicsConverter.GetWeightString(node.Weight),
				HeatingProductivityString = onlineCharacteristicsConverter.GetProductivityString(
					node.HeatingProductivityComparisionSign, node.HeatingProductivity, node.HeatingProductivityUnits),
				HeatingPowerString = onlineCharacteristicsConverter.GetPowerString(node.HeatingPower, node.HeatingPowerUnits),
				CoolingProductivityString = onlineCharacteristicsConverter.GetProductivityString(
					node.CoolingProductivityComparisionSign, node.CoolingProductivity, node.CoolingProductivityUnits),
				CoolingPowerString = onlineCharacteristicsConverter.GetPowerString(node.CoolingPower, node.CoolingPowerUnits),
				HeatingTemperatureString =
					onlineCharacteristicsConverter.GetTemperatureString(node.HeatingTemperatureFrom, node.HeatingTemperatureTo),
				CoolingTemperatureString =
					onlineCharacteristicsConverter.GetTemperatureString(node.CoolingTemperatureFrom, node.CoolingTemperatureTo)
			};
		}
	}
}
