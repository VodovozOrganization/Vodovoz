using System;
using Vodovoz.Domain.Goods;

namespace Vodovoz.Nodes
{
	public class OnlineNomenclatureNode
	{
		public int ErpId { get; set; }
		public Guid OnlineCatalogGuid { get; set; }
		public string OnlineGroup { get; set; }
		public string OnlineCategory { get; set; }
		public string OnlineName { get; set; }
		public TareVolume? TareVolume { get; set; }
		public bool IsDisposableTare { get; set; }
		public bool IsNewBottle { get; set; }
		public bool IsSparklingWater { get; set; }
		public EquipmentInstallationType? EquipmentInstallationType { get; set; }
		public EquipmentWorkloadType? EquipmentWorkloadType { get; set; }
		public PumpType? PumpType { get; set; }
		public CupHolderBracingType? CupHolderBracingType { get; set; }
		public bool? HasHeating { get; set; }
		public int? HeatingPower { get; set; }
		public decimal? HeatingProductivity { get; set; }
		public ProtectionOnHotWaterTap? ProtectionOnHotWaterTap { get; set; }
		public bool? HasCooling { get; set; }
		public int? CoolingPower { get; set; }
		public decimal? CoolingProductivity { get; set; }
		public CoolingType? CoolingType { get; set; }
		public LockerRefrigeratorType? LockerRefrigeratorType { get; set; }
		public int? LockerRefrigeratorVolume { get; set; }
		public TapType? TapType { get; set; }
		public GlassHolderType? GlassHolderType { get; set; }
		public int? HeatingTemperatureFrom { get; set; }
		public int? HeatingTemperatureTo { get; set; }
		public int? CoolingTemperatureFrom { get; set; }
		public int? CoolingTemperatureTo { get; set; }
		public int? Length { get; set; }
		public int? Width { get; set; }
		public int? Height { get; set; }
		public decimal? Weight { get; set; }
		public PowerUnits? HeatingPowerUnits { get; set; }
		public PowerUnits? CoolingPowerUnits { get; set; }
		public ProductivityUnits? HeatingProductivityUnits { get; set; }
		public ProductivityUnits? CoolingProductivityUnits { get; set; }
		public ProductivityComparisionSign? HeatingProductivityComparisionSign { get; set; }
		public ProductivityComparisionSign? CoolingProductivityComparisionSign { get; set; }
	}
}
