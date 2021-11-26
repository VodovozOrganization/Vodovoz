using System.ComponentModel;
using System.Windows.Controls;
using WhereIsTheBottle.Controls;
using WhereIsTheBottle.Models.MainContent.Nodes;
using WhereIsTheBottle.ViewModels.MainContent;

namespace WhereIsTheBottle.Views.MainContent
{
	public partial class GeneralDeltaView : UserControl
	{
		public GeneralDeltaView()
		{
			InitializeComponent();
			Loaded += (sender, _) =>
			{
				if(DataContext is not GeneralDeltaViewModel vm)
				{
					return;
				}
				vm.PropertyChanged -= VmOnPropertyChanged;
				vm.PropertyChanged += VmOnPropertyChanged;
				MainDataGrid_OnSorted(sender, new DataGridSortedEventArgs(MainDataGrid.Columns[0]));
			};
		}

		private void VmOnPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if(DataContext is not GeneralDeltaViewModel vm)
			{
				return;
			}
			if(e.PropertyName == nameof(GeneralDeltaViewModel.IsDataLoaded) && vm.IsDataLoaded)
			{
				MainDataGrid_OnSorted(sender, new DataGridSortedEventArgs(MainDataGrid.Columns[0]));
			}
		}

		private void MainDataGrid_OnSorted(object sender, DataGridSortedEventArgs args)
		{
			MainDataGrid.Items.SortDescriptions.Insert(0,
				new SortDescription(nameof(GeneralDeltaNode.SortType), ListSortDirection.Ascending));
		}
	}
}
