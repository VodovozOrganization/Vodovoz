using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Windows.Input;
using Autofac;
using EdoService.Library.Dto;
using EdoService.Library.Services;
using Microsoft.Extensions.Logging;
using QS.Attachments.Domain;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Services;
using QS.Tdi;
using QS.Validation;
using QS.ViewModels;
using TISystems.TTC.CRM.BE.Serialization;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Contacts;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Organizations;
using Vodovoz.Factories;
using Vodovoz.Settings.Edo;
using Vodovoz.Settings.Organizations;
using VodovozBusiness.Domain.Client;

namespace Vodovoz.ViewModels.ViewModels.Counterparty
{
	public class EdoAccountViewModel : EntityWidgetViewModelBase<CounterpartyEdoAccount>, IDisposable
	{
		private readonly ILogger<EdoAccountViewModel> _logger;
		private readonly IContactListService _contactListService;
		private readonly IEdoSettings _edoSettings;
		private readonly IOrganizationSettings _organizationSettings;
		private readonly IValidator _validator;
		private readonly ValidationContext _counterpartyValidationContext;

		private int _organizationId => Entity.OrganizationId ?? 0;

		public EdoAccountViewModel(
			ILogger<EdoAccountViewModel> logger,
			IUnitOfWork uow,
			ILifetimeScope scope,
			Domain.Client.Counterparty counterparty,
			CounterpartyEdoAccount edoAccount,
			IContactListService contactListService,
			ITdiTab parentTab,
			ICommonServices commonServices,
			IEdoSettings edoSettings,
			IOrganizationSettings organizationSettings,
			INavigationManager navigationManager,
			IValidator validator,
			IValidationContextFactory validationContextFactory) : base(edoAccount, commonServices)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_contactListService = contactListService ?? throw new ArgumentNullException(nameof(contactListService));
			_edoSettings = edoSettings ?? throw new ArgumentNullException(nameof(edoSettings));
			_organizationSettings = organizationSettings ?? throw new ArgumentNullException(nameof(organizationSettings));
			_validator = validator ?? throw new ArgumentNullException(nameof(validator));
			UoW = uow ?? throw new ArgumentNullException(nameof(uow));
			Scope = scope ?? throw new ArgumentNullException(nameof(scope));
			NavigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
			Counterparty = counterparty;
			ParentTab = parentTab;
			_counterpartyValidationContext =
				(validationContextFactory ?? throw new ArgumentNullException(nameof(validationContextFactory)))
				.CreateNewValidationContext(Counterparty);

			Initialize();
			Entity.Counterparty.PropertyChanged += OnCounterpartyPropertyChanged;
		}

		public event Action RefreshEdoLightsMatrixAction;
		public event Action<CounterpartyEdoAccount> RemovedEdoAccountAction;
		public Action UpdateOperators { get; set; }
		public Domain.Client.Counterparty Counterparty { get; private set; }
		public ITdiTab ParentTab { get; private set; }
		public ILifetimeScope Scope { get; private set; }
		public INavigationManager NavigationManager { get; }
		
		public ICommand CheckClientInTaxcomCommand { get; private set; }
		public ICommand CheckConsentForEdoCommand { get; private set; }
		public ICommand SendInviteByTaxcomCommand { get; private set; }
		public ICommand SendManualInviteByTaxcomCommand { get; private set; }
		public ICommand RemoveEdoAccountCommand { get; private set; }
		public ICommand CopyFromEdoOperatorWithAccountCommand { get; private set; }

		public bool CanCheckConsentForEdo => true;
		
		public bool CanSendInviteByTaxcom => Entity.EdoOperator != null
			&& !string.IsNullOrWhiteSpace(Entity.PersonalAccountIdInEdo)
			&& Entity.ConsentForEdoStatus == ConsentForEdoStatus.Unknown;
		
		public bool CanSendManualInviteByTaxcom => Entity.EdoOperator != null
			&& Entity.ConsentForEdoStatus == ConsentForEdoStatus.Unknown;
		
