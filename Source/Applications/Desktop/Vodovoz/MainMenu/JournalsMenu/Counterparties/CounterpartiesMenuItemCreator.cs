using System;
using Gtk;
using QS.Navigation;
using QS.Project.Services;
using QSOrmProject;
using Vodovoz.Core.Domain.Permissions;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Contacts;
using Vodovoz.Filters.ViewModels;
using Vodovoz.JournalViewModels;
using Vodovoz.ViewModels.Counterparties;
using Vodovoz.ViewModels.Counterparties.ClientClassification;
using Vodovoz.ViewModels.Journals.JournalViewModels.Client;
using Vodovoz.ViewModels.Journals.JournalViewModels.Logistic;
using Vodovoz.ViewModels.Journals.JournalViewModels.Organizations;
using Vodovoz.ViewModels.Journals.JournalViewModels.Retail;

namespace Vodovoz.MainMenu.JournalsMenu.Counterparties
{
	/// <summary>
	/// Создатель меню Справочники - Контрагенты
	/// </summary>
	public class CounterpartiesMenuItemCreator : MenuItemCreator
	{
		private readonly ConcreteMenuItemCreator _concreteMenuItemCreator;
		private MenuItem _externalCounterpartiesMatchingMenuItem;
		private MenuItem _counterpartyClassificationCalculationMenuItem;

		public CounterpartiesMenuItemCreator(ConcreteMenuItemCreator concreteMenuItemCreator)
		{
			_concreteMenuItemCreator = concreteMenuItemCreator ?? throw new ArgumentNullException(nameof(concreteMenuItemCreator));
		}

		/// <inheritdoc/>
		public override MenuItem Create()
		{
			var counterpartiesMenuItem = _concreteMenuItemCreator.CreateMenuItem("Контрагенты");
			var counterpartiesMenu = new Menu();
			counterpartiesMenuItem.Submenu = counterpartiesMenu;

			AddFirstSection(counterpartiesMenu);
			counterpartiesMenu.Add(CreateSeparatorMenuItem());
			AddSecondSection(counterpartiesMenu);
			counterpartiesMenu.Add(CreateSeparatorMenuItem());
			AddThirdSection(counterpartiesMenu);
			counterpartiesMenu.Add(CreateSeparatorMenuItem());
			AddFourthSection(counterpartiesMenu);

			Configure();

			return counterpartiesMenuItem;
		}

		#region ThirdSection

		private void AddThirdSection(Menu counterpartiesMenu)
		{
			counterpartiesMenu.Add(_concreteMenuItemCreator.CreateMenuItem(
				"Имена контрагентов Roboats", OnRoboAtsCounterpartyNamesPressed));

			counterpartiesMenu.Add(
				_concreteMenuItemCreator.CreateMenuItem(
					"Отчества контрагентов Roboats", OnRoboAtsCounterpartyPatronymicsPressed));
		}
		
		/// <summary>
		/// Имена контрагентов Roboats
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnRoboAtsCounterpartyNamesPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<RoboAtsCounterpartyNameJournalViewModel>(null);
		}

