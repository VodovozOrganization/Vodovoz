namespace Vodovoz.Services
{
	public interface INomenclatureOnlineParametersProvider
	{
		int WaterNomenclatureOnlineGroupId { get; }
		int EquipmentNomenclatureOnlineGroupId { get; }
		int OtherGoodsNomenclatureOnlineGroupId { get; }
		int KulerNomenclatureOnlineCategoryId { get; }
		int PurifierNomenclatureOnlineCategoryId { get; }
		int WaterPumpNomenclatureOnlineCategoryId { get; }
		int CupHolderNomenclatureOnlineCategoryId { get; }
	}
}
