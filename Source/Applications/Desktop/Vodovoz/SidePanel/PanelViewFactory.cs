using System;
using System.Collections.Generic;
using Gtk;
using QS.DomainModel.UoW;
using QS.Project.Services;
using Vodovoz.Core.DataService;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Cash;
using Vodovoz.EntityRepositories.Complaints;
using Vodovoz.EntityRepositories.Complaints.ComplaintResults;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.FastPayments;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.EntityRepositories.Subdivisions;
using Vodovoz.Parameters;
using Vodovoz.SidePanel.InfoViews;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.ViewModels.SidePanels;

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
						new SmsPaymentRepository(),
						new FastPaymentRepository(),
						new FastPaymentParametersProvider(new ParametersProvider()));
				case PanelViewType.FixedPricesPanelView:
					var fixedPricesDialogOpener = new FixedPricesDialogOpener();
					var fixedPricesPanelViewModel = new FixedPricesPanelViewModel(fixedPricesDialogOpener, ServicesConfig.CommonServices);
					return new FixedPricesPanelView(fixedPricesPanelViewModel);
				case PanelViewType.CashInfoPanelView:
					return new CashInfoPanelView(
						UnitOfWorkFactory.GetDefaultFactory,
						new CashRepository(),
						new SubdivisionRepository(new ParametersProvider()),
						new UserRepository());
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
