using System.Windows;
using System.Windows.Controls;

namespace WhereIsTheBottle.Views.MainContent
{
	public partial class GeneralSummaryView
	{
		public GeneralSummaryView()
		{
			InitializeComponent();
		}

		private async void Texbox_OnGotFocus(object sender, RoutedEventArgs e)
		{
			await Application.Current.Dispatcher.InvokeAsync(((TextBox)sender).SelectAll);
		}
	}
}
