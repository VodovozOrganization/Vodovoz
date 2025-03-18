using System;
using Vodovoz.Core.Domain.Goods;

namespace Vodovoz.Nodes
{
	/// <summary>
	/// Данные номенклатуры для передачи в ИПЗ
	/// </summary>
	public class OnlineNomenclatureNode
	{
		/// <summary>
		/// Id номенклатуры в ДВ
		/// </summary>
		public int ErpId { get; set; }
		/// <summary>
		/// Id каталога в ИПЗ
		/// </summary>
		public Guid OnlineCatalogGuid { get; set; }
		/// <summary>
		/// Группа товара в ИПЗ
		/// </summary>
		public string OnlineGroup { get; set; }
		/// <summary>
		/// Категория товара в ИПЗ
		/// </summary>
		public string OnlineCategory { get; set; }
		/// <summary>
		/// Название товара в ИПЗ
		/// </summary>
		public string OnlineName { get; set; }
		/// <summary>
		/// Объем тары
		/// </summary>
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
		/// Тип установки оборудования <see cref="Core.Domain.Goods.EquipmentInstallationType"/>
		/// </summary>
		public EquipmentInstallationType? EquipmentInstallationType { get; set; }
		/// <summary>
		/// Способ загрузки <see cref="Core.Domain.Goods.EquipmentWorkloadType"/>
		/// </summary>
		public EquipmentWorkloadType? EquipmentWorkloadType { get; set; }
		/// <summary>
		/// Тип помпы <see cref="Core.Domain.Goods.PumpType"/>
		/// </summary>
		public PumpType? PumpType { get; set; }
		/// <summary>
		/// Тип крепления стаканодержателя <see cref="Core.Domain.Goods.CupHolderBracingType"/>
		/// </summary>
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
		/// Защита на кране с горячей водой <see cref="Core.Domain.Goods.ProtectionOnHotWaterTap"/>
		/// </summary>
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
		/// Тип охлаждения <see cref="Core.Domain.Goods.CoolingType"/>
		/// </summary>
		public CoolingType? CoolingType { get; set; }
		/// <summary>
		/// Шкафчик/Холодильник <see cref="Core.Domain.Goods.LockerRefrigeratorType"/>
		/// </summary>
		public LockerRefrigeratorType? LockerRefrigeratorType { get; set; }
		/// <summary>
		/// Объем шкафчика/холодильника
		/// </summary>
		public int? LockerRefrigeratorVolume { get; set; }
		/// <summary>
		/// Тип кранов <see cref="Core.Domain.Goods.TapType"/>
		/// </summary>
		public TapType? TapType { get; set; }
		/// <summary>
		/// Тип стаканодержателя <see cref="Core.Domain.Goods.GlassHolderType"/>
		/// </summary>
		public GlassHolderType? GlassHolderType { get; set; }
		/// <summary>
		/// Температура нагрева (от)
		/// </summary>
		public int? HeatingTemperatureFrom { get; set; }
		/// <summary>
		/// Температура нагрева (до)
		/// </summary>
		public int? HeatingTemperatureTo { get; set; }
		/// <summary>
		/// Температура охлаждения (от)
		/// </summary>
		public int? CoolingTemperatureFrom { get; set; }
		/// <summary>
		/// Температура охлаждения (до)
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
		/// Единицы мощности нагрева <see cref="Core.Domain.Goods.PowerUnits"/>
		/// </summary>
		public PowerUnits? HeatingPowerUnits { get; set; }
		/// <summary>
		/// Единицы мощности охлаждения <see cref="Core.Domain.Goods.PowerUnits"/>
		/// </summary>
		public PowerUnits? CoolingPowerUnits { get; set; }
		/// <summary>
		/// Единицы производительности нагрева <see cref="Core.Domain.Goods.ProductivityUnits"/>
		/// </summary>
		public ProductivityUnits? HeatingProductivityUnits { get; set; }
		/// <summary>
		/// Единицы производительности охлаждения <see cref="Core.Domain.Goods.ProductivityUnits"/>
		/// </summary>
		public ProductivityUnits? CoolingProductivityUnits { get; set; }
		/// <summary>
		/// Показатель производительности нагрева <see cref="Core.Domain.Goods.ProductivityComparisionSign"/>
		/// </summary>
		public ProductivityComparisionSign? HeatingProductivityComparisionSign { get; set; }
		/// <summary>
		/// Показатель производительности охлаждения <see cref="Core.Domain.Goods.ProductivityComparisionSign"/>
		/// </summary>
		public ProductivityComparisionSign? CoolingProductivityComparisionSign { get; set; }
	}
}
