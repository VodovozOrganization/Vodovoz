using System.ComponentModel;
using System.Windows;

namespace WhereIsTheBottle.Services
{
	public partial class MessageWindowView : Window
	{
		public MessageWindowView(MessageWindowViewModel messageWindowViewModel)
		{
			InitializeComponent();
			DataContext = messageWindowViewModel;
		}

		private void ButtonOk_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}
	}
}
