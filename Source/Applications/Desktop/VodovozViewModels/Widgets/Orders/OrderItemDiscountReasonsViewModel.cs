using QS.Commands;
using QS.DomainModel.Entity;
using QS.Extensions.Observable.Collections.List;
using QS.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Orders;
using VodovozBusiness.Controllers;

namespace Vodovoz.ViewModels.Widgets.Orders
{
	public class OrderItemDiscountReasonsViewModel : WidgetViewModelBase
	{
		private OrderItem _orderItem;
		private DiscountReason _newDiscountReason;
		private DiscountReason _selectedDiscountReason;
		private IList<DiscountReason> _applicableDiscountReasons = new List<DiscountReason>();
		private IObservableList<DiscountReason> _orderItemDiscountReasons = new ObservableList<DiscountReason>();

		private readonly IDiscountController _discountController;

		public OrderItemDiscountReasonsViewModel(IDiscountController discountController)
		{
			_discountController = discountController ?? throw new ArgumentNullException(nameof(discountController));

			AddDiscountReasonCommand = new DelegateCommand(AddDiscountReason, () => CanAddDiscountReason);
			AddDiscountReasonCommand.CanExecuteChangedWith(this, x => x.CanAddDiscountReason);
			DeleteDiscountReasonCommand = new DelegateCommand(DeleteDiscountReason, () => CanDeleteDiscountReason);
			DeleteDiscountReasonCommand.CanExecuteChangedWith(this, x => x.CanDeleteDiscountReason);
		}

		public DelegateCommand AddDiscountReasonCommand { get; }
		public DelegateCommand DeleteDiscountReasonCommand { get; }

		public event EventHandler<OrderItemDiscountReasonsChangedEventArgs> DiscountReasonsChanged;

		[PropertyChangedAlso(nameof(AvailableDiscountReasons))]
		public IObservableList<DiscountReason> OrderItemDiscountReasons
		{
			get => _orderItemDiscountReasons;
			private set => SetField(ref _orderItemDiscountReasons, value);
		}

		public IList<DiscountReason> AvailableDiscountReasons =>
			_applicableDiscountReasons
			.Where(x => !OrderItemDiscountReasons.Contains(x))
			.ToList();

		[PropertyChangedAlso(nameof(OrderItemDiscountReasons))]
		public OrderItem OrderItem
		{
			get => _orderItem;
			private set => SetField(ref _orderItem, value);
		}

		[PropertyChangedAlso(nameof(CanAddDiscountReason))]
		public DiscountReason NewDiscountReason
		{
			get => _newDiscountReason;
			set => SetField(ref _newDiscountReason, value);
		}

		[PropertyChangedAlso(nameof(CanDeleteDiscountReason))]
		public DiscountReason SelectedDiscountReason
		{
			get => _selectedDiscountReason;
			set => SetField(ref _selectedDiscountReason, value);
		}

		public void Initialize()
		{
			Update(null, null);
		}

		public void Update(OrderItem orderItem, IList<DiscountReason> discountReasons)
		{
			OrderItem = orderItem;

			OrderItemDiscountReasons.Clear();
			if(OrderItem?.DiscountReasons != null)
			{
				foreach(var dr in OrderItem.DiscountReasons)
				{
					OrderItemDiscountReasons.Add(dr);
				}
			}

			_applicableDiscountReasons.Clear();
			if(discountReasons != null)
			{
				foreach (var dr in discountReasons)
				{
					_applicableDiscountReasons.Add(dr);
				}
			}

			OnPropertyChanged(nameof(OrderItemDiscountReasons));
		}

		public bool CanAddDiscountReason => NewDiscountReason != null;

		public bool CanDeleteDiscountReason => SelectedDiscountReason != null;

		private void AddDiscountReason()
		{
			if(NewDiscountReason is null)
			{
				return;
			}

			OrderItem.DiscountReasons.Add(NewDiscountReason);

			OnPropertyChanged(nameof(OrderItemDiscountReasons));

			OnDiscountReasonsChanged();
		}

		private void DeleteDiscountReason()
		{
			if(SelectedDiscountReason is null)
			{
				return;
			}

			OrderItem.DiscountReasons.Remove(SelectedDiscountReason);

			OnPropertyChanged(nameof(OrderItemDiscountReasons));

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
