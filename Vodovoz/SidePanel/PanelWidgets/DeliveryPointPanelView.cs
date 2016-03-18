using System;
using Vodovoz.Domain;
using Vodovoz.Repository;
using QSProjectsLib;

namespace Vodovoz.Panel
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class DeliveryPointPanelView : Gtk.Bin, IPanelView
	{
		DeliveryPoint DeliveryPoint{get;set;}

		public DeliveryPointPanelView()
		{
			this.Build();
			Configure();
		}

		void Configure()
		{
			labelAddress.LineWrapMode = Pango.WrapMode.WordChar;
		}

		#region IPanelView implementation
		public IInfoProvider InfoProvider{ get; set;}

		public void Refresh()
		{
			DeliveryPoint = (InfoProvider as IDeliveryPointInfoProvider)?.DeliveryPoint;
			if (DeliveryPoint == null)
				return;			
			labelAddress.Text = DeliveryPoint.CompiledAddress;
			var bottlesAtDeliveryPoint = DeliveryPointRepository.GetBottlesAtDeliveryPoint(InfoProvider.UoW, DeliveryPoint);
			labelBottles.Text = bottlesAtDeliveryPoint.ToString()+" шт.";
			var depositsAtDeliveryPoint = MoneyRepository.GetBottleDepositsAtDeliveryPoint(InfoProvider.UoW, DeliveryPoint);
			labelDeposits.Text = CurrencyWorks.GetShortCurrencyString(depositsAtDeliveryPoint);
			textviewComment.Buffer.Text = DeliveryPoint.Comment;
		}

		public void OnCurrentObjectChanged(object changedObject)
		{
			var deliveryPoint = changedObject as DeliveryPoint;
			if (deliveryPoint != null)
			{
				DeliveryPoint = deliveryPoint;
				Refresh();
			}
		}			
		#endregion
	}
}