		/// <summary>
		/// Отчества контрагентов Roboats
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnRoboAtsCounterpartyPatronymicsPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<RoboAtsCounterpartyPatronymicJournalViewModel>(null);
		}
		
		#endregion

		#region SecondSection

		private void AddSecondSection(Menu counterpartiesMenu)
		{
			counterpartiesMenu.Add(_concreteMenuItemCreator.CreateMenuItem(
				"Должности сотрудников контрагента", OnCounterpartyPostsPressed));
		}
		
		/// <summary>
		/// Должности сотрудников контрагента
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		[Obsolete("Старый диалог, заменить")]
		private void OnCounterpartyPostsPressed(object sender, ButtonPressEventArgs e)
		{
			var refWin = new OrmReference(typeof(Post));
			Startup.MainWin.TdiMain.AddTab(refWin);
		}

		#endregion

		#region FirstSection

		private void AddFirstSection(Menu counterpartiesMenu)
		{
			counterpartiesMenu.Add(_concreteMenuItemCreator.CreateMenuItem(Startup.MainWin.CounterpartiesJournalAction));
			
			counterpartiesMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Точки доставки", OnDeliveryPointsPressed));
			
			counterpartiesMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Откуда клиент", OnCameFromPressed));
			
			counterpartiesMenu.Add(
				_concreteMenuItemCreator.CreateMenuItem("Типы объектов в точках доставки", OnDeliveryPointCategoriesPressed));
			
			counterpartiesMenu.Add(
				_concreteMenuItemCreator.CreateMenuItem("Виды деятельности контрагента", OnCounterpartyActivityKindsPressed));
			
			counterpartiesMenu.Add(
				_concreteMenuItemCreator.CreateMenuItem("Типы ответственных за точку доставки лиц",
					OnResponsiblePersonTypesPressed));
			
			counterpartiesMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Каналы сбыта", OnSalesChannelsPressed));
			
			counterpartiesMenu.Add(
				_concreteMenuItemCreator.CreateMenuItem(
					"Формы собственности контрагентов", OnOrganizationOwnershipTypesPressed));
		}
		
		/// <summary>
		/// Точки доставки
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnDeliveryPointsPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<DeliveryPointJournalViewModel, bool, bool>(null, true, true);
		}

		/// <summary>
		/// Откуда клиент
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnCameFromPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<ClientCameFromJournalViewModel, Action<ClientCameFromFilterViewModel>>(
				null,
				filter => filter.HidenByDefault = true,
				OpenPageOptions.IgnoreHash);
		}

		/// <summary>
		/// Типы объектов в точках доставки
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		[Obsolete("Старый диалог, заменить")]
		private void OnDeliveryPointCategoriesPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.TdiMain.OpenTab(
				OrmReference.GenerateHashName<DeliveryPointCategory>(),
				() => new OrmReference(typeof(DeliveryPointCategory))
			);
		}

		/// <summary>
		/// Виды деятельности контрагента
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		[Obsolete("Старый диалог, заменить")]
		private void OnCounterpartyActivityKindsPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.TdiMain.OpenTab(
				OrmReference.GenerateHashName<CounterpartyActivityKind>(),
				() => new OrmReference(typeof(CounterpartyActivityKind))
			);
		}

		/// <summary>
		/// Типы ответственных за точку доставки лиц
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnResponsiblePersonTypesPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<DeliveryPointResponsiblePersonTypeJournalViewModel>(null);
		}

		/// <summary>
		/// Каналы сбыта
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnSalesChannelsPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<SalesChannelJournalViewModel>(null);
		}

		/// <summary>
		/// Формы собственности контрагентов
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnOrganizationOwnershipTypesPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<OrganizationOwnershipTypeJournalViewModel>(null);
		}

		#endregion

		#region FourthSection

		private void AddFourthSection(Menu counterpartiesMenu)
		{
			_externalCounterpartiesMatchingMenuItem = _concreteMenuItemCreator
				.CreateMenuItem("Сопоставление клиентов из внешних источников", OnExternalCounterpartiesMatchingPressed);
			counterpartiesMenu.Add(_externalCounterpartiesMatchingMenuItem);

			counterpartiesMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Подтипы контрагентов", OnCounterpartySubtypesPressed));

			_counterpartyClassificationCalculationMenuItem = _concreteMenuItemCreator
				.CreateMenuItem("Пересчёт классификации", OnCounterpartyClassificationCalculationPressed);
			counterpartiesMenu.Add(_counterpartyClassificationCalculationMenuItem);
		}

		/// <summary>
		/// Сопоставление клиентов из внешних источников
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnExternalCounterpartiesMatchingPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<ExternalCounterpartiesMatchingJournalViewModel>(null);
		}

		/// <summary>
		/// Подтипы контрагентов
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnCounterpartySubtypesPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<SubtypesJournalViewModel>(null);
		}

		/// <summary>
		/// Пересчёт классификации контрагентов
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnCounterpartyClassificationCalculationPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<CounterpartyClassificationCalculationViewModel>(null);
		}
		
		#endregion

		private void Configure()
		{
			var permissionService = ServicesConfig.CommonServices.CurrentPermissionService;
			_externalCounterpartiesMatchingMenuItem.Sensitive =
				permissionService.ValidatePresetPermission("can_matching_counterparties_from_external_sources");
			
			_counterpartyClassificationCalculationMenuItem.Sensitive = 
				permissionService.ValidatePresetPermission(CounterpartyPermissions.CanCalculateCounterpartyClassifications);
		}
	}
}
