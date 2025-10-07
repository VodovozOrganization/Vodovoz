using Autofac;
using EdoService.Library;
using Gamma.Utilities;
using NHibernate.Criterion;
using NHibernate.Transform;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal.EntitySelector;
using QS.Services;
using QS.Tdi;
using QS.ViewModels;
using System;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Dialog;
using Vodovoz.Core.Domain.Documents;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Orders.Documents;
using Vodovoz.Domain.Orders.OrdersWithoutShipment;
using Vodovoz.Domain.Organizations;
using Vodovoz.EntityRepositories;
using Vodovoz.Infrastructure.Print;
using Vodovoz.NHibernateProjections.Orders;
using Vodovoz.Services;
using Vodovoz.Settings;
using Vodovoz.Settings.Common;
using Vodovoz.Settings.Organizations;
using Vodovoz.Specifications.Orders.EdoContainers;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Dialogs.Email;
using Vodovoz.ViewModels.Organizations;
using EdoDocumentType = Vodovoz.Core.Domain.Documents.DocumentContainerType;
using VodOrder = Vodovoz.Domain.Orders.Order;

namespace Vodovoz.ViewModels.Orders.OrdersWithoutShipment
{
	public class OrderWithoutShipmentForPaymentViewModel : EntityTabViewModelBase<OrderWithoutShipmentForPayment>, ITdiTabAddedNotifier
	{
		private readonly CommonMessages _commonMessages;
		private readonly IRDLPreviewOpener _rdlPreviewOpener;
		private IGenericRepository<EdoContainer> _edoContainerRepository;
		private IGenericRepository<OrderEdoTrueMarkDocumentsActions> _orderEdoTrueMarkDocumentsActionsRepository;
		private readonly IEmailSettings _emailSettings;
		private readonly IEmailRepository _emailRepository;
		private readonly IEdoService _edoService;
		private readonly IOrganizationSettings _organizationSettings;
		private DateTime? _startDate = DateTime.Now.AddMonths(-1);
		private DateTime? _endDate = DateTime.Now;
		private Organization _organization;
		
		public Action<string> OpenCounterpartyJournal;

		private bool _userHavePermissionToResendEdoDocuments;
		private bool _canSetOrganization = true;

