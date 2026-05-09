using QS.Commands;
using QS.DomainModel.Entity;
using QS.ViewModels;
using System;
using Vodovoz.Domain.Orders;

namespace Vodovoz.ViewModels.Widgets.Orders
{
	public class OrderItemDiscountReasonsViewModel : WidgetViewModelBase
	{
		private OrderItem _orderItem;
		private DiscountReason _newDiscountReason;
		private DiscountReason _selectedDiscountReason;

		public OrderItemDiscountReasonsViewModel()
		{
			AddDiscountReasonCommand = new DelegateCommand(AddDiscountReason, () => CanAddDiscountReason);
			AddDiscountReasonCommand.CanExecuteChangedWith(this, x => x.CanAddDiscountReason);
			DeleteDiscountReasonCommand = new DelegateCommand(DeleteDiscountReason, () => CanDeleteDiscountReason);
			DeleteDiscountReasonCommand.CanExecuteChangedWith(this, x => x.CanDeleteDiscountReason);
		}

		public DelegateCommand AddDiscountReasonCommand { get; }
		public DelegateCommand DeleteDiscountReasonCommand { get; }

		public event EventHandler<OrderItemDiscountReasonsChangedEventArgs> DiscountReasonsChanged;

		public OrderItem OrderItem
		{
			get => _orderItem;
			set => SetField(ref _orderItem, value);
		}

		public DiscountReason NewDiscountReason
		{
			get => _newDiscountReason;
			set => SetField(ref _newDiscountReason, value);
		}

		public DiscountReason SelectedDiscountReason
		{
			get => _selectedDiscountReason;
			set => SetField(ref _selectedDiscountReason, value);
		}

		[PropertyChangedAlso(nameof(NewDiscountReason))]
		public bool CanAddDiscountReason => NewDiscountReason != null;

		[PropertyChangedAlso(nameof(SelectedDiscountReason))]
		public bool CanDeleteDiscountReason => SelectedDiscountReason != null;

		private void AddDiscountReason()
		{
			if(NewDiscountReason is null)
			{
				return;
			}
			OrderItem.DiscountReasons.Add(NewDiscountReason);
			OnDiscountReasonsChanged();
		}

		private void DeleteDiscountReason()
		{
			if(SelectedDiscountReason is null)
			{
				return;
			}
			OrderItem.DiscountReasons.Remove(SelectedDiscountReason);
			OnDiscountReasonsChanged();
		}

		protected virtual void OnDiscountReasonsChanged()
		{
			DiscountReasonsChanged?.Invoke(this, new OrderItemDiscountReasonsChangedEventArgs(OrderItem));
		}
	}

	public class OrderItemDiscountReasonsChangedEventArgs : EventArgs
	{
		public OrderItemDiscountReasonsChangedEventArgs(OrderItem orderItem)
		{
			OrderItem = orderItem;
		}

		public OrderItem OrderItem { get; }
	}
}
