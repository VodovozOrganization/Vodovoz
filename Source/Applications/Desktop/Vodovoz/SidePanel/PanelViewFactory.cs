using Gtk;
using QS.DomainModel.UoW;
using QS.Project.Services;
using QS.Tdi;
using System;
using System.Collections.Generic;
using Vodovoz.Core.DataService;
using Vodovoz.Dialogs.OrderWidgets;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Cash;
using Vodovoz.EntityRepositories.Complaints;
using Vodovoz.EntityRepositories.Complaints.ComplaintResults;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.FastPayments;
using Vodovoz.EntityRepositories.Subdivisions;
using Vodovoz.Parameters;
using Vodovoz.SidePanel.InfoViews;
using Vodovoz.TempAdapters;
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
					return new DeliveryPricePanelView();
				case PanelViewType.UndeliveredOrdersPanelView:
					return new UndeliveredOrdersPanelView();
				case PanelViewType.EmailsPanelView:
					return new EmailsPanelView();
				case PanelViewType.CallTaskPanelView:
					return new CallTaskPanelView(
						new BaseParametersProvider(new ParametersProvider()),
						new EmployeeRepository(),
						ServicesConfig.CommonServices);
				case PanelViewType.ComplaintPanelView:
					return new ComplaintPanelView(new ComplaintsRepository(), new ComplaintResultsRepository(), new ComplaintParametersProvider(new ParametersProvider()));
				case PanelViewType.SmsSendPanelView:
					return new SmsSendPanelView(
						ServicesConfig.CommonServices,
						new FastPaymentRepository(),
						new FastPaymentParametersProvider(new ParametersProvider()));
				case PanelViewType.FixedPricesPanelView:
					var fixedPricesDialogOpener = new FixedPricesDialogOpener();
					var fixedPricesPanelViewModel = new FixedPricesPanelViewModel(fixedPricesDialogOpener, ServicesConfig.CommonServices);
					return new FixedPricesPanelView(fixedPricesPanelViewModel);
				case PanelViewType.CashInfoPanelView:
					return new CashInfoPanelView(
						ServicesConfig.UnitOfWorkFactory,
						new CashRepository(),
						new SubdivisionRepository(new ParametersProvider()),
						new UserRepository());
				case PanelViewType.EdoLightsMatrixPanelView:
					var edoLightsMatrixViewModel = new EdoLightsMatrixViewModel();
					IGtkTabsOpener gtkTabsOpener = new GtkTabsOpener();
					ITdiTab tdiTab = TDIMain.MainNotebook.CurrentTab;
					var edoLightsMatrixPanelViewModel = new EdoLightsMatrixPanelViewModel(edoLightsMatrixViewModel, gtkTabsOpener, tdiTab);
					return new EdoLightsMatrixPanelView(edoLightsMatrixPanelViewModel);
				case PanelViewType.CarsMonitoringInfoPanelView:
					return new CarsMonitoringInfoPanelView(ServicesConfig.UnitOfWorkFactory, new DeliveryRulesParametersProvider(new ParametersProvider()), Startup.MainWin.NavigationManager);
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
