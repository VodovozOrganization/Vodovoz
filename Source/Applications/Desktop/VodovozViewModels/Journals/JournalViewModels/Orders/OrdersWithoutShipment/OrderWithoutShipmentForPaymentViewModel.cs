using Gamma.Utilities;
using Microsoft.Extensions.Logging;
using NHibernate.Criterion;
using NHibernate.Transform;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Services;
using QS.Tdi;
using QS.ViewModels;
using System;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Orders.Documents;
using Vodovoz.Domain.Orders.OrdersWithoutShipment;
using Vodovoz.EntityRepositories;
using Vodovoz.Infrastructure.Print;
using Vodovoz.NHibernateProjections.Orders;
using Vodovoz.Parameters;
using Vodovoz.Services;
using Vodovoz.Settings.Database;
using Vodovoz.Specifications.Orders.EdoContainers;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Dialogs.Email;
using EdoDocumentType = Vodovoz.Domain.Orders.Documents.Type;
using VodOrder = Vodovoz.Domain.Orders.Order;

namespace Vodovoz.ViewModels.Orders.OrdersWithoutShipment
{
	public class OrderWithoutShipmentForPaymentViewModel : EntityTabViewModelBase<OrderWithoutShipmentForPayment>, ITdiTabAddedNotifier
	{
		private readonly CommonMessages _commonMessages;
		private readonly IRDLPreviewOpener _rdlPreviewOpener;
		private readonly ICounterpartyJournalFactory _counterpartyJournalFactory;
		private IGenericRepository<EdoContainer> _edoContainerRepository;

		private DateTime? _startDate = DateTime.Now.AddMonths(-1);
		private DateTime? _endDate = DateTime.Now;

		public Action<string> OpenCounterpartyJournal;

		private bool _isSendBillByEdo;
		private bool _canSendBillByEdo;

		public OrderWithoutShipmentForPaymentViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory uowFactory,
			ICommonServices commonServices,
			IEmployeeService employeeService,
			IParametersProvider parametersProvider,
			CommonMessages commonMessages,
			IRDLPreviewOpener rdlPreviewOpener,
			ICounterpartyJournalFactory counterpartyJournalFactory,
			IGenericRepository<EdoContainer> edoContainerRepository)
			: base(uowBuilder, uowFactory, commonServices)
		{
			if(parametersProvider == null)
			{
				throw new ArgumentNullException(nameof(parametersProvider));
			}

			_commonMessages = commonMessages
				?? throw new ArgumentNullException(nameof(commonMessages));
			_rdlPreviewOpener = rdlPreviewOpener
				?? throw new ArgumentNullException(nameof(rdlPreviewOpener));
			_counterpartyJournalFactory = counterpartyJournalFactory
				?? throw new ArgumentNullException(nameof(counterpartyJournalFactory));
			_edoContainerRepository = edoContainerRepository
				?? throw new ArgumentNullException(nameof(edoContainerRepository));

			bool canCreateBillsWithoutShipment = CommonServices.PermissionService.ValidateUserPresetPermission("can_create_bills_without_shipment", CurrentUser.Id);

			CanResendEdoBill = CommonServices.PermissionService.ValidateUserPresetPermission(Vodovoz.Permissions.EdoContainer.OrderWithoutShipmentForDebt.CanResendEdoBill, CurrentUser.Id);

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

			var loggerFactory = new LoggerFactory();
			var settingsController = new SettingsController(UnitOfWorkFactory, new Logger<SettingsController>(loggerFactory));
			SendDocViewModel =
				new SendDocumentByEmailViewModel(
					new EmailRepository(),
					new EmailParametersProvider(settingsController),
					currentEmployee,
					commonServices.InteractiveService,
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

			if(!UoWGeneric.IsNew)
			{
				CanSendBillByEdo = Entity.Client.NeedSendBillByEdo;
			}
		}

		public bool IsSendBillByEdo
		{
			get => _isSendBillByEdo;
			set => SetField(ref _isSendBillByEdo, value);
		}

		public bool CanSendBillByEdo
		{
			get => _canSendBillByEdo;
			set
			{
				if(SetField(ref _canSendBillByEdo, value) && !value)
				{
					IsSendBillByEdo = false;
				}
			}
		}

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

		public bool IsDocumentSent => Entity.IsBillWithoutShipmentSent;

		#region Commands

		public DelegateCommand CancelCommand { get; }

		public DelegateCommand OpenBillCommand { get; }

		public ICounterpartyJournalFactory CounterpartyJournalFactory => _counterpartyJournalFactory;

		#endregion

		public GenericObservableList<EdoContainer> EdoContainers { get; } =
			new GenericObservableList<EdoContainer>();

		public bool CanResendEdoBill { get; }

		private void OnEntityPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(Entity.Client))
			{
				CanSendBillByEdo = Entity.Client?.NeedSendBillByEdo ?? false;
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

			var cashlessOrdersQuery = UoW.Session.QueryOver(() => orderAlias)
				.Left.JoinAlias(() => orderAlias.OrderItems, () => orderItemAlias)
				.Left.JoinAlias(() => orderAlias.DeliveryPoint, () => deliveryPointAlias)
				.Where(x => x.OrderStatus == OrderStatus.Closed)
				.Where(x => x.PaymentType == PaymentType.Cashless);

			if(Entity.Client != null)
			{
				cashlessOrdersQuery.Where(x => x.Client == Entity.Client);
			}

			if(StartDate.HasValue)
			{
				cashlessOrdersQuery.Where(x => x.DeliveryDate >= StartDate);
			}
			
			if(EndDate.HasValue)
			{
				cashlessOrdersQuery.Where(x => x.DeliveryDate <= EndDate);
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
				    .Select(() => deliveryPointAlias.CompiledAddress).WithAlias(() => resultAlias.DeliveryAddress)
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
		}

		private void SendBillByEdo(IUnitOfWork uow)
		{
			var edoContainer = new EdoContainer
			{
				Type = EdoDocumentType.BillWSForPayment,
				Created = DateTime.Now,
				Container = new byte[64],
				OrderWithoutShipmentForPayment = Entity,
				Counterparty = Entity.Counterparty,
				MainDocumentId = string.Empty,
				EdoDocFlowStatus = EdoDocFlowStatus.PreparingToSend
			};

			uow.Save();
			uow.Save(edoContainer);
			uow.Commit();
		}

		public void OnButtonSendDocumentAgainClicked(object sender, EventArgs e)
		{
			if(EdoContainers.Any(x => x.EdoDocFlowStatus == EdoDocFlowStatus.Succeed))
			{
				if(!CommonServices.InteractiveService.Question("Для данного заказа имеется документ со статусом \"Документооборот завершен успешно\".\nВы уверены, что хотите отправить дубль?"))
				{
					return;
				}
			}
			else if(EdoContainers.Any(x => x.EdoDocFlowStatus == EdoDocFlowStatus.InProgress))
			{
				if(!CommonServices.InteractiveService.Question("Для данного заказа имеется документ со статусом \"В процессе\".\nВы уверены, что хотите отправить дубль?"))
				{
					return;
				}
			}

			SendBillByEdo(UoW);
			UpdateEdoContainers();
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
			}
		}

		public override bool Save(bool close)
		{
			if(!Entity.IsBillWithoutShipmentSent && !EdoContainers.Any() && Entity.Id == 0 && IsSendBillByEdo)
			{
				SendBillByEdo(UoW);
			}

			return base.Save(close);
		}
	}

	public class OrderWithoutShipmentForPaymentNode : JournalEntityNodeBase<VodOrder>
	{
		public override string Title => $"{EntityType.GetSubjectNames()} №{Id}";
		public bool IsSelected { get; set; }
		public int OrderId { get; set; }
		public DateTime OrderDate { get; set; }
		public OrderStatus OrderStatus { get; set; }
		public decimal Bottles { get; set; }
		public decimal OrderSum { get; set; }
		public string DeliveryAddress { get; set; }
	}
}
