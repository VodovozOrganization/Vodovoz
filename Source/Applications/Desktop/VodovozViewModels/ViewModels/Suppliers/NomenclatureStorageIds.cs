namespace Vodovoz.ViewModels.ViewModels.Suppliers
{
	public struct NomenclatureStorageIds
	{
		private readonly int _nomenclatureId;
		private readonly int _storageId;
		public NomenclatureStorageIds(int nomenclatureId, int storageId)
		{
			_nomenclatureId = nomenclatureId;
			_storageId = storageId;
		}
	}
}
