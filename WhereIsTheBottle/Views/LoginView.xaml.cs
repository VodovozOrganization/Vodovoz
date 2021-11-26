using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Autofac;
using WhereIsTheBottle.ViewModels;

namespace WhereIsTheBottle.Views
{
	public partial class LoginView
	{
		private readonly ILifetimeScope _lifetimeScope;

		public LoginView(ILifetimeScope lifetimeScope, LoginViewModel loginViewModel)
		{
			_lifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));
			LoginViewModel = loginViewModel ?? throw new ArgumentNullException(nameof(loginViewModel));
			InitializeComponent();

			DataContext = LoginViewModel;
			LoginViewModel.Close += Close;
			LoginViewModel.OpenEditConnectionsWindow += OpenEditConnectionsWindow;
		}

		private void OpenEditConnectionsWindow()
		{
			var editConnectionView = _lifetimeScope.Resolve<EditConnectionView>();
			editConnectionView.WindowStartupLocation = WindowStartupLocation.CenterScreen;
			editConnectionView.ShowDialog();

			if(editConnectionView.EditConnectionViewModel.EditConnectionModel.ConnectionsSaved)
			{
				LoginViewModel.RefreshConnections();
			}
		}

		public LoginViewModel LoginViewModel { get; }

		private void PasswordBox_OnPasswordChanged(object sender, RoutedEventArgs e)
		{
			if(sender is PasswordBox box && DataContext is LoginViewModel vm)
			{
				vm.Password = box.SecurePassword;
				vm.ErrorMessage = "";
			}
		}

		private void CancelButton_OnClick(object sender, RoutedEventArgs e)
		{
			Close();
		}

		private void Window_KeyDownPressed(object sender, KeyEventArgs e)
		{
			//OkButton Click
			if(e.Key == Key.Return && DataContext is LoginViewModel vm)
			{
				vm.ConfirmLogInCommand.Execute(null);
			}
		}
	}
}
