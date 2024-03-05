using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using QS.Commands;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.Tdi;
using QS.ViewModels;
using QS.ViewModels.Control.EEVM;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Orders;
using Vodovoz.Extensions;
using Vodovoz.Services;
using Vodovoz.Services.Orders;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.JournalViewModels.Orders;

namespace Vodovoz.ViewModels.ViewModels.Orders
{
	public class OnlineOrderViewModel : EntityTabViewModelBase<OnlineOrder>
	{
		private readonly IOrderFromOnlineOrderValidator _onlineOrderValidator;
		private readonly ILifetimeScope _lifetimeScope;
		private readonly IGtkTabsOpener _gtkTabsOpener;
		private readonly Employee _currentEmployee;

		public OnlineOrderViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			INavigationManager navigation,
			IEmployeeService employeeService,
			IOrderFromOnlineOrderValidator onlineOrderValidator,
			ILifetimeScope scope,
			IGtkTabsOpener gtkTabsOpener)
			: base(uowBuilder, unitOfWorkFactory, commonServices, navigation)
		{
			_currentEmployee =
				(employeeService ?? throw new ArgumentNullException(nameof(employeeService)))
				.GetEmployeeForUser(UoW, CurrentUser.Id);

			if(_currentEmployee is null)
			{
				AbortOpening("Ваш пользователь не привязан к сотруднику. Дальнейшая работа не возможна");
			}

			_onlineOrderValidator = onlineOrderValidator ?? throw new ArgumentNullException(nameof(onlineOrderValidator));
			_lifetimeScope = scope ?? throw new ArgumentNullException(nameof(scope));
			_gtkTabsOpener = gtkTabsOpener ?? throw new ArgumentNullException(nameof(gtkTabsOpener));
			
			CreateCommands();
			CreatePropertyChangeRelations();
			GetOnlineOrderItems();
			ConfigureEntryViewModels();
			ValidateOnlineOrder();
		}

		public DelegateCommand GetToWorkCommand { get; private set; }
		public DelegateCommand CancelOnlineOrderCommand { get; private set; }
		public IList<OnlineOrderItem> OnlineOrderPromoItems { get; } = new List<OnlineOrderItem>();
		public IList<OnlineOrderItem> OnlineOrderNotPromoItems { get; } = new List<OnlineOrderItem>();
		public IList<OnlineFreeRentPackage> OnlineRentPackages { get; private set; }

		private void CreateCommands()
		{
			CreateGetToWorkCommand();
			CreateCancelOnlineOrderCommand();
		}

		public bool CanShowId => Entity.Id > 0;

		public bool CanShowWarnings => !string.IsNullOrWhiteSpace(ValidationErrors);
		public bool CanGetToWork => Entity.Order is null;
		public bool CanCancelOnlineOrder => Entity.Order is null;
		public bool CanShowSelfDeliveryGeoGroup => Entity.IsSelfDelivery;
		public bool CanShowEmployeeWorkWith => Entity.EmployeeWorkWith != null;
		public bool CanShowOrder => Entity.Order != null;
		public bool CanShowOnlinePayment => Entity.OnlinePayment != null;
		public bool CanShowOnlinePaymentSource => Entity.OnlinePaymentSource != null;
		public bool CanShowContactPhone => !string.IsNullOrWhiteSpace(Entity.ContactPhone);
		public bool CanShowNotPromoItems => OnlineOrderNotPromoItems.Any();
		public bool CanShowPromoItems => OnlineOrderPromoItems.Any();
		public bool CanShowRentPackages => OnlineRentPackages.Any();
		public bool CanShowCancellationReason => Entity.OnlineOrderCancellationReason != null;

		public string IdToString => Entity.Id.ToString();

		public string OnlineOrderStatusString => Entity.OnlineOrderStatus.GetEnumDisplayName();

		public string EmployeeWorkWith =>
			Entity.EmployeeWorkWith is null
				? "Онлайн заказ не взят в работу"
				: $"{ Entity.EmployeeWorkWith.ShortName }";

		public string Order =>
			Entity.Order is null
				? "Заказ не создан"
				: $"{ Entity.Order.Title }";

		public string Counterparty =>
			Entity.Counterparty is null
				? "Клиент не указан"
				: $"{ Entity.Counterparty.Name }";

		public string DeliveryPoint =>
			Entity.DeliveryPoint is null
				? "Точка доставки не указана"
				: $"{ Entity.DeliveryPoint.CompiledAddress }";

		public string DeliverySchedule =>
			Entity.DeliverySchedule is null
				? "Время доставки не указано"
				: $"{ Entity.DeliverySchedule.DeliveryTime }";

