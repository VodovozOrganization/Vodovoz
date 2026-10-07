using Autofac;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Dynamic.Core;
using Vodovoz.Controllers;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Sale;
using Vodovoz.EntityRepositories.DiscountReasons;
using Vodovoz.Presentation.ViewModels.PaymentTypes;
using Vodovoz.ViewModels.Widgets.Orders;
using VodovozBusiness.Services.Orders;

namespace Vodovoz.ViewModels.Logistic
{
	public class SelfDeliveringOrderEditViewModel : EntityTabViewModelBase<Order>
	{
		private readonly IInteractiveService _interactiveService;
		private readonly ICurrentPermissionService _currentPermissionService;
		private readonly IOrderContractUpdater _orderContractUpdater;
		private readonly SelectPaymentTypeViewModel _selectPaymentTypeViewModel;
		private bool _canEditPriceDiscountFromRouteListAndSelfDelivery;
		private OrderItem _selectedOrderItem;

		public SelfDeliveringOrderEditViewModel(

			ILifetimeScope lifetimeScope,
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			IOrderDiscountsController discountsController,
			IInteractiveService interactiveService,
			ICurrentPermissionService currentPermissionService,
			IOrderContractUpdater orderContractUpdater,
			SelectPaymentTypeViewModel selectPaymentTypeViewModel,
			OrderItemDiscountReasonsViewModel orderItemDiscountReasonsViewModel,
			INavigationManager navigation = null) : base(uowBuilder, unitOfWorkFactory, commonServices, navigation)
		{
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			_currentPermissionService = currentPermissionService ?? throw new ArgumentNullException(nameof(currentPermissionService));
			_selectPaymentTypeViewModel = selectPaymentTypeViewModel ?? throw new ArgumentNullException(nameof(selectPaymentTypeViewModel));;
			_orderContractUpdater = orderContractUpdater ?? throw new ArgumentNullException(nameof(orderContractUpdater));

			OrderItemDiscountReasonsViewModel = orderItemDiscountReasonsViewModel ?? throw new ArgumentNullException(nameof(orderItemDiscountReasonsViewModel));

			SetPermissions();

			CanChangeDiscountValue = _canEditPriceDiscountFromRouteListAndSelfDelivery;

			OrderItemDiscountReasonsViewModel.Initialize(UoW);
			OrderItemDiscountReasonsViewModel.IsEditEnabled = CanChangeDiscountValue;

			DiscountsController = discountsController ?? throw new ArgumentNullException(nameof(discountsController));
			LifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));

			var orderDate = Entity.DeliveryDate.HasValue
				? Entity.DeliveryDate.Value.ToString("dd.MM.yyyy")
				: "дата не указана";
			TabName = $"Редактирование самовывоза №{Entity.Id} от {orderDate}";

			SaveCommand = new DelegateCommand(() => SaveAndClose());
			CloseCommand = new DelegateCommand(() => Close(false, CloseSource.ClosePage));
			PaymentTypeCommand = new DelegateCommand(() => OnSelectPaymentTypeClicked());
		}

		public DelegateCommand SaveCommand { get; }
		public DelegateCommand CloseCommand { get; }
		public DelegateCommand PaymentTypeCommand { get; }
		public bool CanChangeDiscountValue { get; }
		public IOrderDiscountsController DiscountsController { get; }
		public ILifetimeScope LifetimeScope { get; }

		/// <summary>
		/// ViewModel блока оснований скидки выбранной строки заказа
		/// </summary>
		public OrderItemDiscountReasonsViewModel OrderItemDiscountReasonsViewModel { get; }

		/// <summary>
		/// Выбранная в таблице строка заказа, основания которой показывает блок оснований скидки
		/// </summary>
		public OrderItem SelectedOrderItem
		{
			get => _selectedOrderItem;
			set
			{
				if(ReferenceEquals(_selectedOrderItem, value))
				{
					return;
				}

				UnsubscribeFromSelectedOrderItemDiscountReasons();
				SetField(ref _selectedOrderItem, value);
				SubscribeToSelectedOrderItemDiscountReasons();
				UpdateOrderItemDiscountReasonsViewModel();
			}
		}

		public IEnumerable<GeoGroup> GetSelfDeliveryGeoGroups()
		{
			var currentGeoGroupId = Entity?.SelfDeliveryGeoGroup?.Id;

			var geoGroups = UoW.GetAll<GeoGroup>().Where(geo => !geo.IsArchived || geo.Id == currentGeoGroupId).ToList();

			return geoGroups;
		}

		private void OnSelectPaymentTypeClicked()
		{
			NavigationManager.OpenViewModel<SelectPaymentTypeViewModel>(null, addingRegistrations: containerBuilder =>
			{
				containerBuilder.Register((cb) => _selectPaymentTypeViewModel);
			});

			_selectPaymentTypeViewModel.PaymentTypeSelected += OnPaymentTypeSelected;
		}

		private void OnPaymentTypeSelected(object sender, SelectPaymentTypeViewModel.PaymentTypeSelectedEventArgs e)
		{
			Entity.UpdatePaymentType(e.PaymentType, _orderContractUpdater);

			_selectPaymentTypeViewModel.PaymentTypeSelected -= OnPaymentTypeSelected;
		} 

		private void UpdateOrderItemDiscountReasonsViewModel()
		{
			if(SelectedOrderItem != null)
			{
				OrderItemDiscountReasonsViewModel.SetOrderItem(SelectedOrderItem);
			}
			else
			{
				OrderItemDiscountReasonsViewModel.ResetOrderItem();
			}

			OrderItemDiscountReasonsViewModel.NewDiscountReason = null;
			OrderItemDiscountReasonsViewModel.SelectedDiscountReason = null;
		}

		private void SubscribeToSelectedOrderItemDiscountReasons()
		{
			if(_selectedOrderItem?.DiscountReasons != null)
			{
				_selectedOrderItem.DiscountReasons.CollectionChanged += OnSelectedOrderItemDiscountReasonsChanged;
			}
		}

		private void UnsubscribeFromSelectedOrderItemDiscountReasons()
		{
			if(_selectedOrderItem?.DiscountReasons != null)
			{
				_selectedOrderItem.DiscountReasons.CollectionChanged -= OnSelectedOrderItemDiscountReasonsChanged;
			}
		}

		private void OnSelectedOrderItemDiscountReasonsChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if(SelectedOrderItem != null)
			{
				OrderItemDiscountReasonsViewModel.SetOrderItem(SelectedOrderItem);
			}
		}

		public override void Dispose()
		{
			UnsubscribeFromSelectedOrderItemDiscountReasons();
			base.Dispose();
		}

		private void SetPermissions()
		{
			_canEditPriceDiscountFromRouteListAndSelfDelivery = _currentPermissionService.
				ValidatePresetPermission(Core.Domain.Permissions.OrderPermissions.CanEditPriceDiscountFromRouteListAndSelfDelivery);
		}
	}
}
