using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using WhereIsTheBottle.ViewModels.MainContent;

namespace WhereIsTheBottle.Views.MainContent
{
	public partial class AssetWarehouseView : UserControl
	{
		private Border _emptyBorder1;
		private Border _emptyBorder2;
		private Border _assetMovementsborder;
		private Border _deltaBorder;
		private Border _emptyBorder3;
		private Border _emptyBorder4;

		public AssetWarehouseView()
		{
			InitializeComponent();
			CreateAdvancedHeaderGrid();
		}

		private void CreateAdvancedHeaderGrid()
		{
			_emptyBorder1 = CreateDefaultBorder("");
			_emptyBorder2 = CreateDefaultBorder("");
			_assetMovementsborder = CreateDefaultBorder("Движения внутри актива");
			_deltaBorder = CreateDefaultBorder("Дельта");
			_emptyBorder3 = CreateDefaultBorder("");
			_emptyBorder4 = CreateDefaultBorder("", true);

			TopPanel.Children.Add(_emptyBorder1);
			TopPanel.Children.Add(_emptyBorder2);
			TopPanel.Children.Add(_assetMovementsborder);
			TopPanel.Children.Add(_deltaBorder);
			TopPanel.Children.Add(_emptyBorder3);
			TopPanel.Children.Add(_emptyBorder4);
		}

		private Border CreateDefaultBorder(string text, bool showRightBorder = false)
		{
			var rightBorderThickness = 0;
			if(showRightBorder)
			{
				rightBorderThickness = 1;
			}

			return new()
			{
				BorderBrush = Brushes.Gray,
				BorderThickness = new Thickness(1, 1, rightBorderThickness, 0),
				HorizontalAlignment = HorizontalAlignment.Stretch,
				Child = new TextBlock
				{
					Text = text,
					VerticalAlignment = VerticalAlignment.Center,
					HorizontalAlignment = HorizontalAlignment.Center
				}
			};
		}

		private void MainDataGrid_OnLayoutUpdated(object sender, EventArgs e)
		{
			// if(DataContext is not AssetWarehouseViewModel { IsDataLoaded: true } vm)
			// {
			// 	return;
			// }

			_emptyBorder1.Width = MainDataGrid.Columns.Take(1).Sum(x => x.ActualWidth);
			_emptyBorder2.Width = MainDataGrid.Columns.Skip(1).Take(1).Sum(x => x.ActualWidth);
			_assetMovementsborder.Width = MainDataGrid.Columns.Skip(2).Take(4).Sum(x => x.ActualWidth);
			_deltaBorder.Width = MainDataGrid.Columns.Skip(6).Take(10).Sum(x => x.ActualWidth);
			_emptyBorder3.Width = MainDataGrid.Columns.Skip(16).Take(1).Sum(x => x.ActualWidth);
			_emptyBorder4.Width = MainDataGrid.Columns.Skip(17).Sum(x => x.ActualWidth);
		}

		//Необходимо, чтобы скроллинг работал, когда курсор находится в DataGrid
		private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
		{
			var scv = (ScrollViewer)sender;
			scv.ScrollToVerticalOffset(scv.VerticalOffset - e.Delta);
			e.Handled = true;
		}
	}
}
