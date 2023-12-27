using System;
using System.Text.Json.Serialization;
using Vodovoz.Domain.Goods;
using Vodovoz.Nodes;

namespace CustomerAppsApi.Library.Dto
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
		public int? HeatingProductivity { get; set; }
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
		public int? CoolingProductivity { get; set; }
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

		public static OnlineNomenclatureDto Create(OnlineNomenclatureNode node)
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
				GlassHolderType = node.GlassHolderType
			};
		}
	}
}
