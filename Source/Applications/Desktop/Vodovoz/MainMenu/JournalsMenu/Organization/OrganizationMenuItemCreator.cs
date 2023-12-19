﻿using System;
using Gtk;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Project.Services;
using QSOrmProject;
using Vodovoz.Domain.Complaints;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Orders;
using Vodovoz.Journals.JournalViewModels;
using Vodovoz.Journals.JournalViewModels.Organizations;
using Vodovoz.JournalViewers;
using Vodovoz.ViewModels;
using Vodovoz.ViewModels.Complaints;
using Vodovoz.ViewModels.Dialogs.Roboats;
using Vodovoz.ViewModels.Journals.FilterViewModels.Employees;
using Vodovoz.ViewModels.Journals.JournalViewModels.Client;
using Vodovoz.ViewModels.Journals.JournalViewModels.Complaints;
using Vodovoz.ViewModels.Journals.JournalViewModels.Employees;

namespace Vodovoz.MainMenu.JournalsMenu.Organization
{
	public class OrganizationMenuItemCreator : MenuItemCreator
	{
		private readonly ConcreteMenuItemCreator _concreteMenuItemCreator;
		private readonly WageMenuItemCreator _wageMenuItemCreator;
		private readonly ComplaintResultsMenuItemCreator _complaintResultsMenuItemCreator;
		private readonly ComplaintClassificationMenuItemCreator _complaintClassificationMenuItemCreator;
		private readonly UndeliveryClassificationMenuItemCreator _undeliveryClassificationMenuItemCreator;

		public OrganizationMenuItemCreator(
			ConcreteMenuItemCreator concreteMenuItemCreator,
			WageMenuItemCreator wageMenuItemCreator,
			ComplaintResultsMenuItemCreator complaintResultsMenuItemCreator,
			ComplaintClassificationMenuItemCreator complaintClassificationMenuItemCreator,
			UndeliveryClassificationMenuItemCreator undeliveryClassificationMenuItemCreator)
		{
			_concreteMenuItemCreator = concreteMenuItemCreator ?? throw new ArgumentNullException(nameof(concreteMenuItemCreator));
			_wageMenuItemCreator = wageMenuItemCreator ?? throw new ArgumentNullException(nameof(wageMenuItemCreator));
			_complaintResultsMenuItemCreator =
				complaintResultsMenuItemCreator ?? throw new ArgumentNullException(nameof(complaintResultsMenuItemCreator));
			_complaintClassificationMenuItemCreator =
				complaintClassificationMenuItemCreator ?? throw new ArgumentNullException(nameof(complaintClassificationMenuItemCreator));
			_undeliveryClassificationMenuItemCreator =
				undeliveryClassificationMenuItemCreator ?? throw new ArgumentNullException(nameof(undeliveryClassificationMenuItemCreator));
		}

		public MenuItem Create()
		{
			var organizationMenuItem = new MenuItem("Наша организация");
			var organizationMenu = new Menu();
			organizationMenuItem.Submenu = organizationMenu;

			organizationMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Организации", OnOrganizationsPressed));
			organizationMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Подразделения", OnSubdivisionsPressed));
			organizationMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Склады", OnWarehousesPressed));
			organizationMenu.Add(CreateSeparatorMenuItem());
			organizationMenu.Add(_wageMenuItemCreator.Create());
			organizationMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Сотрудники", OnEmployeePressed));
			organizationMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Национальность", OnNationalityPressed));
			organizationMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Иностранное гражданство", OnCitizenshipPressed));
			organizationMenu.Add(CreateSeparatorMenuItem());
			organizationMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Источники рекламаций", OnComplaintSourcesPressed));
			organizationMenu.Add(_complaintResultsMenuItemCreator.Create());
			organizationMenu.Add(_complaintClassificationMenuItemCreator.Create());
			organizationMenu.Add(_undeliveryClassificationMenuItemCreator.Create());
			organizationMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Источники проблем", OnUndeliveryProblemSourcesPressed));
			organizationMenu.Add(_concreteMenuItemCreator.CreateMenuItem(
				"Причины рекламаций водителей", OnDriversComplaintReasonsPressed));
			organizationMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Ответственные за рекламации", OnResponsiblesPressed));
			organizationMenu.Add(CreateSeparatorMenuItem());
			organizationMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Типы телефонов", OnPhoneTypesPressed));
			organizationMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Типы e-mail адресов", OnEmailTypesPressed));
			organizationMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Причины отписки от рассылки", OnUnsubscribingReasonsPressed));
			organizationMenu.Add(CreateSeparatorMenuItem());
			organizationMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Справочники Roboats", OnRoboatsExportPressed));

			return organizationMenuItem;
		}
		