		public OrderWithoutShipmentForPaymentViewModel(
			ILifetimeScope lifetimeScope,
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory uowFactory,
			ICommonServices commonServices,
			IEmployeeService employeeService,
			CommonMessages commonMessages,
			IRDLPreviewOpener rdlPreviewOpener,
			ICounterpartyJournalFactory counterpartyJournalFactory,
			IGenericRepository<EdoContainer> edoContainerRepository,
			IGenericRepository<OrderEdoTrueMarkDocumentsActions> orderEdoTrueMarkDocumentsActionsRepository,
			IEmailSettings emailSettings,
			IEmailRepository emailRepository,
			IEdoService edoService,
			ViewModelEEVMBuilder<Organization> organizationViewModelEEVMBuilder,
			IOrganizationSettings organizationSettings)
			: base(uowBuilder, uowFactory, commonServices)
		{
			if(lifetimeScope == null)
			{
				throw new ArgumentNullException(nameof(lifetimeScope));
			}

			_commonMessages = commonMessages ?? throw new ArgumentNullException(nameof(commonMessages));
			_rdlPreviewOpener = rdlPreviewOpener ?? throw new ArgumentNullException(nameof(rdlPreviewOpener));
			_edoContainerRepository = edoContainerRepository ?? throw new ArgumentNullException(nameof(edoContainerRepository));
			_orderEdoTrueMarkDocumentsActionsRepository = orderEdoTrueMarkDocumentsActionsRepository ?? throw new ArgumentNullException(nameof(orderEdoTrueMarkDocumentsActionsRepository));
			_emailSettings = emailSettings ?? throw new ArgumentNullException(nameof(emailSettings));
			_emailRepository = emailRepository ?? throw new ArgumentNullException(nameof(emailRepository));
			_edoService = edoService ?? throw new ArgumentNullException(nameof(edoService));
			_organizationSettings = organizationSettings;
			CounterpartyAutocompleteSelectorFactory =
				(counterpartyJournalFactory ?? throw new ArgumentNullException(nameof(counterpartyJournalFactory)))
				.CreateCounterpartyAutocompleteSelectorFactory(lifetimeScope);
			
			OrganizationViewModel = organizationViewModelEEVMBuilder
				.SetUnitOfWork(UoW)
				.SetViewModel(this)
				.ForProperty(this, x => x.Organization)
				.UseViewModelJournalAndAutocompleter<OrganizationJournalViewModel>()
				.UseViewModelDialog<OrganizationViewModel>()
				.Finish();
			
			bool canCreateBillsWithoutShipment = CommonServices.PermissionService.ValidateUserPresetPermission("can_create_bills_without_shipment", CurrentUser.Id);
			_userHavePermissionToResendEdoDocuments = CommonServices.PermissionService.ValidateUserPresetPermission(Vodovoz.Core.Domain.Permissions.EdoContainerPermissions.OrderWithoutShipmentForDebt.CanResendEdoBill, CurrentUser.Id);

			var currentEmployee = employeeService.GetEmployeeForUser(UoW, UserService.CurrentUserId);

			if(uowBuilder.IsNewEntity)
			{
				if(canCreateBillsWithoutShipment)
				{
					if(!AskQuestion("Вы действительно хотите создать счет без отгрузки на постоплату?"))
					{
						AbortOpening();
					}
					else
					{
						Entity.Author = currentEmployee;
					}
				}
				else
				{
					AbortOpening("У Вас нет прав на выставление счетов без отгрузки.");
				}
			}

			TabName = "Счет без отгрузки на постоплату";

			SendDocViewModel =
				new SendDocumentByEmailViewModel(
					uowFactory,
					_emailRepository,
					_emailSettings,
					currentEmployee,
					commonServices,
					UoW);

			ObservableAvailableOrders = new GenericObservableList<OrderWithoutShipmentForPaymentNode>();

			UpdateEdoContainers();

			CancelCommand = new DelegateCommand(
				() => Close(true, CloseSource.Cancel),
				() => true);

			OpenBillCommand = new DelegateCommand(
				() =>
				{
					string whatToPrint = "документа \"" + Entity.Type.GetEnumTitle() + "\"";

					Entity.Organization = Organization ?? UoW.GetById<Organization>(_organizationSettings.GetCashlessOrganisationId);

					if(UoWGeneric.HasChanges && _commonMessages.SaveBeforePrint(typeof(OrderWithoutShipmentForPayment), whatToPrint))
					{
						if(Save(false))
						{
							_rdlPreviewOpener.OpenRldDocument(typeof(OrderWithoutShipmentForPayment), Entity);
						}
					}

					if(!UoWGeneric.HasChanges && Entity.Id > 0)
					{
						_rdlPreviewOpener.OpenRldDocument(typeof(OrderWithoutShipmentForPayment), Entity);
					}
				},
				() => true);

			Entity.PropertyChanged += OnEntityPropertyChanged;
			
			Organization = Entity.Id != 0 ? Entity.Organization : null;
		}

		public bool CanSendBillByEdo => Entity.Client?.NeedSendBillByEdo ?? false && !EdoContainers.Any();

		public DateTime? StartDate
		{
			get => _startDate;
			set => SetField(ref _startDate, value);
		}

		public DateTime? EndDate
		{
			get => _endDate;
			set => SetField(ref _endDate, value);
		}

		public OrderWithoutShipmentForPaymentNode SelectedNode { get; set; }
		public SendDocumentByEmailViewModel SendDocViewModel { get; set; }
		public GenericObservableList<OrderWithoutShipmentForPaymentNode> ObservableAvailableOrders { get; }
		
		public IEntityAutocompleteSelectorFactory CounterpartyAutocompleteSelectorFactory { get; }

