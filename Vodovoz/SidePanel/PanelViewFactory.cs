using System;
using System.Collections.Generic;
using Gtk;
using QS.DomainModel.NotifyChange;
using Vodovoz.Core.DataService;
using Vodovoz.EntityRepositories.Complaints;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.SidePanel.InfoViews;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.ViewModels.SidePanels;

namespace Vodovoz.SidePanel
{
	public static class PanelViewFactory
	{
		public static Widget Create(PanelViewType type)
		{
			switch(type) {
				case PanelViewType.CounterpartyView:
					return new CounterpartyPanelView();
				case PanelViewType.DeliveryPointView:
					return new DeliveryPointPanelView();
				case PanelViewType.DeliveryPricePanelView:
					return new DeliveryPricePanelView();
				case PanelViewType.UndeliveredOrdersPanelView:
					return new UndeliveredOrdersPanelView();
				case PanelViewType.EmailsPanelView:
					return new EmailsPanelView();
				case PanelViewType.CallTaskPanelView:
					return new CallTaskPanelView(new BaseParametersProvider(), EmployeeSingletonRepository.GetInstance());
				case PanelViewType.ComplaintPanelView:
					return new ComplaintPanelView(new ComplaintsRepository());
				case PanelViewType.SmsSendPanelView:
					return new SmsSendPanelView();
				case PanelViewType.FixedPricesPanelView:
					var fixedPricesDialogOpener = new FixedPricesDialogOpener();
					FixedPricesPanelViewModel fixedPricesPanelViewModel = new FixedPricesPanelViewModel(fixedPricesDialogOpener);
					return new FixedPricesPanelView(fixedPricesPanelViewModel);
				default:
					throw new NotSupportedException();
			}
		}

		public static IEnumerable<Widget> CreateAll(IEnumerable<PanelViewType> types)
		{
			var iterator = types.GetEnumerator();
			while(iterator.MoveNext())
				yield return Create(iterator.Current);
		}
	}
}

