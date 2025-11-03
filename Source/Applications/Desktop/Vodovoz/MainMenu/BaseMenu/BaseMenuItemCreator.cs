using System;
using Autofac;
using Gtk;
using MySqlConnector;
using QS.ChangePassword.Views;
using QS.Project.DB;
using QS.Project.Domain;
using QS.Project.Repositories;
using QS.Validation;
using QS.ViewModels;
using Vodovoz.ViewModels.Users;
using Vodovoz.ViewModels.ViewModels.Settings;
using VodovozInfrastructure.Configuration;
using VodovozInfrastructure.Passwords;

namespace Vodovoz.MainMenu.BaseMenu
{
	/// <summary>
	/// Создатель меню База
	/// </summary>
	public class BaseMenuItemCreator : MenuItemCreator
	{
		private readonly ConcreteMenuItemCreator _concreteMenuItemCreator;

		public BaseMenuItemCreator(ConcreteMenuItemCreator concreteMenuItemCreator)
		{
			_concreteMenuItemCreator = concreteMenuItemCreator ?? throw new ArgumentNullException(nameof(concreteMenuItemCreator));
		}

		/// <inheritdoc/>
		public override MenuItem Create()
		{
			var baseMenuItem = _concreteMenuItemCreator.CreateMenuItem("База");
			var baseMenu = new Menu();
			baseMenuItem.Submenu = baseMenu;
			
			baseMenu.Add(
				_concreteMenuItemCreator.CreateImageMenuItem(
					"ChangePasswordAction",
					"Изменить пароль",
					Stock.DialogAuthentication,
					null,
					OnChangePasswordPressed));
			
			baseMenu.Add(CreateSeparatorMenuItem());
			
			baseMenu.Add(_concreteMenuItemCreator.CreateImageMenuItem(
				"UserSettingsAction",
				"Настройки пользователя",
				Stock.Properties,
				null,
				OnUserPropertiesPressed));
			
			baseMenu.Add(_concreteMenuItemCreator.CreateImageMenuItem(
				"GeneralSettingsAction",
				"Общие настройки",
				Stock.Preferences,
				null,
				OnGeneralSettingsPressed));
			
			baseMenu.Add(_concreteMenuItemCreator.CreateImageMenuItem(
				"HistoryChangesJournalAction",
				"Журнал изменений",
				Stock.Find,
				null,
				OnHistoryLogPressed));
			
			baseMenu.Add(CreateSeparatorMenuItem());
			
			baseMenu.Add(_concreteMenuItemCreator.CreateImageMenuItem(
				"QuitAction",
				"Выход",
				Stock.Quit,
				null,
				OnQuitPressed));

			return baseMenuItem;
		}
		
		/// <summary>
		/// Изменить пароль
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		/// <exception cref="InvalidOperationException"></exception>
		private void OnChangePasswordPressed(object sender, EventArgs e)
		{
			if(!(Connection.ConnectionDB is MySqlConnection mySqlConnection))
			{
				throw new InvalidOperationException($"Текущее подключение не является {nameof(MySqlConnection)}");
			}
			
			var scope = Startup.AppDIContainer.BeginLifetimeScope();
			var applicationConfigurator = scope.Resolve<IApplicationConfigurator>();
			var mySqlPasswordRepository = scope.Resolve<IMySqlPasswordRepository>();
			var passwordValidator = scope.Resolve<IPasswordValidator>();
			var changePasswordModel = new MysqlChangePasswordModelExtended(applicationConfigurator, mySqlConnection, mySqlPasswordRepository);
			var changePasswordViewModel = new ChangePasswordViewModel(changePasswordModel, passwordValidator, null);
			var changePasswordView = new ChangePasswordView(changePasswordViewModel);

			changePasswordView.Destroyed += (o, args) => scope.Dispose();
			changePasswordView.ShowAll();
			changePasswordView.Run();
			changePasswordView.Destroy();
		}

		/// <summary>
		/// Настройки пользователя
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnUserPropertiesPressed(object sender, EventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<UserSettingsViewModel, IEntityUoWBuilder>(
				null, EntityUoWBuilder.ForOpen(CurrentUserSettings.Settings.Id));
		}

		/// <summary>
		/// Общие настройки
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnGeneralSettingsPressed(object sender, EventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<GeneralSettingsViewModel>(null);
		}

		/// <summary>
		/// Журнал изменений
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnHistoryLogPressed(object sender, EventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenTdiTab<Vodovoz.Dialogs.HistoryView>(null, QS.Navigation.OpenPageOptions.IgnoreHash);
		}

		/// <summary>
		/// Выход
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnQuitPressed(object sender, EventArgs e)
		{
			if(Startup.MainWin.TdiMain.CloseAllTabs())
			{
				Startup.AppDIContainer.Dispose();
				Gtk.Application.Quit();
			}
		}
	}
}
