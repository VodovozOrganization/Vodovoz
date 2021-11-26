using System;

namespace WhereIsTheBottle.ViewModels.MainContent
{
	public class MainContentViewModelFactory : IMainContentViewModelFactory
	{
		private readonly Func<MainContentParameters, GeneralSummaryViewModel> _generalSummaryViewModelFunc;
		private readonly Func<MainContentParameters, GeneralDeltaViewModel> _generalDeltaViewModelFunc;
		private readonly Func<MainContentParameters, GeneralAssetViewModel> _generalAssetViewModelFunc;
		private readonly Func<MainContentParameters, DeltaLossViewModel> _deltaLossViewModelFunc;
		private readonly Func<MainContentParameters, DeltaShabbyViewModel> _deltaShabbyViewModelFunc;
		private readonly Func<MainContentParameters, DeltaDefectiveViewModel> _deltaDefectiveViewModelFunc;
		private readonly Func<MainContentParameters, AssetDriversViewModel> _assetDriversViewModelFunc;
		private readonly Func<MainContentParameters, AssetWarehouseViewModel> _assetWarehouseViewModelFunc;

		public MainContentViewModelFactory(
			Func<MainContentParameters, GeneralSummaryViewModel> generalSummaryViewModelFunc,
			Func<MainContentParameters, GeneralDeltaViewModel> generalDeltaViewModelFunc,
			Func<MainContentParameters, GeneralAssetViewModel> generalAssetViewModelFunc,
			Func<MainContentParameters, DeltaLossViewModel> deltaLossViewModelFunc,
			Func<MainContentParameters, DeltaShabbyViewModel> deltaShabbyViewModelFunc,
			Func<MainContentParameters, DeltaDefectiveViewModel> deltaDefectiveViewModelFunc,
			Func<MainContentParameters, AssetDriversViewModel> assetDriversViewModelFunc,
			Func<MainContentParameters, AssetWarehouseViewModel> assetWarehouseViewModelFunc
		)
		{
			_generalSummaryViewModelFunc = generalSummaryViewModelFunc;
			_generalDeltaViewModelFunc = generalDeltaViewModelFunc;
			_generalAssetViewModelFunc = generalAssetViewModelFunc;
			_deltaLossViewModelFunc = deltaLossViewModelFunc;
			_deltaShabbyViewModelFunc = deltaShabbyViewModelFunc;
			_deltaDefectiveViewModelFunc = deltaDefectiveViewModelFunc;
			_assetDriversViewModelFunc = assetDriversViewModelFunc;
			_assetWarehouseViewModelFunc = assetWarehouseViewModelFunc;
		}

		public GeneralSummaryViewModel GetGeneralSummaryViewModel(MainContentParameters parameters)
		{
			return _generalSummaryViewModelFunc(parameters);
		}

		public GeneralDeltaViewModel GetGeneralDeltaViewModel(MainContentParameters parameters)
		{
			return _generalDeltaViewModelFunc(parameters);
		}

		public GeneralAssetViewModel GetGeneralAssetViewModel(MainContentParameters parameters)
		{
			return _generalAssetViewModelFunc(parameters);
		}

		public DeltaLossViewModel GetDeltaLossViewModel(MainContentParameters parameters)
		{
			return _deltaLossViewModelFunc(parameters);
		}

		public DeltaShabbyViewModel GetDeltaShabbyViewModel(MainContentParameters parameters)
		{
			return _deltaShabbyViewModelFunc(parameters);
		}

		public DeltaDefectiveViewModel GetDeltaDefectiveViewModel(MainContentParameters parameters)
		{
			return _deltaDefectiveViewModelFunc(parameters);
		}

		public AssetDriversViewModel GetAssetDriversViewModel(MainContentParameters parameters)
		{
			return _assetDriversViewModelFunc(parameters);
		}

		public AssetWarehouseViewModel GetAssetWarehouseViewModel(MainContentParameters parameters)
		{
			return _assetWarehouseViewModelFunc(parameters);
		}
	}
}
