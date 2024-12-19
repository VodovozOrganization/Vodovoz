using QS.Project.Domain;
using QSOrmProject;
using QSProjectsLib;
using System;
using Vodovoz.Core.Domain.StoredResources;
using Vodovoz.Domain.Client;
using Vodovoz.JournalViewModels;
using Vodovoz.ServiceDialogs;
using Vodovoz.ServiceDialogs.Database;
using Vodovoz.ViewModels.AdministrationTools;
using Vodovoz.ViewModels.BaseParameters;
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
		NavigationManager.OpenViewModel<RegisteredRMJournalViewModel>(null, QS.Navigation.OpenPageOptions.IgnoreHash);
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
		NavigationManager.OpenTdiTab<MergeAddressesDlg>(null);
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
		NavigationManager.OpenTdiTab<OrdersWithoutBottlesOperationDlg>(null);
	}

	/// <summary>
	/// Переотправка почты
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnAction62Activated(object sender, EventArgs e)
	{
		NavigationManager.OpenTdiTab<ResendEmailsDialog>(null, QS.Navigation.OpenPageOptions.IgnoreHash);
	}

	/// <summary>
	/// Пересчет ЗП водителей
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionRecalculateDriverWagesActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenTdiTab<RecalculateDriverWageDlg>(null, QS.Navigation.OpenPageOptions.IgnoreHash);
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
