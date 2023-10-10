using Gtk;
using MySqlConnector;
using QS.ChangePassword.Views;
using QS.DomainModel.UoW;
using QS.Project.DB;
using QS.Project.Domain;
using QS.Project.Repositories;
using QS.Project.Services;
using QS.ViewModels;
using System;
using Vodovoz;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.EntityRepositories.Subdivisions;
using Vodovoz.Parameters;
using Vodovoz.TempAdapters;
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
		var subdivisionJournalFactory = new SubdivisionJournalFactory();
		var subdivisionRepository = new SubdivisionRepository(new ParametersProvider());
		var counterpartyJournalFactory = new CounterpartyJournalFactory(Startup.AppDIContainer.BeginLifetimeScope());

		tdiMain.OpenTab(
			() => new UserSettingsViewModel(
				EntityUoWBuilder.ForOpen(CurrentUserSettings.Settings.Id),
				UnitOfWorkFactory.GetDefaultFactory,
				NavigationManager,
				ServicesConfig.CommonServices,
				VodovozGtkServicesConfig.EmployeeService,
				new SubdivisionParametersProvider(new ParametersProvider()),
				subdivisionJournalFactory,
				counterpartyJournalFactory,
				subdivisionRepository,
				new NomenclaturePricesRepository()
			));
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
		NavigationManager.OpenTdiTab<Vodovoz.Dialogs.HistoryView>(null);
	}

	/// <summary>
	/// Выход
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnQuitActionActivated(object sender, EventArgs e)
	{
		if(tdiMain.CloseAllTabs())
		{
			_autofacScope.Dispose();
			Application.Quit();
		}
	}

	#endregion База
}
