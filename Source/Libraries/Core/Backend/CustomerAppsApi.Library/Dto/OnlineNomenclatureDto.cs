using System;
using System.Text.Json.Serialization;
using Vodovoz.Domain.Goods;
using Vodovoz.Nodes;

namespace CustomerAppsApi.Library.Dto
{
	public class OnlineNomenclatureDto
	{
		public int ErpId { get; set; }
		public Guid OnlineCatalogGuid { get; set; }
		public string OnlineGroup { get; set; }
		public string OnlineCategory { get; set; }
		public string OnlineName { get; set; }
		[JsonConverter(typeof(JsonStringEnumConverter))]
		public TareVolume? TareVolume { get; set; }
		public bool IsDisposableTare { get; set; }
		public bool IsNewBottle { get; set; }
		public bool IsSparklingWater { get; set; }
		[JsonConverter(typeof(JsonStringEnumConverter))]
		public EquipmentInstallationType? EquipmentInstallationType { get; set; }
		[JsonConverter(typeof(JsonStringEnumConverter))]
		public EquipmentWorkloadType? EquipmentWorkloadType { get; set; }
		[JsonConverter(typeof(JsonStringEnumConverter))]
		public PumpType? PumpType { get; set; }
		[JsonConverter(typeof(JsonStringEnumConverter))]
		public CupHolderBracingType? CupHolderBracingType { get; set; }
		public bool? HasHeating { get; set; }
		public int? HeatingPower { get; set; }
		public int? HeatingProductivity { get; set; }
		[JsonConverter(typeof(JsonStringEnumConverter))]
		public ProtectionOnHotWaterTap? ProtectionOnHotWaterTap { get; set; }
		public bool? HasCooling { get; set; }
		public int? CoolingPower { get; set; }
		public int? CoolingProductivity { get; set; }
		[JsonConverter(typeof(JsonStringEnumConverter))]
		public CoolingType? CoolingType { get; set; }
		[JsonConverter(typeof(JsonStringEnumConverter))]
		public LockerRefrigeratorType? LockerRefrigeratorType { get; set; }
		public int? LockerRefrigeratorVolume { get; set; }
		[JsonConverter(typeof(JsonStringEnumConverter))]
		public TapType? TapType { get; set; }
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
