namespace Vodovoz.Settings.Nomenclature
{
	public interface INomenclatureSettings
	{
		int[] EquipmentKindsHavingGlassHolder { get; }
		int[] EquipmentForCheckProductGroupsIds { get; }
		int ForfeitId { get; }
		int ReturnedBottleNomenclatureId { get; }
	}
}
