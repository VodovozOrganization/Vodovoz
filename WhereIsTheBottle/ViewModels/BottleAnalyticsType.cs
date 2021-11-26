using System.ComponentModel.DataAnnotations;

namespace WhereIsTheBottle.ViewModels
{
	public enum BottleAnalyticsType
	{
		[Display(Name = "Общая сводка")]
		GeneralSummary,
		[Display(Name = "Дельта")]
		GeneralDelta,
		[Display(Name = "Актив")]
		GeneralAsset,
		[Display(Name = "Дельта - Потери")]
		DeltaLoss,
		[Display(Name = "Дельта - Стройка")]
		DeltaShabby,
		[Display(Name = "Дельта - Брак")]
		DeltaDefective,
		[Display(Name = "Актив - Водители")]
		AssetDrivers,
		[Display(Name = "Актив - Производство")]
		AssetProduction,
		[Display(Name = "Актив - Склад")]
		AssetShipment
	}
}