		public IEntityEntryViewModel OrganizationViewModel { get; }
		
		public Organization Organization
		{
			get => _organization;
			set
			{
				SetField(ref _organization, value);
				UpdateAvailableOrders();
			}
		}

		public bool CanSetOrganization
		{
			get => _canSetOrganization;
			set => SetField(ref _canSetOrganization, value);
		}

		public bool IsDocumentSent => Entity.IsBillWithoutShipmentSent;

		#region Commands

		public DelegateCommand CancelCommand { get; }

		public DelegateCommand OpenBillCommand { get; }
		
		#endregion

		public GenericObservableList<EdoContainer> EdoContainers { get; } =
			new GenericObservableList<EdoContainer>();

		public bool CanResendEdoBill => _userHavePermissionToResendEdoDocuments && EdoContainers.Any();

		private void OnEntityPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(Entity.Client))
			{
				OnPropertyChanged(nameof(CanSendBillByEdo));
				OnPropertyChanged(nameof(CanResendEdoBill));
			}
		}

		public void UpdateAvailableOrders()
		{
			ObservableAvailableOrders.Clear();
			
			if(Entity.Client is null)
			{
				return;
			}

			OrderWithoutShipmentForPaymentNode resultAlias = null;
			VodOrder orderAlias = null;
			OrderItem orderItemAlias = null;
			Nomenclature nomenclatureAlias = null;
			DeliveryPoint deliveryPointAlias = null;
			CounterpartyContract counterpartyContractAlias = null;
			Organization contractOrganizationAlias = null;

			var cashlessOrdersQuery = UoW.Session.QueryOver(() => orderAlias)
				.Left.JoinAlias(() => orderAlias.OrderItems, () => orderItemAlias)
				.Left.JoinAlias(() => orderAlias.DeliveryPoint, () => deliveryPointAlias)
				.Left.JoinAlias(() => orderAlias.Contract, () => counterpartyContractAlias)
				.Left.JoinAlias(() => counterpartyContractAlias.Organization, () => contractOrganizationAlias)
				.Where(x => x.OrderStatus == OrderStatus.Closed)
				.Where(x => x.PaymentType == PaymentType.Cashless);

			if(Organization != null)
			{
				cashlessOrdersQuery = cashlessOrdersQuery.Where(x => contractOrganizationAlias.Id == Organization.Id);
			}
			
			if(Entity.Client != null)
			{
				cashlessOrdersQuery = cashlessOrdersQuery.Where(x => x.Client == Entity.Client);
			}

			if(StartDate.HasValue)
			{
				cashlessOrdersQuery = cashlessOrdersQuery.Where(x => x.DeliveryDate >= StartDate);
			}
			
			if(EndDate.HasValue)
			{
				cashlessOrdersQuery = cashlessOrdersQuery.Where(x => x.DeliveryDate <= EndDate);
			}
			
			var bottleCountSubQuery = QueryOver.Of(() => orderItemAlias)
				.Where(() => orderAlias.Id == orderItemAlias.Order.Id)
				.JoinAlias(() => orderItemAlias.Nomenclature, () => nomenclatureAlias)
				.Where(() => nomenclatureAlias.Category == NomenclatureCategory.water
					&& nomenclatureAlias.TareVolume == TareVolume.Vol19L)
				.Select(Projections.Sum(() => orderItemAlias.Count));

			var resultQuery = cashlessOrdersQuery
					.SelectList(list => list
					.SelectGroup(() => orderAlias.Id).WithAlias(() => resultAlias.OrderId)
				   	.Select(() => orderAlias.OrderStatus).WithAlias(() => resultAlias.OrderStatus)
				   	.Select(() => orderAlias.DeliveryDate).WithAlias(() => resultAlias.OrderDate)
				   	.Select(() => orderAlias.OrderPaymentStatus).WithAlias(() => resultAlias.OrderPaymentStatus)
				    .Select(() => deliveryPointAlias.CompiledAddress).WithAlias(() => resultAlias.DeliveryAddress)
				    .Select(() => contractOrganizationAlias.Name).WithAlias(() => resultAlias.OrganizationName)
					.Select(OrderProjections.GetOrderSumProjection()).WithAlias(() => resultAlias.OrderSum)
				    .SelectSubQuery(bottleCountSubQuery).WithAlias(() => resultAlias.Bottles))
				.OrderBy(x => x.DeliveryDate).Desc
				.TransformUsing(Transformers.AliasToBean<OrderWithoutShipmentForPaymentNode>())
				.List<OrderWithoutShipmentForPaymentNode>();

			foreach(var item in resultQuery)
			{
				ObservableAvailableOrders.Add(item);
			}
		}

		public void OnTabAdded()
		{
			if(Entity.Id == 0)
			{
				OpenCounterpartyJournal?.Invoke(string.Empty);
			}
		}

		public void OnEntityViewModelEntryChanged(object sender, EventArgs e)
		{
			var email = Entity.GetEmailAddressForBill();
			SendDocViewModel.Update(Entity, email != null ? email.Address : string.Empty);
			UpdateAvailableOrders();
		}

		public void UpdateItems()
		{
			var order = UoW.GetById<VodOrder>(SelectedNode.OrderId);

			if(order is null)
			{
				return;
			}
			
			if(SelectedNode.IsSelected)
			{
				Entity.AddOrder(order);
			}
			else
			{
				Entity.RemoveItem(order);
			}

			CanSetOrganization = Entity.ObservableOrderWithoutDeliveryForPaymentItems.Count == 0;
		}

		public void OnButtonSendDocumentAgainClicked(object sender, EventArgs e)
		{
			var edoValidateResult = _edoService.ValidateEdoContainers(EdoContainers);

			var errorMessages = edoValidateResult.Errors.Select(x => x.Message).ToArray();

			if(edoValidateResult.IsFailure)
			{
				if(edoValidateResult.Errors.Any(error => error.Code == Errors.Edo.EdoErrors.AlreadySuccefullSended)
					&& !CommonServices.InteractiveService.Question(
						"Вы уверены, что хотите отправить дубль?\n" +
						string.Join("\n", errorMessages),
						"Требуется подтверждение!"))
				{
					return;
				}
			}

			if(UoW.IsNew)
			{
				if(CommonServices.InteractiveService.Question("Перед отправкой необходимо сохранить счёт, продолжить?"))
				{
					UoW.Save();
				}
				else
				{
					return;
				}
			}

			_edoService.SetNeedToResendEdoDocumentForOrder(Entity, EdoDocumentType.BillWSForPayment);

			UpdateEdoContainers();

			OnPropertyChanged(nameof(CanSendBillByEdo));
			OnPropertyChanged(nameof(CanResendEdoBill));

			CommonServices.InteractiveService.ShowMessage(ImportanceLevel.Info, "Отправлено");
		}

		public void UpdateEdoContainers()
		{
			EdoContainers.Clear();

			using(var uow = UnitOfWorkFactory.CreateWithoutRoot())
			{
				foreach(var item in _edoContainerRepository.Get(uow, EdoContainerSpecification.CreateForOrderWithoutShipmentForPaymentId(Entity.Id)))
				{
					EdoContainers.Add(item);
				}

				var action = _orderEdoTrueMarkDocumentsActionsRepository.Get(uow, x => x.OrderWithoutShipmentForPayment.Id == Entity.Id && x.IsNeedToResendEdoBill == true)
					.FirstOrDefault();

				if(action != null)
				{
					var tempContainer = new EdoContainer { Type = EdoDocumentType.BillWSForAdvancePayment, EdoDocFlowStatus = EdoDocFlowStatus.PreparingToSend };
					EdoContainers.Add(tempContainer);
				}
			}
		}

		public override bool Save(bool close)
		{
			OnPropertyChanged(nameof(CanSendBillByEdo));

			return base.Save(close);
		}
	}
}
