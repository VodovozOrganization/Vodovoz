using System;
using System.Collections.Generic;
using Gtk;
using Vodovoz.SidePanel.InfoViews;

namespace Vodovoz.SidePanel
{
	public static class PanelViewFactory
	{
		public static Widget Create(PanelViewType type)
		{
			switch (type)
			{
				case PanelViewType.CounterpartyView:
					return new CounterpartyPanelView();
				case PanelViewType.DeliveryPointView:
					return new DeliveryPointPanelView();
				case PanelViewType.AdditionalAgreementPanelView:
					return new AdditionalAgreementPanelView();
				case PanelViewType.DeliveryPricePanelView:
					return new DeliveryPricePanelView();
				case PanelViewType.UndeliveredOrdersPanelView:
					return new UndeliveredOrdersPanelView();
				default:
					throw new NotSupportedException();
			}
		}

		public static IEnumerable<Widget> CreateAll(IEnumerable<PanelViewType> types)
		{
			var iterator = types.GetEnumerator();
			while (iterator.MoveNext())
				yield return Create(iterator.Current);
		}
	}

	public enum PanelViewType{
		CounterpartyView,
		DeliveryPointView,
		AdditionalAgreementPanelView,
		DeliveryPricePanelView,
		UndeliveredOrdersPanelView
	}
}