		public string SelfDeliveryGeoGroup =>
			Entity.SelfDeliveryGeoGroup is null
				? "Район города не указан"
				: Entity.SelfDeliveryGeoGroup.Name;

		public string BottlesReturn =>
			Entity.BottlesReturn is null
				? "Планируемое количество бутылей не указано"
				: Entity.BottlesReturn.ToString();

		public string Trifle =>
			Entity.Trifle is null
				? "Сдача с не указана"
				: Entity.Trifle.ToString();

		public string OnlineOrderPaymentType => Entity.OnlineOrderPaymentType.GetEnumDisplayName();
		public string OnlineOrderDeliveryDate => Entity.DeliveryDate.ToShortDateString();
		public string OnlinePayment => Entity.OnlinePayment.ToString();
		public string OnlinePaymentSource => Entity.OnlinePaymentSource.ToString();
		public string ValidationErrors { get; private set; }
		
		public IEntityEntryViewModel CancellationReasonViewModel { get; private set; }

		private void CreateGetToWorkCommand()
		{
			GetToWorkCommand = new DelegateCommand(
				() =>
				{
					if(Entity.EmployeeWorkWith != null && Entity.EmployeeWorkWith.Id != _currentEmployee.Id)
					{
						ShowWarningMessage($"Эту заявку уже обрабатывает {Entity.EmployeeWorkWith.ShortName}. Дальнейшая работа не возможна");
						return;
					}

					if(Entity.EmployeeWorkWith is null)
					{
						Entity.EmployeeWorkWith = _currentEmployee;
					
						if(!Save(false))
						{
							return;
						}
					}

					OpenOrderDlgAndFillOnlineOrderData();
				});
		}
		
		private void CreateCancelOnlineOrderCommand()
		{
			CancelOnlineOrderCommand = new DelegateCommand(
				() =>
				{
					var oldStatus = Entity.OnlineOrderStatus;
					Entity.OnlineOrderStatus = OnlineOrderStatus.Canceled;
					
					if(!Save())
					{
						Entity.OnlineOrderStatus = oldStatus;
					}
				});
		}

		private void OpenOrderDlgAndFillOnlineOrderData()
		{
			var page = _gtkTabsOpener.OpenOrderDlgByNavigatorForCreateFromOnlineOrder(this, Entity);
			(page as ITdiDialog).EntitySaved += OnOrderSaved;
		}

		private void OnOrderSaved(object sender, EntitySavedEventArgs e)
		{
			var order = UoW.GetById<Order>(e.Entity.GetId());
			Entity.OnlineOrderStatus = OnlineOrderStatus.OrderPerformed;
			Entity.Order = order;
			Save(true);
		}

		private void CreatePropertyChangeRelations()
		{
			SetPropertyChangeRelation(
				e => e.Id,
				() => CanShowId,
				() => IdToString);
			
			SetPropertyChangeRelation(
				e => e.EmployeeWorkWith,
				() => CanShowEmployeeWorkWith,
				() => EmployeeWorkWith);
			
			SetPropertyChangeRelation(
				e => e.OnlineOrderStatus,
				() => OnlineOrderStatusString);
			
			SetPropertyChangeRelation(
				e => e.Order,
				() => CanGetToWork,
				() => CanCancelOnlineOrder);
			
			SetPropertyChangeRelation(
				e => e.OnlineOrderCancellationReason,
				() => CanShowCancellationReason);
		}
		
		private void GetOnlineOrderItems()
		{
			foreach(var item in Entity.OnlineOrderItems)
			{
				if(item.PromoSet != null)
				{
					OnlineOrderPromoItems.Add(item);
				}
				else
				{
					OnlineOrderNotPromoItems.Add(item);
				}
			}
			
			OnlineRentPackages = Entity.OnlineRentPackages;
		}
		
		private void ConfigureEntryViewModels()
		{
			var builder = new CommonEEVMBuilderFactory<OnlineOrder>(this, Entity, UoW, NavigationManager, _lifetimeScope);

			CancellationReasonViewModel = builder.ForProperty(x => x.OnlineOrderCancellationReason)
				.UseViewModelJournalAndAutocompleter<OnlineOrderCancellationReasonsJournalViewModel>()
				.UseViewModelDialog<OnlineOrderCancellationReasonViewModel>()
				.Finish();
		}
		
		private void ValidateOnlineOrder()
		{
			var result = _onlineOrderValidator.ValidateOnlineOrder(Entity);

			if(result.IsFailure)
			{
				ValidationErrors = result.GetErrorsString();
			}
		}
	}
}
