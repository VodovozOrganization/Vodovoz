using QS.DomainModel.UoW;
using QS.Models;
using WhereIsTheBottle.Models.MainContent;

namespace WhereIsTheBottle.ViewModels.MainContent
{
	public class AssetDriversViewModel : BottleAnalyticsReportViewModelBase<UoWFactoryModelBase>
	{
		public AssetDriversViewModel(
			UoWFactoryModelBase model)
			: base(model)
		{ }

		public override string HeaderString { get; }
	}
}
