using Gamma.Utilities;
using Microsoft.Extensions.Logging;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.Tdi;
using QS.ViewModels;
using System;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Orders.Documents;
using Vodovoz.Domain.Orders.OrdersWithoutShipment;
using Vodovoz.EntityRepositories;
using Vodovoz.Infrastructure.Print;
using Vodovoz.Parameters;
using Vodovoz.Services;
using Vodovoz.Settings.Database;
using Vodovoz.Specifications.Orders.EdoContainers;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Dialogs.Email;
using EdoDocumentType = Vodovoz.Domain.Orders.Documents.Type;

namespace Vodovoz.ViewModels.Orders.OrdersWithoutShipment
{
	public class OrderWithoutShipmentForDebtViewModel : EntityTabViewModelBase<OrderWithoutShipmentForDebt>, ITdiTabAddedNotifier
	{
		private readonly CommonMessages _commonMessages;
		private readonly IRDLPreviewOpener _rdlPreviewOpener;
		private readonly ICounterpartyJournalFactory _counterpartyJournalFactory;
		private IGenericRepository<EdoContainer> _edoContainerRepository;

		public Action<string> OpenCounterpartyJournal;
		private bool _isSendBillByEdo;
		private bool _canSendBillByEdo;

		public bool IsDocumentSent => Entity.IsBillWithoutShipmentSent;

		public OrderWithoutShipmentForDebtViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory uowFactory,
			ICommonServices commonServices,
			IEmployeeService employeeService,
			CommonMessages commonMessages,
			IRDLPreviewOpener rdlPreviewOpener,
			ICounterpartyJournalFactory counterpartyJournalFactory,
			IGenericRepository<EdoContainer> edoContainerRepository) : base(uowBuilder, uowFactory, commonServices)
		{
			if(employeeService == null)
			{
				throw new ArgumentNullException(nameof(employeeService));
			}

			_commonMessages = commonMessages ?? throw new ArgumentNullException(nameof(commonMessages));
			_rdlPreviewOpener = rdlPreviewOpener ?? throw new ArgumentNullException(nameof(rdlPreviewOpener));
			_counterpartyJournalFactory = counterpartyJournalFactory ?? throw new ArgumentNullException(nameof(counterpartyJournalFactory));
			_edoContainerRepository = edoContainerRepository ?? throw new ArgumentNullException(nameof(edoContainerRepository));

			bool canCreateBillsWithoutShipment =
				CommonServices.PermissionService.ValidateUserPresetPermission("can_create_bills_without_shipment", CurrentUser.Id);

			var currentEmployee = employeeService.GetEmployeeForUser(UoW, UserService.CurrentUserId);

			if(uowBuilder.IsNewEntity)
			{
				if(canCreateBillsWithoutShipment)
				{
					if(!AskQuestion("Вы действительно хотите создать счет без отгрузки на долг?"))
					{
						AbortOpening();
						return;
					}
					else
					{
						Entity.Author = currentEmployee;
					}
				}
				else
				{
					AbortOpening("У Вас нет прав на выставление счетов без отгрузки.");
					return;
				}
			}

			TabName = "Счет без отгрузки на долг";
			EntityUoWBuilder = uowBuilder;

			var loggerFactory = new LoggerFactory();
			var settingsController = new SettingsController(UnitOfWorkFactory, new Logger<SettingsController>(loggerFactory));
			SendDocViewModel =
				new SendDocumentByEmailViewModel(
					new EmailRepository(),
					new EmailParametersProvider(settingsController),
					currentEmployee,
					commonServices.InteractiveService,
					UoW);

			CanSendBillByEdo = false;

			UpdateEdoContainers();

			CanResendEdoBill = CommonServices.PermissionService.ValidateUserPresetPermission(Vodovoz.Permissions.EdoContainer.OrderWithoutShipmentForDebt.CanResendEdoBill, CurrentUser.Id)
				&& EdoContainers.Any();

			CancelCommand = new DelegateCommand(
				() => Close(true, CloseSource.Cancel),
				() => true);

			OpenBillCommand = new DelegateCommand(
				() =>
				{
					string whatToPrint = "документа \"" + Entity.Type.GetEnumTitle() + "\"";

					if(UoWGeneric.HasChanges && _commonMessages.SaveBeforePrint(typeof(OrderWithoutShipmentForDebt), whatToPrint))
					{
						if(Save(false))
						{
							_rdlPreviewOpener.OpenRldDocument(typeof(OrderWithoutShipmentForDebt), Entity);
						}
					}

					if(!UoWGeneric.HasChanges && Entity.Id > 0)
					{
						_rdlPreviewOpener.OpenRldDocument(typeof(OrderWithoutShipmentForDebt), Entity);
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

		public SendDocumentByEmailViewModel SendDocViewModel { get; set; }
		public IEntityUoWBuilder EntityUoWBuilder { get; }
		public ICounterpartyJournalFactory CounterpartyJournalFactory => _counterpartyJournalFactory;

		#region Commands

		public DelegateCommand CancelCommand { get; }
		
		public DelegateCommand OpenBillCommand { get; }

		private void OnEntityPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(Entity.Client))
			{
				CanSendBillByEdo = Entity.Client?.NeedSendBillByEdo ?? false;
			}
		}

		private void SendBillByEdo(IUnitOfWork uow)
		{
			var edoContainer = new EdoContainer
			{
				Type = EdoDocumentType.BillWSForDebt,
				Created = DateTime.Now,
				Container = new byte[64],
				OrderWithoutShipmentForDebt = Entity,
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
				foreach(var item in _edoContainerRepository.Get(uow, EdoContainerSpecification.CreateForOrderWithoutShipmentForDebtId(Entity.Id)))
				{
					EdoContainers.Add(item);
				}
			}
		}

		#endregion

		public GenericObservableList<EdoContainer> EdoContainers { get; } =
			new GenericObservableList<EdoContainer>();

		public bool CanResendEdoBill { get; }

		public void OnTabAdded()
		{
			if(EntityUoWBuilder.IsNewEntity)
			{
				OpenCounterpartyJournal?.Invoke(string.Empty);
			}
		}

		public void OnEntityViewModelEntryChanged(object sender, EventArgs e)
		{
			var email = Entity.GetEmailAddressForBill();
			SendDocViewModel.Update(Entity, email != null ? email.Address : string.Empty);
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
}
