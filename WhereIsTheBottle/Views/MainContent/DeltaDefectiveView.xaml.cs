using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WhereIsTheBottle.ViewModels.MainContent;

namespace WhereIsTheBottle.Views.MainContent
{
	public partial class DeltaDefectiveView
	{
		public DeltaDefectiveView()
		{
			InitializeComponent();
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