		public bool CanCheckClientInTaxcom => Counterparty != null
			&& Counterparty.PersonType == PersonType.legal
			&& (Counterparty.ReasonForLeaving == ReasonForLeaving.ForOwnNeeds 
				|| Counterparty.ReasonForLeaving == ReasonForLeaving.Resale);
		
		public bool CanEditPersonalAccountCodeInEdo => Counterparty != null
			&& Counterparty.PersonType == PersonType.legal
			&& Counterparty.ReasonForLeaving != ReasonForLeaving.Unknown
			&& Counterparty.ReasonForLeaving != ReasonForLeaving.Other;
		
		public bool CanSelectRegisteredEdoAccount => Counterparty != null
			&& Counterparty.PersonType == PersonType.legal
			&& Counterparty.ReasonForLeaving != ReasonForLeaving.Unknown
			&& Counterparty.ReasonForLeaving != ReasonForLeaving.Other;
		
		public bool CanChangeOperatorEdo => Counterparty != null
			&& Counterparty.PersonType == PersonType.legal
			&& Counterparty.ReasonForLeaving != ReasonForLeaving.Unknown
			&& Counterparty.ReasonForLeaving != ReasonForLeaving.Other;

		public bool CanCopyFromEdoOperatorWithAccount =>
			Entity.ConsentForEdoStatus != ConsentForEdoStatus.Sent
			&& Entity.ConsentForEdoStatus != ConsentForEdoStatus.Agree;

		public void ResetConsentForEdo()
		{
			Entity.ConsentForEdoStatus = ConsentForEdoStatus.Unknown;
			TryRefreshEdoLightsMatrix();
		}
		
		public void TryFillEdoAccount(CounterpartyEdoOperator selectedEdoOperator)
		{
			if(!string.IsNullOrWhiteSpace(Entity.PersonalAccountIdInEdo))
			{
				if(!CommonServices.InteractiveService.Question("Вы уверены что хотите подставить выбранные данные в аккаунт?"))
				{
					return;
				}
			}

			Entity.PersonalAccountIdInEdo = selectedEdoOperator.PersonalAccountIdInEdo;
			Entity.EdoOperator = selectedEdoOperator.EdoOperator;
			Entity.ConsentForEdoStatus = ConsentForEdoStatus.Unknown;
			TryRefreshEdoLightsMatrix();
		}
		
		private void Initialize()
		{
			var checkConsentForEdoCommand = new DelegateCommand(CheckConsentForEdo, () => CanCheckConsentForEdo);
			checkConsentForEdoCommand.CanExecuteChangedWith(this, x => x.CanCheckConsentForEdo);
			CheckConsentForEdoCommand = checkConsentForEdoCommand;
			
			var checkClientCommand = new DelegateCommand(CheckClientInTaxcom, () => CanCheckClientInTaxcom);
			checkClientCommand.CanExecuteChangedWith(this, x => x.CanCheckClientInTaxcom);
			CheckClientInTaxcomCommand = checkClientCommand;
			
			var sendInviteByTaxcomCommand = new DelegateCommand(SendInviteByTaxcom, () => CanSendInviteByTaxcom);
			sendInviteByTaxcomCommand.CanExecuteChangedWith(this, x => x.CanSendInviteByTaxcom);
			SendInviteByTaxcomCommand = sendInviteByTaxcomCommand;
			
			var sendManualInviteByTaxcomCommand = new DelegateCommand(SendManualInviteByTaxcom, () => CanSendManualInviteByTaxcom);
			sendManualInviteByTaxcomCommand.CanExecuteChangedWith(this, x => x.CanSendManualInviteByTaxcom);
			SendManualInviteByTaxcomCommand = sendManualInviteByTaxcomCommand;
			
			var removeEdoAccountCommand = new DelegateCommand(RemoveEdoAccount);
			RemoveEdoAccountCommand = removeEdoAccountCommand;

			var copyFromEdoOperatorWithAccountCommand = new DelegateCommand<CounterpartyEdoAccount>(
				CopyFromEdoOperatorWithAccount,
				acc => CanCopyFromEdoOperatorWithAccount);
			copyFromEdoOperatorWithAccountCommand.CanExecuteChangedWith(this, x => x.CanCopyFromEdoOperatorWithAccount);
			CopyFromEdoOperatorWithAccountCommand = copyFromEdoOperatorWithAccountCommand;

			SetPropertyChangeRelations();
		}
		
