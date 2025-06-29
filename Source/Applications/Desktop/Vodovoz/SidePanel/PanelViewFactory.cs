using Autofac;
using Gtk;
using Microsoft.Extensions.Logging;
using QS.Project.Services;
using QS.Tdi;
using System;
using System.Collections.Generic;
using Vodovoz.Application.Clients;
using Vodovoz.Dialogs.OrderWidgets;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Cash;
using Vodovoz.EntityRepositories.Complaints;
using Vodovoz.EntityRepositories.Complaints.ComplaintResults;
using Vodovoz.EntityRepositories.Counterparties;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.FastPayments;
using Vodovoz.EntityRepositories.Operations;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.EntityRepositories.Subdivisions;
using Vodovoz.EntityRepositories.Undeliveries;
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
			var orderRepository = ScopeProvider.Scope.Resolve<IOrderRepository>();

			switch(type)
			{
				case PanelViewType.CounterpartyView:
					var loger = ScopeProvider.Scope.Resolve<ILogger<CounterpartyPanelView>>();
					return new CounterpartyPanelView(loger, ServicesConfig.CommonServices, orderRepository);
				case PanelViewType.DeliveryPointView:
					var deliveryPointRepository = ScopeProvider.Scope.Resolve<IDeliveryPointRepository>();
					var bottlesRepository = ScopeProvider.Scope.Resolve<IBottlesRepository>();
					var depositRepository = ScopeProvider.Scope.Resolve<IDepositRepository>();
					return new DeliveryPointPanelView(ServicesConfig.CommonServices, deliveryPointRepository, bottlesRepository, depositRepository, orderRepository);
				case PanelViewType.DeliveryPricePanelView:
					var deliveryPriceCalculator = ScopeProvider.Scope.Resolve<IDeliveryPriceCalculator>();
					return new DeliveryPricePanelView(deliveryPriceCalculator);
				case PanelViewType.UndeliveredOrdersPanelView:
					var undeliveredOrdersRepository = ScopeProvider.Scope.Resolve<IUndeliveredOrdersRepository>();
					return new UndeliveredOrdersPanelView(undeliveredOrdersRepository);
				case PanelViewType.EmailsPanelView:
					return new EmailsPanelView();
				case PanelViewType.CallTaskPanelView:
					var employeeSettings = ScopeProvider.Scope.Resolve<IEmployeeSettings>();
					var employeeRepository = ScopeProvider.Scope.Resolve<IEmployeeRepository>();
					return new CallTaskPanelView(
						employeeSettings,
						employeeRepository,
						ServicesConfig.CommonServices);
				case PanelViewType.ComplaintPanelView:
					var complaintSettigs = ScopeProvider.Scope.Resolve<IComplaintSettings>();
					var complaintsRepository = ScopeProvider.Scope.Resolve<IComplaintsRepository>();
					var complaintResultsRepository = ScopeProvider.Scope.Resolve<IComplaintResultsRepository>();
					return new ComplaintPanelView(complaintsRepository, complaintResultsRepository, complaintSettigs);
				case PanelViewType.SmsSendPanelView:
					var fastPaymentSettings = ScopeProvider.Scope.Resolve<IFastPaymentSettings>();
					var fastPaymentRepository = ScopeProvider.Scope.Resolve<IFastPaymentRepository>();
					return new SmsSendPanelView(
						ServicesConfig.CommonServices,
						fastPaymentRepository,
						fastPaymentSettings);
				case PanelViewType.FixedPricesPanelView:
					var fixedPricesDialogOpener = new FixedPricesDialogOpener();
					var fixedPricesPanelViewModel = new FixedPricesPanelViewModel(fixedPricesDialogOpener, ServicesConfig.CommonServices);
					return new FixedPricesPanelView(fixedPricesPanelViewModel);
				case PanelViewType.CashInfoPanelView:
					var cashRepository = ScopeProvider.Scope.Resolve<ICashRepository>();
					var subdivisionRepository = ScopeProvider.Scope.Resolve<ISubdivisionRepository>();
					var userRepository = ScopeProvider.Scope.Resolve<IUserRepository>();
					var cashInfoPanelViewModel = new CashInfoPanelViewModel(
						ServicesConfig.UnitOfWorkFactory,
						ServicesConfig.CommonServices,
						cashRepository,
						subdivisionRepository,
						userRepository);
					return new CashInfoPanelView(cashInfoPanelViewModel);
				case PanelViewType.EdoLightsMatrixPanelView:
					var edoLightsMatrixPanelViewModel = ScopeProvider.Scope.Resolve<EdoLightsMatrixPanelViewModel>(
						new TypedParameter(typeof(ITdiTab), TDIMain.MainNotebook.CurrentTab));
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
