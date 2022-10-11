using Vodovoz.Domain.Logistic;

namespace Vodovoz.ViewModels.Dialogs.Logistic
{
	public class DeliveryShiftNode
	{
		public bool Selected { get; set; }

		public DeliveryShift DeliveryShift { get; }

		public string Title => DeliveryShift.Name;

		public DeliveryShiftNode(DeliveryShift deliveryShift, bool isSelected = false)
		{
			DeliveryShift = deliveryShift;
			Selected = isSelected;
		}
	}
}
