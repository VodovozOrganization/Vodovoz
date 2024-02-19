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
		public int? HeatingProductivity { get; set; }
		public ProtectionOnHotWaterTap? ProtectionOnHotWaterTap { get; set; }
		public bool? HasCooling { get; set; }
		public int? CoolingPower { get; set; }
		public int? CoolingProductivity { get; set; }
		public CoolingType? CoolingType { get; set; }
		public LockerRefrigeratorType? LockerRefrigeratorType { get; set; }
		public int? LockerRefrigeratorVolume { get; set; }
		public TapType? TapType { get; set; }
		public GlassHolderType? GlassHolderType { get; set; }
	}
}
