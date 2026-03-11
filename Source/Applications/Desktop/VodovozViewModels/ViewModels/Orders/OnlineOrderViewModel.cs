using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Autofac;
using Gamma.Utilities;
using Microsoft.Extensions.Logging;
using QS.Commands;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using QS.ViewModels.Control.EEVM;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Counterparties;
using Vodovoz.Extensions;
using Vodovoz.Filters.ViewModels;
using Vodovoz.Services;
using Vodovoz.Services.Orders;
using Vodovoz.ViewModels.Dialogs.Counterparties;
using Vodovoz.ViewModels.Journals.JournalViewModels.Client;
using Vodovoz.ViewModels.Journals.JournalViewModels.Orders;
using Vodovoz.ViewModels.ViewModels.Counterparty;
using VodovozBusiness.Controllers;
using VodovozBusiness.Domain.Orders;

namespace Vodovoz.ViewModels.ViewModels.Orders
{
	public class OnlineOrderViewModel : EntityTabViewModelBase<OnlineOrder>
	{
		private readonly IOrderFromOnlineOrderValidator _onlineOrderValidator;
		private readonly ViewModelEEVMBuilder<DeliveryPoint> _deliveryPointViewModelBuilder;
		private readonly DeliveryPointJournalFilterViewModel _deliveryPointJournalFilterViewModel;
		private readonly ILifetimeScope _lifetimeScope;
		private readonly Employee _currentEmployee;
		private bool _orderCreatingState;
		private bool _canCancelAnyOnlineOrder;

		public OnlineOrderViewModel(
			ILogger<OnlineOrderViewModel> logger,
			ILifetimeScope scope,
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			INavigationManager navigation,
			IEmployeeService employeeService,
			IOrderFromOnlineOrderValidator onlineOrderValidator,
			IExternalCounterpartyMatchingRepository externalCounterpartyMatchingRepository,
			ViewModelEEVMBuilder<DeliveryPoint> deliveryPointViewModelBuilder,
			DeliveryPointJournalFilterViewModel deliveryPointJournalFilterViewModel,
			IDiscountController discountController,
			IOrderOrganizationManager orderOrganizationManager
			)
			: base(uowBuilder, unitOfWorkFactory, commonServices, navigation)
		{
			_currentEmployee =
				(employeeService ?? throw new ArgumentNullException(nameof(employeeService)))
				.GetEmployeeForUser(UoW, CurrentUser.Id);

			if(_currentEmployee is null)
			{
				AbortOpening("Ваш пользователь не привязан к сотруднику. Дальнейшая работа не возможна");
			}

			TabName = Entity.ToString();

			_onlineOrderValidator = onlineOrderValidator ?? throw new ArgumentNullException(nameof(onlineOrderValidator));
			_deliveryPointViewModelBuilder =
				deliveryPointViewModelBuilder ?? throw new ArgumentNullException(nameof(deliveryPointViewModelBuilder));
			_deliveryPointJournalFilterViewModel =
				deliveryPointJournalFilterViewModel ?? throw new ArgumentNullException(nameof(deliveryPointJournalFilterViewModel));
			DiscountController = discountController ?? throw new ArgumentNullException(nameof(discountController));
			Logger = logger ?? throw new ArgumentNullException(nameof(logger));
			ExternalCounterpartyMatchingRepository =
				externalCounterpartyMatchingRepository ?? throw new ArgumentNullException(nameof(externalCounterpartyMatchingRepository));
			_lifetimeScope = scope ?? throw new ArgumentNullException(nameof(scope));
			OrderOrganizationManager = orderOrganizationManager ?? throw new ArgumentNullException(nameof(orderOrganizationManager));
			
			SetPermissions();
			CreateCommands();
			CreatePropertyChangeRelations();
			GetOnlineOrderItems();
			ConfigureEntryViewModels();
			TryValidateOnlineOrder();
		}

