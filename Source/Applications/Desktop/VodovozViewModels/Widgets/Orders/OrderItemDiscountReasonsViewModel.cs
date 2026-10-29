using QS.Commands;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Extensions.Observable.Collections.List;
using QS.Services;
using QS.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.DiscountReasons;
using VodovozBusiness.Controllers;
using VodovozBusiness.Domain.Orders;

namespace Vodovoz.ViewModels.Widgets.Orders
{
	public class OrderItemDiscountReasonsViewModel : WidgetViewModelBase, IDisposable
	{
		private bool _isEditEnabled;
		private IApplyDiscountReasonItem _saleItem;
		private DiscountReasonBase _newDiscountReason;
		private DiscountReasonBase _selectedDiscountReason;
		private IList<DiscountReasonBase> _allDiscountReasons;
		private IList<DiscountReasonBase> _applicableDiscountReasons = new List<DiscountReasonBase>();
		private IObservableList<DiscountReasonBase> _saleItemDiscountReasons = new ObservableList<DiscountReasonBase>();

		private IUnitOfWork _uow;
		private ISaleDiscountController _saleDiscountController;

		private readonly ICommonServices _commonServices;
		private readonly IDiscountReasonRepository _discountReasonRepository;
		private readonly IInteractiveService _interactiveService;
		private readonly bool _userCanSetDirectDiscountValue;
		private readonly bool _isUserCanChoosePremiumDiscount;

		public OrderItemDiscountReasonsViewModel(
			ICommonServices commonServices,
			IDiscountReasonRepository discountReasonRepository)
		{
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
		public IObservableList<DiscountReasonBase> SaleItemDiscountReasons
		{
			get => _saleItemDiscountReasons;
			private set => SetField(ref _saleItemDiscountReasons, value);
		}

		public IList<DiscountReasonBase> AvailableDiscountReasons =>
			_applicableDiscountReasons
			.Where(x => !SaleItemDiscountReasons.Contains(x))
			.ToList();

		[PropertyChangedAlso(nameof(IsEditable))]
		public bool IsEditEnabled
		{
			get => _isEditEnabled;
			set => SetField(ref _isEditEnabled, value);
		}

		[PropertyChangedAlso(
			nameof(SaleItemDiscountReasons),
			nameof(IsEditable))]
		public IApplyDiscountReasonItem SaleItem
		{
			get => _saleItem;
			private set => SetField(ref _saleItem, value);
		}

		[PropertyChangedAlso(nameof(CanAddDiscountReason))]
		public DiscountReasonBase NewDiscountReason
		{
			get => _newDiscountReason;
			set => SetField(ref _newDiscountReason, value);
		}

		[PropertyChangedAlso(nameof(CanDeleteDiscountReason))]
		public DiscountReasonBase SelectedDiscountReason
		{
			get => _selectedDiscountReason;
			set => SetField(ref _selectedDiscountReason, value);
		}

		public bool CanAddDiscountReason => NewDiscountReason != null;

		public bool CanDeleteDiscountReason => SelectedDiscountReason != null;

		public bool IsEditable => IsEditEnabled && SaleItem != null;

		public bool IsInitialized => _uow != null && _allDiscountReasons != null;

		public void Initialize(IUnitOfWork uow, ISaleDiscountController saleDiscountController)
		{
			if(IsInitialized)
			{
				throw new InvalidOperationException("ViewModel is already initialized");
			}

			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
			_saleDiscountController = saleDiscountController ?? throw new ArgumentNullException(nameof(saleDiscountController));

			SetAllDiscountReasons();
		}

		public void SetSaleItem(IApplyDiscountReasonItem saleItem)
		{
			if(saleItem is null)
			{
				throw new ArgumentNullException(nameof(saleItem));
			}

			if(!IsInitialized)
			{
				throw new InvalidOperationException("ViewModel must be initialized before setting sale item");
			}

			UpdateSaleItem(saleItem);
		}

		public void ResetSaleItem()
		{
			if(!IsInitialized)
			{
				throw new InvalidOperationException("ViewModel must be initialized before resetting sale item");
			}

			UpdateSaleItem();
		}

		private void UpdateSaleItem(IApplyDiscountReasonItem saleItem = null)
		{
			UnSubscribeOrderItemDiscountReasons();

			SaleItem = saleItem;

			SubscribeOrderItemDiscountReasons();

			UpdateOrderItemDiscountReasons();
			UpdateApplicableDiscountReasons();
		}

		private void SubscribeOrderItemDiscountReasons()
		{
			if(SaleItem?.DiscountReasons is INotifyCollectionChanged newObservable)
			{
				newObservable.CollectionChanged += OnDiscountReasonsCollectionChanged;
			}
		}

		private void UnSubscribeOrderItemDiscountReasons()
		{
			if(_saleItem?.DiscountReasons is INotifyCollectionChanged oldObservable)
			{
				oldObservable.CollectionChanged -= OnDiscountReasonsCollectionChanged;
			}
		}

		private void OnDiscountReasonsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			OnDiscountReasonsChanged();
		}

		private void UpdateApplicableDiscountReasons()
		{
			_applicableDiscountReasons.Clear();

			if(SaleItem is null)
			{
				return;
			}

			foreach(var discountReason in _allDiscountReasons)
			{
				var isApplicableResult = _saleDiscountController.IsApplicableDiscount(discountReason, SaleItem);
				
				if(isApplicableResult.IsFailure)
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
			SaleItemDiscountReasons.Clear();

			if(SaleItem?.DiscountReasons != null)
			{
				foreach(var dr in SaleItem.DiscountReasons)
				{
					SaleItemDiscountReasons.Add(dr);
				}
			}

			OnPropertyChanged(nameof(SaleItemDiscountReasons));
		}

		private void AddDiscountReason()
		{
			if(SaleItem is null || NewDiscountReason is null)
			{
				return;
			}

			var discountValue = DiscountValue.Create(
				NewDiscountReason.ValueType == DiscountUnits.money,
				NewDiscountReason.Value,
				NewDiscountReason.Value);

			if(!_saleDiscountController.IsDiscountValueCanBeAdded(discountValue, SaleItem))
			{
				_interactiveService.ShowMessage(
					ImportanceLevel.Warning,
					"Суммарное значение скидок превышает сумму строки заказа. Скидка будет добавлена, но будет пересчитана");
			}

			var addingDiscountResult =
				_saleDiscountController.AddDiscountFromDiscountReason(NewDiscountReason, SaleItem, _userCanSetDirectDiscountValue);

			if(addingDiscountResult.IsFailure)
			{
				_interactiveService.ShowMessage(
					ImportanceLevel.Warning,
					string.Join(Environment.NewLine, addingDiscountResult.Errors.Select(e => e.Message)),
					"Не удалось добавить скидку с указанным основанием");
			}
		}

		private void DeleteDiscountReason()
		{
			if(SelectedDiscountReason is null)
			{
				return;
			}

			_saleDiscountController.RemoveDiscount(SelectedDiscountReason.Id, SaleItem);
		}

		protected virtual void OnDiscountReasonsChanged()
		{
			UpdateOrderItemDiscountReasons();
		}

		public void Dispose()
		{
			UnSubscribeOrderItemDiscountReasons();
		}
	}
}
