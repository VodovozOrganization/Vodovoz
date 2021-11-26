using System.Windows.Controls;
using System.Windows.Input;

namespace WhereIsTheBottle.Views.MainContent
{
	public partial class DeltaLossView
	{
		public DeltaLossView()
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