		public DelegateCommand GetToWorkCommand { get; private set; }
		public DelegateCommand CancelOnlineOrderCommand { get; private set; }
		public DelegateCommand OpenExternalCounterpartyMatchingCommand { get; private set; }
		public IList<OnlineOrderItem> OnlineOrderPromoItems { get; } = new List<OnlineOrderItem>();
		public IList<OnlineOrderItem> OnlineOrderNotPromoItems { get; } = new List<OnlineOrderItem>();
		public IList<OnlineFreeRentPackage> OnlineRentPackages { get; private set; }
		public ILogger<OnlineOrderViewModel> Logger { get; }
		public IExternalCounterpartyMatchingRepository ExternalCounterpartyMatchingRepository { get; }
		public IDiscountController DiscountController { get; }
		public IOrderOrganizationManager OrderOrganizationManager { get; }

		public bool CanShowId => Entity.Id > 0;

		public bool CanShowWarnings => !string.IsNullOrWhiteSpace(ValidationErrors);
		public bool CanGetToWork => Entity.EmployeeWorkWith is null;
		public bool CanChangeDeliveryPoint =>
			Entity.IsDeliveryPointNotBelongCounterparty.HasValue
			&& Entity.IsDeliveryPointNotBelongCounterparty.Value
			&& CurrentEmployeeIsEmployeeWorkWith
			&& OrderIsNullAndOnlineOrderNotCanceledStatus;

		public bool CanCreateOrder => OrderIsNullAndOnlineOrderNotCanceledStatus
			&& CurrentEmployeeIsEmployeeWorkWith
			&& (!Entity.IsDeliveryPointNotBelongCounterparty.HasValue || !Entity.IsDeliveryPointNotBelongCounterparty.Value);
		public bool CanCancelOnlineOrder =>
			OrderIsNullAndOnlineOrderNotCanceledStatus
			&& !_orderCreatingState
			&& (CurrentEmployeeIsEmployeeWorkWith || _canCancelAnyOnlineOrder);
		public bool CanEditCancellationReason => OrderIsNullAndOnlineOrderNotCanceledStatus;
		public bool CanShowSelfDeliveryGeoGroup => Entity.IsSelfDelivery;
		public bool CanShowEmployeeWorkWith => Entity.EmployeeWorkWith != null;
		public bool CanShowOrder => Entity.Orders.Any();
		public bool CanShowOnlinePayment => Entity.OnlinePayment.HasValue;
		public bool CanShowOnlinePaymentSource => Entity.OnlinePaymentSource.HasValue;
		public bool CanShowContactPhone => !string.IsNullOrWhiteSpace(Entity.ContactPhone);
		public bool CanShowNotPromoItems => OnlineOrderNotPromoItems.Any();
		public bool CanShowPromoItems => OnlineOrderPromoItems.Any();
		public bool CanShowRentPackages => OnlineRentPackages.Any();
		public bool CanShowCancellationReason => Entity.OnlineOrderCancellationReason != null;
		public bool CanOpenExternalCounterpartyMatching =>
			HasEmptyCounterpartyAndNotNullDataForMatching && CurrentEmployeeIsEmployeeWorkWith;
		public bool HasEmptyCounterpartyAndNotNullDataForMatching =>
			Entity.Counterparty is null
			&& Entity.ExternalCounterpartyId.HasValue
			&& !string.IsNullOrWhiteSpace(Entity.ContactPhone);
		
		public bool OrderCreatingState
		{
			get => _orderCreatingState;
			set
			{
				_orderCreatingState = value;
				OnPropertyChanged(nameof(CanCancelOnlineOrder));
			}
		}

		public string IdToString => Entity.Id.ToString();

		public string OnlineOrderStatusString => Entity.OnlineOrderStatus.GetEnumDisplayName();

		public string EmployeeWorkWith =>
			Entity.EmployeeWorkWith is null
				? "Онлайн заказ не взят в работу"
				: $"{ Entity.EmployeeWorkWith.ShortName }";

		public string Orders
		{
			get
			{
				if(!Entity.Orders.Any())
				{
					return "Заказ не создан";
				}

				var sb = new StringBuilder();

				foreach(var order in Entity.Orders)
				{
					sb.Append(order.Id);
					sb.Append(", ");
				}
					
				return sb.ToString().TrimEnd(',', ' ');
			}
		}