		private void OnCounterpartyPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(Counterparty.PersonType) || e.PropertyName == nameof(Counterparty.ReasonForLeaving))
			{
				OnPropertyChanged(nameof(CanCheckClientInTaxcom));
				OnPropertyChanged(nameof(CanEditPersonalAccountCodeInEdo));
				OnPropertyChanged(nameof(CanSelectRegisteredEdoAccount));
				OnPropertyChanged(nameof(CanChangeOperatorEdo));
			}
		}

		private void SetPropertyChangeRelations()
		{
			SetPropertyChangeRelation(
				e => e.EdoOperator,
				() => CanSendInviteByTaxcom,
				() => CanSendManualInviteByTaxcom
			);
			
			SetPropertyChangeRelation(
				e => e.PersonalAccountIdInEdo,
				() => CanSendInviteByTaxcom
			);
			
			SetPropertyChangeRelation(
				e => e.ConsentForEdoStatus,
				() => CanSendInviteByTaxcom,
				() => CanSendManualInviteByTaxcom,
				() => CanCheckConsentForEdo,
				() => CanCopyFromEdoOperatorWithAccount
			);
		}

		private void CheckClientInTaxcom()
		{
			ContactList contactResult;

			try
			{
				contactResult = _contactListService.CheckContragentAsync(UoW, _organizationId, Counterparty.INN, Counterparty.KPP).Result;
			}
			catch(Exception ex)
			{
				_logger.LogError(
					ex,
					"Ошибка при проверке контрагента {Client} от организации {OrganizationId} в Такском",
					Counterparty.Name,
					Entity.OrganizationId);
				CommonServices.InteractiveService.ShowMessage(ImportanceLevel.Warning,
					"Ошибка при проверке контрагента в Такском");

				return;
			}

			if(contactResult?.Contacts == null)
			{
				CommonServices.InteractiveService.ShowMessage(ImportanceLevel.Warning,
					"Контрагент не найден через Такском");

				return;
			}

			if(contactResult.Contacts.Length == 1)
			{
				var contactListItem = contactResult.Contacts[0];
				Entity.PersonalAccountIdInEdo = contactListItem.EdxClientId;
				Entity.EdoOperator = GetEdoOperatorByEdoAccountId(contactListItem.EdxClientId);

				if(contactListItem.State != null)
				{
					Entity.ConsentForEdoStatus = _contactListService.ConvertStateToConsentForEdoStatus(contactListItem.State.Code);
				}

				TryRefreshEdoLightsMatrix();

				CommonServices.InteractiveService.ShowMessage(ImportanceLevel.Info,
					"Оператор получен");

				return;
			}

			foreach(var edoOperator in contactResult.Contacts)
			{
				var isNotExistsCounterpartyEdoAccount = Counterparty.CounterpartyEdoAccounts
					.FirstOrDefault(x => x.PersonalAccountIdInEdo == edoOperator.EdxClientId) == null;

				if(isNotExistsCounterpartyEdoAccount)
				{
					Counterparty.CounterpartyEdoAccounts.Add(new CounterpartyEdoAccount
					{
						PersonalAccountIdInEdo = edoOperator.EdxClientId,
						EdoOperator = GetEdoOperatorByEdoAccountId(edoOperator.EdxClientId),
						Counterparty = Counterparty,
						OrganizationId = _organizationId
					});
				}

				var isNotExistsCounterpartyEdoOperator = Counterparty.CounterpartyEdoOperators
						.FirstOrDefault(x => x.PersonalAccountIdInEdo == edoOperator.EdxClientId) == null;

				if(isNotExistsCounterpartyEdoOperator)
				{
					Counterparty.CounterpartyEdoOperators.Add(new CounterpartyEdoOperator
					{
						PersonalAccountIdInEdo = edoOperator.EdxClientId,
						EdoOperator = GetEdoOperatorByEdoAccountId(edoOperator.EdxClientId),
						Counterparty = Counterparty
					});										
				}
			}

			Entity.EdoOperator = null;
			Entity.PersonalAccountIdInEdo = null;
			UpdateOperators?.Invoke();

			CommonServices.InteractiveService.ShowMessage(ImportanceLevel.Warning,
				"У контрагента найдено несколько операторов, выберите нужный из списка");
		}
		
		private void CheckConsentForEdo()
		{
			if(Entity.ConsentForEdoStatus == ConsentForEdoStatus.Agree)
			{
				CommonServices.InteractiveService.ShowMessage(ImportanceLevel.Info,
					"В статусе \"Принят\" проверка согласия не требуется");
				return;
			}

			if(string.IsNullOrWhiteSpace(Counterparty.INN))
			{
				CommonServices.InteractiveService.ShowMessage(ImportanceLevel.Error,
					"Проверка согласия невозможна, должен быть заполнен ИНН");
				return;
			}
			
			if(Entity.EdoOperator is null || string.IsNullOrWhiteSpace(Entity.PersonalAccountIdInEdo))
			{
				CommonServices.InteractiveService.ShowMessage(ImportanceLevel.Info,
					"Проверка согласия невозможна, должны быть заполнены данные оператора и кабинета клиента");
				return;
			}

			var checkDate = DateTime.Now.AddDays(-_edoSettings.EdoCheckPeriodDays);
			ContactListItem contactListItem = null;

			try
			{
				contactListItem = _contactListService
					.GetLastChangeOnDate(UoW, _organizationId, checkDate, Counterparty.INN, Counterparty.KPP)
					.Result;
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Ошибка при проверке согласия на ЭДО у аккаунта {EdoAccount}", Entity.PersonalAccountIdInEdo);
				CommonServices.InteractiveService
					.ShowMessage(ImportanceLevel.Info, $"Ошибка при проверке статуса приглашения.\n{ex.Message}");
				return;
			}

			if(contactListItem == null)
			{
				Entity.ConsentForEdoStatus = ConsentForEdoStatus.Unknown;
				CommonServices.InteractiveService.ShowMessage(ImportanceLevel.Info, "Приглашение не найдено.");
				return;
			}

			Entity.ConsentForEdoStatus = _contactListService.ConvertStateToConsentForEdoStatus(contactListItem.State.Code);
			TryRefreshEdoLightsMatrix();
			CommonServices.InteractiveService.ShowMessage(ImportanceLevel.Info, "Согласие проверено.");
		}
		
		private void SendInviteByTaxcom()
		{
			SendContact();
		}

		private void SendManualInviteByTaxcom()
		{
			SendContact(true);
		}

		private void SendContact(bool isManual = false)
		{
			var email = 
				Counterparty.Emails.LastOrDefault(em => em.EmailType?.EmailPurpose == EmailPurpose.ForBills)
				?? Counterparty.Emails.LastOrDefault(em => em.EmailType?.EmailPurpose == EmailPurpose.Work)
				?? Counterparty.Emails.LastOrDefault();

			ResultDto resultMessage;

			if(email == null)
			{
				CommonServices.InteractiveService.ShowMessage(ImportanceLevel.Warning,
					"Не удалось отправить приглашение. Заполните Email у контрагента");

				return;
			}

			if(!_validator.Validate(Entity.Counterparty, _counterpartyValidationContext))
			{
				return;
			}

			UoW.Save(Entity.Counterparty);
			UoW.Save(Entity);
			UoW.Commit();

			try
			{
				var organization = UoW.GetById<Organization>(Entity.OrganizationId ?? _organizationSettings.VodovozOrganizationId);
				
				if(isManual)
				{
					if(!CommonServices.InteractiveService.Question("Время обработки заявки без кода личного кабинета может составлять до 10 дней.\nПродолжить отправку?"))
					{
						return;
					}

					var document = UoW.GetById<Attachment>(_edoSettings.TaxcomManualInvitationFileId);

					resultMessage = _contactListService
						.SendContactsForManualInvitationAsync(
							UoW,
							_organizationId,
							Counterparty.INN,
							Counterparty.KPP,
							organization.Name,
							Entity.EdoOperator.Code,
							email.Address,
							document.FileName,
							document.ByteFile)
						.Result;
				}
				else
				{
					resultMessage = _contactListService
						.SendContactsAsync(
							UoW,
							_organizationId,
							Counterparty.INN,
							Counterparty.KPP,
							email.Address,
							Entity.PersonalAccountIdInEdo,
							organization.Name)
						.Result;
				}
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Ошибка при отправке приглашения на аккаунт {EdoAccount}", Entity.PersonalAccountIdInEdo);
				CommonServices.InteractiveService.ShowMessage(ImportanceLevel.Info, $"Ошибка при отправке приглашения.\n{ex.Message}");
				return;
			}

			if(resultMessage.IsSuccess)
			{
				Entity.ConsentForEdoStatus = ConsentForEdoStatus.Sent;
				CommonServices.InteractiveService.ShowMessage(ImportanceLevel.Info,
					"Приглашение отправлено.");
			}
			else
			{
				CommonServices.InteractiveService.ShowMessage(ImportanceLevel.Error, resultMessage.ErrorMessage);
			}
		}

		private void RemoveEdoAccount()
		{
			if(Counterparty.CounterpartyEdoAccounts.Count == 1)
			{
				CommonServices.InteractiveService.ShowMessage(
					ImportanceLevel.Info,
					"Нельзя удалить единственный аккаунт");
				return;
			}
			
			if(Counterparty.CounterpartyEdoAccounts.FirstOrDefault(x => x == Entity) is null)
			{
				return;
			}
			
			Counterparty.CounterpartyEdoAccounts.Remove(Entity);
			RemovedEdoAccountAction?.Invoke(Entity);
		}
		
		private void CopyFromEdoOperatorWithAccount(CounterpartyEdoAccount edoAccount)
		{
			if(Entity.ConsentForEdoStatus == ConsentForEdoStatus.Sent
				|| Entity.ConsentForEdoStatus == ConsentForEdoStatus.Agree)
			{
				CommonServices.InteractiveService.ShowMessage(
					ImportanceLevel.Info,
					"Нельзя копировать данные аккаунта на аккаунт с отправленным приглашением или с согласием");
				return;
			}

			if(edoAccount.EdoOperator is null && string.IsNullOrWhiteSpace(edoAccount.PersonalAccountIdInEdo))
			{
				CommonServices.InteractiveService.ShowMessage(
					ImportanceLevel.Info,
					"Данные оператора и аккаунта не заполнены в копируемом аккаунте!");
				return;
			}
			
			Entity.EdoOperator = edoAccount.EdoOperator;
			Entity.PersonalAccountIdInEdo = edoAccount.PersonalAccountIdInEdo;
		}
		
		private void TryRefreshEdoLightsMatrix()
		{
			if(Entity.IsDefault)
			{
				RefreshEdoLightsMatrixAction?.Invoke();
			}
		}

		private EdoOperator GetEdoOperatorByEdoAccountId(string id) =>
			UoW.GetAll<EdoOperator>().SingleOrDefault(eo => eo.Code == id.Substring(0, 3));

		public void Dispose()
		{
			Scope = null;
			ParentTab = null;
			Entity.Counterparty.PropertyChanged -= OnCounterpartyPropertyChanged;
		}
	}
}
