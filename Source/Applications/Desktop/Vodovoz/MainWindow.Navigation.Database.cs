using Gtk;
using MySqlConnector;
using QS.ChangePassword.Views;
using QS.Project.DB;
using QS.Project.Domain;
using QS.Project.Repositories;
using QS.ViewModels;
using System;
using Vodovoz;
using Vodovoz.ViewModels.Users;
using Vodovoz.ViewModels.ViewModels.Settings;
using VodovozInfrastructure.Passwords;

public partial class MainWindow
{
	#region База

	/// <summary>
	/// Изменить пароль
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	/// <exception cref="InvalidOperationException"></exception>
	protected void OnDialogAuthenticationActionActivated(object sender, EventArgs e)
	{
		if(!(Connection.ConnectionDB is MySqlConnection mySqlConnection))
		{
			throw new InvalidOperationException($"Текущее подключение не является {nameof(MySqlConnection)}");
		}
		var mySqlPasswordRepository = new MySqlPasswordRepository();
		var changePasswordModel = new MysqlChangePasswordModelExtended(_applicationConfigurator, mySqlConnection, mySqlPasswordRepository);
		var changePasswordViewModel = new ChangePasswordViewModel(changePasswordModel, _passwordValidator, null);
		var changePasswordView = new ChangePasswordView(changePasswordViewModel);

		changePasswordView.ShowAll();
		changePasswordView.Run();
		changePasswordView.Destroy();
	}

	/// <summary>
	/// Настройки пользователя
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnPropertiesActionActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<UserSettingsViewModel, IEntityUoWBuilder>(null, EntityUoWBuilder.ForOpen(CurrentUserSettings.Settings.Id));
	}

	/// <summary>
	/// Общие настройки
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnGeneralSettingsActionActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<GeneralSettingsViewModel>(null);
	}

	/// <summary>
	/// Журнал изменений
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionHistoryLogActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenTdiTab<Vodovoz.Dialogs.HistoryView>(null, QS.Navigation.OpenPageOptions.IgnoreHash);
	}

	#endregion База
}
