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
using System.Linq;
using System.Linq.Dynamic.Core;
using Vodovoz.Controllers;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Sale;
using Vodovoz.EntityRepositories.DiscountReasons;
using Vodovoz.Presentation.ViewModels.PaymentTypes;
using Vodovoz.TempAdapters;
using VodovozBusiness.Services.Orders;

namespace Vodovoz.ViewModels.Logistic
{
	public class SelfDeliveringOrderEditViewModel : EntityTabViewModelBase<Order>
	{
		private readonly IOrderDiscountsController _discountsController;
		private readonly IInteractiveService _interactiveService;
		private readonly ICurrentPermissionService _currentPermissionService;
		private readonly IDiscountReasonRepository _discountReasonRepository;
		private readonly ILifetimeScope _lifetimeScope;
		private readonly SelectPaymentTypeViewModel _selectPaymentTypeViewModel;

		private bool _canEditPriceDiscountFromRouteListAndSelfDelivery;
		private readonly IList<DiscountReason> _discountReasons;

		public SelfDeliveringOrderEditViewModel(

			ILifetimeScope lifetimeScope,
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			ICounterpartyJournalFactory counterpartyJournalFactory,
			IOrderDiscountsController discountsController,
			IInteractiveService interactiveService,
			ICurrentPermissionService currentPermissionService,
			IDiscountReasonRepository discountReasonRepository,
			SelectPaymentTypeViewModel selectPaymentTypeViewModel,
			INavigationManager navigation = null) : base(uowBuilder, unitOfWorkFactory, commonServices, navigation)
		{
			_discountsController = discountsController ?? throw new ArgumentNullException(nameof(discountsController));
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			_currentPermissionService = currentPermissionService ?? throw new ArgumentNullException(nameof(currentPermissionService));
			_discountReasonRepository = discountReasonRepository ?? throw new ArgumentNullException(nameof(discountReasonRepository));
			_lifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));
			_selectPaymentTypeViewModel = selectPaymentTypeViewModel ?? throw new ArgumentNullException(nameof(selectPaymentTypeViewModel));;

			_discountReasons = _canEditPriceDiscountFromRouteListAndSelfDelivery
				? _discountReasonRepository.GetActiveDiscountReasons(UoW)
				: _discountReasonRepository.GetActiveDiscountReasonsWithoutPremiums(UoW);

			var orderDate = Entity.DeliveryDate.HasValue
				? Entity.DeliveryDate.Value.ToString("dd.MM.yyyy")
				: "дата не указана";
			TabName = $"Редактирование самовывоза №{Entity.Id} от {orderDate}";

			SaveCommand = new DelegateCommand(() => SaveAndClose());
			CloseCommand = new DelegateCommand(() => Close(false, CloseSource.ClosePage));
			PaymentTypeCommand = new DelegateCommand(() => OnSelectPaymentTypeClicked());

			DiscountsController = _discountsController;
			CanChangeDiscountValue = _canEditPriceDiscountFromRouteListAndSelfDelivery;
			DiscountReasons = _discountReasons;
			LifetimeScope = _lifetimeScope;

			SetPermissions();
		}

		public DelegateCommand SaveCommand { get; }
		public DelegateCommand CloseCommand { get; }
		public DelegateCommand PaymentTypeCommand { get; }
		public bool CanChangeDiscountValue { get; }
		public IOrderDiscountsController DiscountsController { get; }
		public IList<DiscountReason> DiscountReasons { get; }
		public ILifetimeScope LifetimeScope { get; }


		public IEnumerable<GeoGroup> GetSelfDeliveryGeoGroups()
		{
			var currentGeoGroupId = Entity?.SelfDeliveryGeoGroup?.Id;

			var geoGroups = UoW.GetAll<GeoGroup>().Where(geo => !geo.IsArchived || geo.Id == currentGeoGroupId).ToList();

			return geoGroups;
		}

		public void ApplyDiscountReasonToOrderItem(OrderItem orderItem, int positionIndex)
		{
			var previousDiscountReason = orderItem.DiscountReason;

			//Дополнительно проверяем основание скидки на null, т.к при двойном щелчке
			//комбо-бокс не откроется, но событие сработает и прилетит null
			if(orderItem.DiscountReason != null)
			{
				if(!_discountsController.SetDiscountFromDiscountReasonForOrderItem(
					orderItem.DiscountReason, orderItem, _canEditPriceDiscountFromRouteListAndSelfDelivery, out string message))
				{
					orderItem.DiscountReason = previousDiscountReason;
				}

				if(message != null)
				{
					_interactiveService.ShowMessage(ImportanceLevel.Warning,
						$"На позицию:\n№{positionIndex + 1} {message}нельзя применить скидку," +
						" т.к. она из промонабора или на нее есть фикса.\nОбратитесь к руководителю");
				}
			}

			Entity?.RecalculateItemsPrice();
		}

		private void OnSelectPaymentTypeClicked()
		{
			NavigationManager.OpenViewModel<SelectPaymentTypeViewModel>(null, addingRegistrations: containerBuilder =>
			{
				containerBuilder.Register((cb) => _selectPaymentTypeViewModel);
			});
		}

		private void SetPermissions()
		{
			_canEditPriceDiscountFromRouteListAndSelfDelivery = _currentPermissionService.ValidatePresetPermission("can_edit_price_discount_from_route_list_and_self_delivery");
		}
	}
}
