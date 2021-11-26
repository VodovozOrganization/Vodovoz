namespace WhereIsTheBottle.ViewModels.MainContent
{
	public interface IMainContentViewModelFactory
	{
		GeneralSummaryViewModel GetGeneralSummaryViewModel(MainContentParameters parameters);
		GeneralDeltaViewModel GetGeneralDeltaViewModel(MainContentParameters parameters);
		GeneralAssetViewModel GetGeneralAssetViewModel(MainContentParameters parameters);
		DeltaLossViewModel GetDeltaLossViewModel(MainContentParameters parameters);
		DeltaShabbyViewModel GetDeltaShabbyViewModel(MainContentParameters parameters);
		DeltaDefectiveViewModel GetDeltaDefectiveViewModel(MainContentParameters parameters);
		AssetDriversViewModel GetAssetDriversViewModel(MainContentParameters parameters);
		AssetWarehouseViewModel GetAssetWarehouseViewModel(MainContentParameters parameters);
	}
}