		public string Counterparty =>
			Entity.Counterparty is null
				? "Клиент не указан"
				: $"({ Entity.Counterparty.Id }) { Entity.Counterparty.Name }";

		public string DeliveryPoint =>
			Entity.DeliveryPoint is null
				? "Точка доставки не указана"
				: $"({ Entity.DeliveryPoint.Id }) { Entity.DeliveryPoint.CompiledAddress }";

		public string DeliverySchedule =>
			Entity.DeliverySchedule is null
				? "Время доставки не указано"
				: $"{ Entity.DeliverySchedule.Name }";

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
		
		public string CallBeforeArrivalMinutes =>
			!Entity.CallBeforeArrivalMinutes.HasValue
				? "Не нужен"
				: $"{ Entity.CallBeforeArrivalMinutes }мин.";

		public string OnlineOrderPaymentType => Entity.OnlineOrderPaymentType.GetEnumDisplayName();
		public string OnlineOrderDeliveryDate => Entity.DeliveryDate.ToShortDateString();
		public string OnlinePayment => Entity.OnlinePayment.ToString();
		public string OnlinePaymentSource =>
			Entity.OnlinePaymentSource.HasValue
				? Entity.OnlinePaymentSource.GetEnumTitle()
				: string.Empty;
		public string ValidationErrors { get; private set; }
		
		public IEntityEntryViewModel CancellationReasonViewModel { get; private set; }
		public IEntityEntryViewModel DeliveryPointViewModel { get; private set; }
		
		private bool CurrentEmployeeIsEmployeeWorkWith =>
			Entity.EmployeeWorkWith != null && Entity.EmployeeWorkWith.Id == _currentEmployee.Id;
		private bool OrderIsNullAndOnlineOrderNotCanceledStatus =>
			!Entity.Orders.Any() && Entity.OnlineOrderStatus != OnlineOrderStatus.Canceled;
		
		public OnlineOrderStatusUpdatedNotification CreateNewNotification() =>
			OnlineOrderStatusUpdatedNotification.Create(Entity);

		public void ShowMessage(string message, string title = null)
		{
			ShowInfoMessage(message, title);
		}
		
		public bool Question(string question, string title = null)
		{
			return AskQuestion(question, title);
		}
		
		private void SetPermissions()
		{
			var permissionService = CommonServices.PermissionService;
			
			_canCancelAnyOnlineOrder =
				permissionService.ValidateUserPresetPermission(Vodovoz.Core.Domain.Permissions.OnlineOrderPermissions.CanCancelAnyOnlineOrder, CurrentUser.Id);
		}
		