		/// <summary>
		/// Организации
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		[Obsolete("Старый диалог")]
		private void OnOrganizationsPressed(object sender, ButtonPressEventArgs e)
		{
			var refWin = new OrmReference(typeof(Domain.Organizations.Organization));
			Startup.MainWin.TdiMain.AddTab(refWin);
		}
		
		/// <summary>
		/// Подразделения
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnSubdivisionsPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<SubdivisionsJournalViewModel>(null);
		}

		/// <summary>
		/// Склады
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnWarehousesPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenTdiTab<WarehousesView>(null);
		}
		
		/// <summary>
		/// Сотрудники
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnEmployeePressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager
				.OpenViewModel<EmployeesJournalViewModel, Action<EmployeeFilterViewModel>>(
					null,
					filter =>
					{
						filter.Status = EmployeeStatus.IsWorking;
					},
					OpenPageOptions.IgnoreHash);
		}
		
		/// <summary>
		/// Национальность
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		[Obsolete("Старый диалог")]
		private void OnNationalityPressed(object sender, ButtonPressEventArgs e)
		{
			var refWin = new OrmReference(typeof(Nationality));
			Startup.MainWin.TdiMain.AddTab(refWin);
		}
		
		/// <summary>
		/// Иностранное гражданство
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		[Obsolete("Старый диалог")]
		private void OnCitizenshipPressed(object sender, ButtonPressEventArgs e)
		{
			var refWin = new OrmReference(typeof(Citizenship));
			Startup.MainWin.TdiMain.AddTab(refWin);
		}
		
		/// <summary>
		/// Источники рекламаций
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnComplaintSourcesPressed(object sender, ButtonPressEventArgs e)
		{
			var complaintSourcesViewModel = new SimpleEntityJournalViewModel<ComplaintSource, ComplaintSourceViewModel>(
				x => x.Name,
				() => new ComplaintSourceViewModel(EntityUoWBuilder.ForCreate(), UnitOfWorkFactory.GetDefaultFactory, ServicesConfig.CommonServices),
				(node) => new ComplaintSourceViewModel(EntityUoWBuilder.ForOpen(node.Id), UnitOfWorkFactory.GetDefaultFactory, ServicesConfig.CommonServices),
				UnitOfWorkFactory.GetDefaultFactory,
				ServicesConfig.CommonServices
			);
			Startup.MainWin.TdiMain.AddTab(complaintSourcesViewModel);
		}
		
		/// <summary>
		/// Источники проблем
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnUndeliveryProblemSourcesPressed(object sender, ButtonPressEventArgs e)
		{
			var undeliveryProblemSourcesViewModel = new SimpleEntityJournalViewModel<UndeliveryProblemSource, UndeliveryProblemSourceViewModel>(
				x => x.Name,
				() => new UndeliveryProblemSourceViewModel(
					EntityUoWBuilder.ForCreate(),
					UnitOfWorkFactory.GetDefaultFactory,
					ServicesConfig.CommonServices
				),
				(node) => new UndeliveryProblemSourceViewModel(
					EntityUoWBuilder.ForOpen(node.Id),
					UnitOfWorkFactory.GetDefaultFactory,
					ServicesConfig.CommonServices
				),
				UnitOfWorkFactory.GetDefaultFactory,
				ServicesConfig.CommonServices
			);
			undeliveryProblemSourcesViewModel.SetActionsVisible(deleteActionEnabled: false);
			
			Startup.MainWin.TdiMain.AddTab(undeliveryProblemSourcesViewModel);
		}
		
		/// <summary>
		/// Причины рекламаций водителей
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnDriversComplaintReasonsPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<DriverComplaintReasonsJournalViewModel>(null, OpenPageOptions.IgnoreHash);
		}

		/// <summary>
		/// Ответственные за рекламации
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnResponsiblesPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<ResponsibleJournalViewModel>(null, OpenPageOptions.IgnoreHash);
		}

		/// <summary>
		/// Типы телефонов
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnPhoneTypesPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<PhoneTypeJournalViewModel>(null, OpenPageOptions.IgnoreHash);
		}

		/// <summary>
		/// Типы e-mail адресов
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnEmailTypesPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<EmailTypeJournalViewModel>(null, OpenPageOptions.IgnoreHash);
		}

		/// <summary>
		/// Причины отписки от рассылки
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnUnsubscribingReasonsPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<BulkEmailEventReasonJournalViewModel>(null);
		}

		/// <summary>
		/// Справочники Roboats
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnRoboatsExportPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<RoboatsCatalogExportViewModel>(null);
		}
	}
}
