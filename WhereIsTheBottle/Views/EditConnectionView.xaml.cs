using System.Windows;
using System.Windows.Input;
using WhereIsTheBottle.ViewModels;

namespace WhereIsTheBottle.Views
{
	public partial class EditConnectionView : Window
	{
		public EditConnectionView(EditConnectionViewModel editConnectionViewModel)
		{
			InitializeComponent();
			DataContext = editConnectionViewModel;
			editConnectionViewModel.Close += Close;
			EditConnectionViewModel = editConnectionViewModel;
		}

		public EditConnectionViewModel EditConnectionViewModel { get; }

		private void CancelButton_OnClick(object sender, RoutedEventArgs e)
		{
			Close();
		}

		private void Window_KeyDownPressed(object sender, KeyEventArgs e)
		{
			//OkButton Click
			if(e.Key == Key.Return && DataContext is EditConnectionViewModel vm)
			{
				vm.SaveCommand.Execute(null);
			}
		}
	}
}
