using Autofac;
using EdoService.Library;
using Gamma.Utilities;
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
using Vodovoz.Core.Domain.Documents;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Orders.Documents;
using Vodovoz.Domain.Orders.OrdersWithoutShipment;
using Vodovoz.Domain.Organizations;
using Vodovoz.EntityRepositories;
using Vodovoz.Infrastructure.Print;
using Vodovoz.Services;
using Vodovoz.Settings.Common;
using Vodovoz.Settings.Organizations;
using Vodovoz.Specifications.Orders.EdoContainers;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Dialogs.Email;
using Vodovoz.ViewModels.Organizations;
using EdoDocumentType = Vodovoz.Core.Domain.Documents.DocumentContainerType;

namespace Vodovoz.ViewModels.Orders.OrdersWithoutShipment
{
	public class OrderWithoutShipmentForDebtViewModel : EntityTabViewModelBase<OrderWithoutShipmentForDebt>, ITdiTabAddedNotifier
	{
		private readonly CommonMessages _commonMessages;
		private readonly IRDLPreviewOpener _rdlPreviewOpener;
		private IGenericRepository<EdoContainer> _edoContainerRepository;
		private IGenericRepository<OrderEdoTrueMarkDocumentsActions> _orderEdoTrueMarkDocumentsActionsRepository;
		private readonly IEmailSettings _emailSettings;
		private readonly IEmailRepository _emailRepository;
		private readonly IEdoService _edoService;
		private readonly IOrganizationSettings _organizationSettings;
		public Action<string> OpenCounterpartyJournal;
		private bool _userHavePermissionToResendEdoDocuments;
		private bool _canSetOrganization = true;

		public bool IsDocumentSent => Entity.IsBillWithoutShipmentSent;

		public OrderWithoutShipmentForDebtViewModel(
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
			IOrganizationSettings organizationSettings) : base(uowBuilder, uowFactory, commonServices)
		{
			if(lifetimeScope == null)
			{
				throw new ArgumentNullException(nameof(lifetimeScope));
			}

			if(employeeService == null)
			{
				throw new ArgumentNullException(nameof(employeeService));
			}

			_commonMessages = commonMessages ?? throw new ArgumentNullException(nameof(commonMessages));
			_rdlPreviewOpener = rdlPreviewOpener ?? throw new ArgumentNullException(nameof(rdlPreviewOpener));
			_edoContainerRepository = edoContainerRepository ?? throw new ArgumentNullException(nameof(edoContainerRepository));
			_orderEdoTrueMarkDocumentsActionsRepository = orderEdoTrueMarkDocumentsActionsRepository ?? throw new ArgumentNullException(nameof(orderEdoTrueMarkDocumentsActionsRepository));
			_emailSettings = emailSettings ?? throw new ArgumentNullException(nameof(emailSettings));
			_edoService = edoService ?? throw new ArgumentNullException(nameof(edoService));
			_organizationSettings = organizationSettings;
			_emailRepository = emailRepository ?? throw new ArgumentNullException(nameof(emailRepository));
			CounterpartyAutocompleteSelectorFactory =
				(counterpartyJournalFactory ?? throw new ArgumentNullException(nameof(counterpartyJournalFactory)))
				.CreateCounterpartyAutocompleteSelectorFactory(lifetimeScope);

			OrganizationViewModel = organizationViewModelEEVMBuilder
				.SetUnitOfWork(UoW)
				.SetViewModel(this)
				.ForProperty(Entity, x => x.Organization)
				.UseViewModelJournalAndAutocompleter<OrganizationJournalViewModel>()
				.UseViewModelDialog<OrganizationViewModel>()
				.Finish();
			
			bool canCreateBillsWithoutShipment =
				CommonServices.PermissionService.ValidateUserPresetPermission("can_create_bills_without_shipment", CurrentUser.Id);
			_userHavePermissionToResendEdoDocuments = CommonServices.PermissionService.ValidateUserPresetPermission(Vodovoz.Core.Domain.Permissions.EdoContainerPermissions.OrderWithoutShipmentForDebt.CanResendEdoBill, CurrentUser.Id);

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

			SendDocViewModel =
				new SendDocumentByEmailViewModel(
					uowFactory,
					_emailRepository,
					_emailSettings,
					currentEmployee,
					commonServices,
					UoW);

			UpdateEdoContainers();

			CancelCommand = new DelegateCommand(
				() => Close(true, CloseSource.Cancel),
				() => true);

			OpenBillCommand = new DelegateCommand(
				() =>
				{
					string whatToPrint = "документа \"" + Entity.Type.GetEnumTitle() + "\"";
					
					if(Entity.Organization == null)
					{
						ShowErrorMessage("Необходимо выбрать организацию для сохранения счета");
						return;
					}
					
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
		}

		public IEntityAutocompleteSelectorFactory CounterpartyAutocompleteSelectorFactory { get; }
		
		public bool CanSendBillByEdo => Entity.Client?.NeedSendBillByEdo ?? false && !EdoContainers.Any();

		public bool CanSetOrganization
		{
			get => _canSetOrganization;
			set => SetField(ref _canSetOrganization, value);
		}

		public SendDocumentByEmailViewModel SendDocViewModel { get; set; }
		public IEntityUoWBuilder EntityUoWBuilder { get; }
		public IEntityEntryViewModel OrganizationViewModel { get; }
		
		#region Commands

		public DelegateCommand CancelCommand { get; }
		
		public DelegateCommand OpenBillCommand { get; }

		private void OnEntityPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(Entity.Client))
			{
				OnPropertyChanged(nameof(CanSendBillByEdo));
				OnPropertyChanged(nameof(CanResendEdoBill));
			}
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

			_edoService.SetNeedToResendEdoDocumentForOrder(Entity, EdoDocumentType.BillWSForDebt);

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
				foreach(var item in _edoContainerRepository.Get(uow, EdoContainerSpecification.CreateForOrderWithoutShipmentForDebtId(Entity.Id)))
				{
					EdoContainers.Add(item);
				}

				var action = _orderEdoTrueMarkDocumentsActionsRepository.Get(uow, x => x.OrderWithoutShipmentForDebt.Id == Entity.Id && x.IsNeedToResendEdoBill == true)
					.FirstOrDefault();

				if(action != null)
				{
					var tempContainer = new EdoContainer { Type = EdoDocumentType.BillWSForAdvancePayment, EdoDocFlowStatus = EdoDocFlowStatus.PreparingToSend};
					EdoContainers.Add(tempContainer);
				}
			}
		}

		#endregion
		
		public GenericObservableList<EdoContainer> EdoContainers { get; } = new GenericObservableList<EdoContainer>();

		public bool CanResendEdoBill => _userHavePermissionToResendEdoDocuments && EdoContainers.Any();

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
			OnPropertyChanged(nameof(CanSendBillByEdo));

			return base.Save(close);
		}
	}
}
