using QS.Commands;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.Extensions.Observable.Collections.List;
using QS.Services;
using QS.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Controllers;
using Vodovoz.Domain.Orders;

namespace Vodovoz.ViewModels.Widgets.Orders
{
	public class OrderItemDiscountReasonsViewModel : WidgetViewModelBase
	{
		private bool _isEditable;
		private OrderItem _orderItem;
		private DiscountReason _newDiscountReason;
		private DiscountReason _selectedDiscountReason;
		private IList<DiscountReason> _applicableDiscountReasons = new List<DiscountReason>();
		private IObservableList<DiscountReason> _orderItemDiscountReasons = new ObservableList<DiscountReason>();

		private readonly IOrderDiscountsController _orderDiscountController;
		private readonly ICommonServices _commonServices;
		private readonly IInteractiveService _interactiveService;

		public OrderItemDiscountReasonsViewModel(
			IOrderDiscountsController orderDiscountController,
			ICommonServices commonServices)
		{
			_orderDiscountController = orderDiscountController ?? throw new ArgumentNullException(nameof(orderDiscountController));
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			_interactiveService = commonServices.InteractiveService;

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

		public bool IsEditable
		{
			get => _isEditable;
			set => SetField(ref _isEditable, value);
		}

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

		public bool CanAddDiscountReason => NewDiscountReason != null;

		public bool CanDeleteDiscountReason => SelectedDiscountReason != null;

		public void Initialize()
		{
			Update(null, null);
		}

		public void Update(OrderItem orderItem, IList<DiscountReason> allDiscountReasons)
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
			if(allDiscountReasons != null)
			{
				foreach (var discountReason in allDiscountReasons)
				{
					if(!_orderDiscountController.IsApplicableDiscount(discountReason, orderItem.Nomenclature))
					{
						continue;
					}
					_applicableDiscountReasons.Add(discountReason);
				}
			}

			UpdateOrderItemDiscountReasons();
		}

		private void UpdateOrderItemDiscountReasons()
		{
			OrderItemDiscountReasons.Clear();

			if(OrderItem?.DiscountReasons != null)
			{
				foreach(var dr in OrderItem.DiscountReasons)
				{
					OrderItemDiscountReasons.Add(dr);
				}
			}

			OnPropertyChanged(nameof(OrderItemDiscountReasons));
		}

		private void AddDiscountReason()
		{
			if(NewDiscountReason is null)
			{
				return;
			}

			var addingDiscountResult =
				_orderDiscountController.AddtDiscountFromDiscountReasonForOrderItem(NewDiscountReason, OrderItem);

			if(addingDiscountResult.IsFailure)
			{
				_interactiveService.ShowMessage(
					ImportanceLevel.Error,
					string.Join(Environment.NewLine, addingDiscountResult.Errors.Select(e => e.Message)),
					"Не удалось добавить скидку с указанным основанием");
				return;
			}

			OnDiscountReasonsChanged();
		}

		private void DeleteDiscountReason()
		{
			if(SelectedDiscountReason is null)
			{
				return;
			}

			_orderDiscountController.RemoveOrdersItemDiscounts(SelectedDiscountReason, OrderItem);

			OnDiscountReasonsChanged();
		}

		protected virtual void OnDiscountReasonsChanged()
		{
			UpdateOrderItemDiscountReasons();
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
