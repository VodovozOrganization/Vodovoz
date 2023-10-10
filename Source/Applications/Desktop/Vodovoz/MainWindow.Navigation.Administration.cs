using Fias.Client;
using Fias.Client.Cache;
using QS.Dialog.Gtk;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Services;
using QSOrmProject;
using QSProjectsLib;
using System;
using Vodovoz.Dialogs.OnlineStore;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.StoredResources;
using Vodovoz.EntityRepositories.Permissions;
using Vodovoz.JournalViewModels;
using Vodovoz.Parameters;
using Vodovoz.ServiceDialogs;
using Vodovoz.ServiceDialogs.Database;
using Vodovoz.Services;
using Vodovoz.ViewModels.AdministrationTools;
using Vodovoz.ViewModels.BaseParameters;
using Vodovoz.ViewModels.Journals.FilterViewModels.Security;
using Vodovoz.ViewModels.Journals.JournalViewModels.Sale;
using Vodovoz.ViewModels.Journals.JournalViewModels.Security;
using Vodovoz.ViewModels.Journals.JournalViewModels.Users;

public partial class MainWindow
{
	/// <summary>
	/// Типы документов
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	[Obsolete("Старый диалог, заменить")]
	protected void OnActionTypesOfEntitiesActivated(object sender, EventArgs e)
	{
		if(QSMain.User.Admin)
		{
			tdiMain.OpenTab(
				OrmReference.GenerateHashName<TypeOfEntity>(),
				() => new OrmReference(typeof(TypeOfEntity)));
		}
	}

	/// <summary>
	/// Пользователи
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionUsersActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<UsersJournalViewModel>(null);
	}

	/// <summary>
	/// Роли пользователей
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnUsersRolesActionActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<UserRolesJournalViewModel>(null);
	}

	/// <summary>
	/// Зарегистрированные RM
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnRegisteredRMActionActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<RegisteredRMJournalViewModel>(null);
	}

	/// <summary>
	/// Параметры
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionParametersActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<BaseParametersViewModel>(null);
	}

	#region Обслуживание

	/// <summary>
	/// Замена ссылок
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnAction45Activated(object sender, EventArgs e)
	{
		NavigationManager.OpenTdiTab<ReplaceEntityLinksDlg>(null);
	}

	/// <summary>
	/// Дубликаты адресов
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionAddressDuplicetesActivated(object sender, EventArgs e)
	{
		IParametersProvider parametersProvider = new ParametersProvider();
		IFiasApiParametersProvider fiasApiParametersProvider = new FiasApiParametersProvider(parametersProvider);
		var geoCoderCache = new GeocoderCache(UnitOfWorkFactory.GetDefaultFactory);
		IFiasApiClient fiasApiClient = new FiasApiClient(fiasApiParametersProvider.FiasApiBaseUrl, fiasApiParametersProvider.FiasApiToken, geoCoderCache);

		tdiMain.OpenTab(
			TdiTabBase.GenerateHashName<MergeAddressesDlg>(),
			() => new MergeAddressesDlg(fiasApiClient));
	}

	/// <summary>
	/// Расчет расстояний до точек
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionDistanceFromCenterActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenTdiTab<CalculateDistanceToPointsDlg>(null);
	}

	/// <summary>
	/// Заказы без операций движения бутылей
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionOrdersWithoutBottlesOperationActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			TdiTabBase.GenerateHashName<OrdersWithoutBottlesOperationDlg>(),
			() => new OrdersWithoutBottlesOperationDlg());
	}

	/// <summary>
	/// Загрузка 1с
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionLoad1cCounterpartyAndDeliveryPointsActivated(object sender, EventArgs e)
	{
		var widget = new LoadFrom1cClientsAndDeliveryPoints();
		tdiMain.AddTab(widget);
	}

	/// <summary>
	/// Выгрузка в интернет-магазин
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionToOnlineStoreActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			TdiTabBase.GenerateHashName<ExportToSiteDlg>(),
			() => new ExportToSiteDlg());
	}

	/// <summary>
	/// Переотправка почты
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnAction62Activated(object sender, EventArgs e)
	{
		var widget = new ResendEmailsDialog();
		tdiMain.AddTab(widget);
	}

	/// <summary>
	/// Пересчет ЗП водителей
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionRecalculateDriverWagesActivated(object sender, EventArgs e)
	{
		var dlg = new RecalculateDriverWageDlg();
		tdiMain.AddTab(dlg);
	}

	/// <summary>
	/// Обновление сведений Контрагентов из ФНС
	/// </summary>
	/// <param name="sender">Sender.</param>
	/// <param name="e">E.</param>
	protected void OnAction76Activated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<RevenueServiceMassCounterpartyUpdateToolViewModel>(null);
	}

	#endregion Обслуживание

	/// <summary>
	/// Шаблоны документов
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	[Obsolete("Старый диалог, заменить")]
	protected void OnActionDocTemplatesActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			OrmReference.GenerateHashName<DocTemplate>(),
			() => new OrmReference(typeof(DocTemplate)));
	}

	/// <summary>
	/// Географические группы
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionGeographicGroupsActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<GeoGroupJournalViewModel>(null);
	}

	/// <summary>
	/// Изображения
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	[Obsolete("Старый диалог, заменить")]
	protected void OnImageListOpenActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			OrmReference.GenerateHashName<StoredResource>(),
			() => new OrmReference(typeof(StoredResource))
		);
	}
}
