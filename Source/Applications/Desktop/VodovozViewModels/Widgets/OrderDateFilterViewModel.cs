using QS.ViewModels;

namespace Vodovoz.ViewModels.Widgets
{
	public class OrderDateFilterViewModel : WidgetViewModelBase
	{
		private bool _filteringByCreationDate;
		private bool _filteringByDeliveryDate = true;
		private bool _filteringByPaymentDate;
		private OrderDateFilterType _orderDateFilterType;

		public bool FilteringByCreationDate
		{
			get => _filteringByCreationDate;
			set
			{
				_filteringByCreationDate = value;
				if(_filteringByCreationDate)
				{
					SelectedOrderDateFilterType = OrderDateFilterType.CreationDate;
				}
			}
		}

		public bool FilteringByDeliveryDate
		{
			get => _filteringByDeliveryDate;
			set
			{
				_filteringByDeliveryDate = value;
				if(_filteringByDeliveryDate)
				{
					SelectedOrderDateFilterType = OrderDateFilterType.DeliveryDate;
				}
			}
		}

		public bool FilteringByPaymentDate
		{
			get => _filteringByPaymentDate;
			set
			{
				_filteringByPaymentDate = value;
				if(_filteringByPaymentDate)
				{
					SelectedOrderDateFilterType = OrderDateFilterType.PaymentDate;
				}
			}
		}

		public virtual OrderDateFilterType SelectedOrderDateFilterType
		{
			get => _orderDateFilterType;
			set => SetField(ref _orderDateFilterType, value);
		}
	}

	public enum OrderDateFilterType
	{
		CreationDate,
		DeliveryDate,
		PaymentDate
	}
}