		private void CreateCommands()
		{
			CreateGetToWorkCommand();
			CreateCancelOnlineOrderCommand();
			CreateOpenExternalCounterpartyMatchingCommand();
		}

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
						Save(false);
					}
				});
		}
		
		private void CreateCancelOnlineOrderCommand()
		{
			CancelOnlineOrderCommand = new DelegateCommand(
				() =>
				{
					if(Entity.OnlineOrderCancellationReason is null)
					{
						ShowWarningMessage("Укажите причину отмены онлайн заказа");
						return;
					}
					
					var oldStatus = Entity.OnlineOrderStatus;
					Entity.OnlineOrderStatus = OnlineOrderStatus.Canceled;
					var notification = CreateNewNotification();
					UoW.Save(notification);
					
					if(!Save())
					{
						Entity.OnlineOrderStatus = oldStatus;
						return;
					}
					
					Close(false, CloseSource.Save);
				});
		}

		private void CreateOpenExternalCounterpartyMatchingCommand()
		{
			OpenExternalCounterpartyMatchingCommand = new DelegateCommand(
				() =>
				{
					if(Entity.ExternalCounterpartyId is null)
					{
						return;
					}
					
					var externalCounterpartyMatching =
						ExternalCounterpartyMatchingRepository.GetExternalCounterpartyMatching(
								UoW,
								Entity.ExternalCounterpartyId.Value,
								Entity.ContactPhone)
							.FirstOrDefault();
					
					if(externalCounterpartyMatching != null)
					{
						if(externalCounterpartyMatching.Status == ExternalCounterpartyMatchingStatus.Processed)
						{
							Entity.Counterparty = UoW.GetById<Domain.Client.Counterparty>(
								externalCounterpartyMatching.AssignedExternalCounterparty.Phone.Counterparty.Id);
							Save(false);
						}
						else
						{
							NavigationManager.OpenViewModel<ExternalCounterpartyMatchingViewModel, IEntityUoWBuilder>(
								this,
								EntityUoWBuilder.ForOpen(externalCounterpartyMatching.Id),
								OpenPageOptions.AsSlave,
								vm => vm.EntitySaved += (sender, args) =>
								{
									var matching = args.GetEntity<ExternalCounterpartyMatching>();
									if(matching.Status == ExternalCounterpartyMatchingStatus.Processed)
									{
										Entity.Counterparty =
											UoW.GetById<Domain.Client.Counterparty>(matching.AssignedExternalCounterparty.Phone.Counterparty.Id);

										Save(false);
									}
								});
						}
					}
					else
					{
						ShowMessage("Не найден запрос на ручное сопоставление клиента из внешних источников");
					}
				});
		}

		private void CreatePropertyChangeRelations()
		{
			Entity.PropertyChanged += OnEntityPropertyChanged;
			
			SetPropertyChangeRelation(
				e => e.Id,
				() => CanShowId,
				() => IdToString);
			
			SetPropertyChangeRelation(
				e => e.EmployeeWorkWith,
				() => CanShowEmployeeWorkWith,
				() => CanGetToWork,
				() => CanCreateOrder,
				() => CanCancelOnlineOrder,
				() => EmployeeWorkWith,
				() => CanOpenExternalCounterpartyMatching);
			
			SetPropertyChangeRelation(
				e => e.Counterparty,
				() => Counterparty,
				() => CanOpenExternalCounterpartyMatching);
			
			SetPropertyChangeRelation(
				e => e.OnlineOrderStatus,
				() => OnlineOrderStatusString,
				() => CanGetToWork,
				() => CanCreateOrder,
				() => CanCancelOnlineOrder,
				() => CanEditCancellationReason);
			
			SetPropertyChangeRelation(
				e => e.Orders,
				() => CanGetToWork,
				() => CanCreateOrder,
				() => CanCancelOnlineOrder,
				() => CanEditCancellationReason);
			
			SetPropertyChangeRelation(
				e => e.IsDeliveryPointNotBelongCounterparty,
				() => CanCreateOrder,
				() => CanChangeDeliveryPoint);
		}

		private void OnEntityPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(Entity.DeliveryPoint))
			{
				TryValidateOnlineOrder();
			}
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

			if(Entity.Counterparty != null)
			{
				_deliveryPointJournalFilterViewModel.Counterparty = Entity.Counterparty;
			}

			var deliveryPointViewModel =  _deliveryPointViewModelBuilder
				.SetUnitOfWork(UoW)
				.SetViewModel(this)
				.ForProperty(Entity, x => x.DeliveryPoint)
				.UseViewModelJournalAndAutocompleter<DeliveryPointByClientJournalViewModel, DeliveryPointJournalFilterViewModel>(
					_deliveryPointJournalFilterViewModel)
				.UseViewModelDialog<DeliveryPointViewModel>()
				.Finish();

			deliveryPointViewModel.CanViewEntity = false;
			DeliveryPointViewModel = deliveryPointViewModel;
		}
		
		private void TryValidateOnlineOrder()
		{
			ValidationErrors = null;
			
			if(Entity.OnlineOrderStatus != OnlineOrderStatus.New)
			{
				return;
			}
			
			var result = _onlineOrderValidator.ValidateOnlineOrder(UoW, Entity, true);

			if(result.IsFailure)
			{
				ValidationErrors = result.GetErrorsString();
			}
			
			OnPropertyChanged(nameof(ValidationErrors));
		}

		public override void Dispose()
		{
			Entity.PropertyChanged -= OnEntityPropertyChanged;
			base.Dispose();
		}
	}
}
