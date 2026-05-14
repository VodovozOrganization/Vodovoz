using QS.Commands;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Extensions.Observable.Collections.List;
using QS.Services;
using QS.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Controllers;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.DiscountReasons;

namespace Vodovoz.ViewModels.Widgets.Orders
{
	public class OrderItemDiscountReasonsViewModel : WidgetViewModelBase
	{
		private bool _isEditEnabled;
		private IDiscount _orderItem;
		private DiscountReason _newDiscountReason;
		private DiscountReason _selectedDiscountReason;
		private IList<DiscountReason> _allDiscountReasons;
		private IList<DiscountReason> _applicableDiscountReasons = new List<DiscountReason>();
		private IObservableList<DiscountReason> _orderItemDiscountReasons = new ObservableList<DiscountReason>();

		private IUnitOfWork _uow;

		private readonly IOrderDiscountsController _orderDiscountController;
		private readonly ICommonServices _commonServices;
		private readonly IDiscountReasonRepository _discountReasonRepository;
		private readonly IInteractiveService _interactiveService;
		private readonly bool _userCanSetDirectDiscountValue;
		private readonly bool _isUserCanChoosePremiumDiscount;

		public OrderItemDiscountReasonsViewModel(
			IOrderDiscountsController orderDiscountController,
			ICommonServices commonServices,
			IDiscountReasonRepository discountReasonRepository)
		{
			_orderDiscountController = orderDiscountController ?? throw new ArgumentNullException(nameof(orderDiscountController));
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			_discountReasonRepository = discountReasonRepository ?? throw new ArgumentNullException(nameof(discountReasonRepository));

			_interactiveService =
				commonServices.InteractiveService;
			_userCanSetDirectDiscountValue =
				commonServices.CurrentPermissionService.ValidatePresetPermission(Vodovoz.Core.Domain.Permissions.OrderPermissions.UserCanSetDirectDiscountValue);
			_isUserCanChoosePremiumDiscount =
				commonServices.CurrentPermissionService.ValidatePresetPermission(Vodovoz.Core.Domain.Permissions.OrderPermissions.CanChoosePremiumDiscount);

			AddDiscountReasonCommand = new DelegateCommand(AddDiscountReason, () => CanAddDiscountReason);
			AddDiscountReasonCommand.CanExecuteChangedWith(this, x => x.CanAddDiscountReason);

			DeleteDiscountReasonCommand = new DelegateCommand(DeleteDiscountReason, () => CanDeleteDiscountReason);
			DeleteDiscountReasonCommand.CanExecuteChangedWith(this, x => x.CanDeleteDiscountReason);
		}

		public DelegateCommand AddDiscountReasonCommand { get; }
		public DelegateCommand DeleteDiscountReasonCommand { get; }

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

		[PropertyChangedAlso(nameof(IsEditable))]
		public bool IsEditEnabled
		{
			get => _isEditEnabled;
			set => SetField(ref _isEditEnabled, value);
		}

		[PropertyChangedAlso(
			nameof(OrderItemDiscountReasons),
			nameof(IsEditable))]
		public IDiscount OrderItem
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

		public bool IsEditable => IsEditEnabled && OrderItem != null;

		public bool IsInitialized => _uow != null && _allDiscountReasons != null;

		public void Initialize(IUnitOfWork uow)
		{
			if(IsInitialized)
			{
				throw new InvalidOperationException("ViewModel is already initialized");
			}

			_uow = uow ?? throw new ArgumentNullException(nameof(uow));

			SetAllDiscountReasons();
		}

		public void SetOrderItem(IDiscount orderItem)
		{
			if(orderItem is null)
			{
				throw new ArgumentNullException(nameof(orderItem));
			}

			if(!IsInitialized)
			{
				throw new InvalidOperationException("ViewModel must be initialized before setting order item");
			}

			UpdateOrderItem(orderItem);
		}

		public void ResetOrderItem()
		{
			if(!IsInitialized)
			{
				throw new InvalidOperationException("ViewModel must be initialized before resetting order item");
			}

			UpdateOrderItem();
		}

		private void UpdateOrderItem(IDiscount orderItem = null)
		{
			OrderItem = orderItem;
			UpdateOrderItemDiscountReasons();
			UpdateApplicableDiscountReasons();
		}

		private void UpdateApplicableDiscountReasons()
		{
			_applicableDiscountReasons.Clear();

			if(OrderItem is null)
			{
				return;
			}

			foreach(var discountReason in _allDiscountReasons)
			{
				if(!_orderDiscountController.IsApplicableDiscount(discountReason, OrderItem.Nomenclature))
				{
					continue;
				}
				_applicableDiscountReasons.Add(discountReason);
			}

			OnPropertyChanged(nameof(AvailableDiscountReasons));
		}

		private void SetAllDiscountReasons()
		{
			_allDiscountReasons =
				_discountReasonRepository.GetActiveDiscountReasonsFetchReferences(_uow, _isUserCanChoosePremiumDiscount);
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
				_orderDiscountController.AddDiscountFromDiscountReasonForOrderItem(NewDiscountReason, OrderItem, _userCanSetDirectDiscountValue);

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

			_orderDiscountController.RemoveDiscountFromOrdersItem(SelectedDiscountReason, OrderItem);

			OnDiscountReasonsChanged();
		}

		protected virtual void OnDiscountReasonsChanged()
		{
			UpdateOrderItemDiscountReasons();
		}
	}
}
