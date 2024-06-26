﻿using Autofac;
using Gtk;
using QS.Project.Services;
using QS.Tdi;
using System;
using System.Collections.Generic;
using Vodovoz.Dialogs.OrderWidgets;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Cash;
using Vodovoz.EntityRepositories.Complaints;
using Vodovoz.EntityRepositories.Complaints.ComplaintResults;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.FastPayments;
using Vodovoz.EntityRepositories.Subdivisions;
using Vodovoz.Settings.Common;
using Vodovoz.Settings.Complaints;
using Vodovoz.Settings.Delivery;
using Vodovoz.Settings.Employee;
using Vodovoz.Settings.FastPayments;
using Vodovoz.SidePanel.InfoViews;
using Vodovoz.TempAdapters;
using Vodovoz.Tools.Logistic;
using Vodovoz.ViewModels.ViewModels.SidePanels;
using Vodovoz.ViewModels.Widgets.EdoLightsMatrix;

namespace Vodovoz.SidePanel
{
	public static class PanelViewFactory
	{
		public static Widget Create(PanelViewType type)
		{
			switch(type)
			{
				case PanelViewType.CounterpartyView:
					return new CounterpartyPanelView(ServicesConfig.CommonServices);
				case PanelViewType.DeliveryPointView:
					return new DeliveryPointPanelView(ServicesConfig.CommonServices);
				case PanelViewType.DeliveryPricePanelView:
					var deliveryPriceCalculator = ScopeProvider.Scope.Resolve<IDeliveryPriceCalculator>();
					return new DeliveryPricePanelView(deliveryPriceCalculator);
				case PanelViewType.UndeliveredOrdersPanelView:
					return new UndeliveredOrdersPanelView();
				case PanelViewType.EmailsPanelView:
					return new EmailsPanelView();
				case PanelViewType.CallTaskPanelView:
					var employeeSettings = ScopeProvider.Scope.Resolve<IEmployeeSettings>();
					return new CallTaskPanelView(
						employeeSettings,
						new EmployeeRepository(),
						ServicesConfig.CommonServices);
				case PanelViewType.ComplaintPanelView:
					var complaintSettigs = ScopeProvider.Scope.Resolve<IComplaintSettings>();
					return new ComplaintPanelView(new ComplaintsRepository(), new ComplaintResultsRepository(), complaintSettigs);
				case PanelViewType.SmsSendPanelView:
					var fastPaymentSettings = ScopeProvider.Scope.Resolve<IFastPaymentSettings>();
					return new SmsSendPanelView(
						ServicesConfig.CommonServices,
						new FastPaymentRepository(),
						fastPaymentSettings);
				case PanelViewType.FixedPricesPanelView:
					var fixedPricesDialogOpener = new FixedPricesDialogOpener();
					var fixedPricesPanelViewModel = new FixedPricesPanelViewModel(fixedPricesDialogOpener, ServicesConfig.CommonServices);
					return new FixedPricesPanelView(fixedPricesPanelViewModel);
				case PanelViewType.CashInfoPanelView:
					var subdivisionRepository = ScopeProvider.Scope.Resolve<ISubdivisionRepository>();
					return new CashInfoPanelView(
						ServicesConfig.UnitOfWorkFactory,
						new CashRepository(),
						subdivisionRepository,
						new UserRepository());
				case PanelViewType.EdoLightsMatrixPanelView:
					var edoLightsMatrixViewModel = new EdoLightsMatrixViewModel();
					IGtkTabsOpener gtkTabsOpener = new GtkTabsOpener();
					ITdiTab tdiTab = TDIMain.MainNotebook.CurrentTab;
					var edoLightsMatrixPanelViewModel = new EdoLightsMatrixPanelViewModel(edoLightsMatrixViewModel, gtkTabsOpener, tdiTab);
					return new EdoLightsMatrixPanelView(edoLightsMatrixPanelViewModel);
				case PanelViewType.CarsMonitoringInfoPanelView:
					var _deliveryRulesSettings = ScopeProvider.Scope.Resolve<IDeliveryRulesSettings>();
					var generalSettings = ScopeProvider.Scope.Resolve<IGeneralSettings>();
					return new CarsMonitoringInfoPanelView(ServicesConfig.UnitOfWorkFactory, _deliveryRulesSettings, Startup.MainWin.NavigationManager, generalSettings);
				default:
					throw new NotSupportedException();
			}
		}

		public static IEnumerable<Widget> CreateAll(IEnumerable<PanelViewType> types)
		{
			using(var iterator = types.GetEnumerator())
			{
				while(iterator.MoveNext())
				{
					yield return Create(iterator.Current);
				}
			}
		}
	}
}
