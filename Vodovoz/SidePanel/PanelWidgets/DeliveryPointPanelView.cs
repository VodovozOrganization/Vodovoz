using System;
using QSProjectsLib;
using Vodovoz.Domain.Client;
using Vodovoz.Repository;
using Vodovoz.Repository.Operations;

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
			var bottlesAtDeliveryPoint = BottlesRepository.GetBottlesAtDeliveryPoint(InfoProvider.UoW, DeliveryPoint);
			var bottlesAvgDeliveryPoint = DeliveryPointRepository.GetAvgBottlesOrdered(InfoProvider.UoW, DeliveryPoint, 5);
			labelBottles.Text = String.Format("{0} шт. (сред. зак.: {1:G3})", bottlesAtDeliveryPoint, bottlesAvgDeliveryPoint);
			var depositsAtDeliveryPoint = DepositRepository.GetDepositsAtDeliveryPoint(InfoProvider.UoW, DeliveryPoint, null);
			labelDeposits.Text = CurrencyWorks.GetShortCurrencyString(depositsAtDeliveryPoint);
			textviewComment.Buffer.Text = DeliveryPoint.Comment;
		}

		public bool VisibleOnPanel
		{
			get
			{
				return DeliveryPoint != null;
			}
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

