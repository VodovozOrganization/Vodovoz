using System.Windows.Controls;
using WhereIsTheBottle.ViewModels.MainContent;

namespace WhereIsTheBottle.Views.MainContent
{
	public partial class AssetDriversView : UserControl
	{
		public AssetDriversView(AssetDriversViewModel viewModel)
		{
			InitializeComponent();
			DataContext = viewModel;
		}

		public AssetDriversView()
		{
			InitializeComponent();
		}
	}
}