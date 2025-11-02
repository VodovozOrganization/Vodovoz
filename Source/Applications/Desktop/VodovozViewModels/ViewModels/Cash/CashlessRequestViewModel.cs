using MoreLinq;
using QS.Banks.Domain;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Services;
using QS.ViewModels;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Extension;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Vodovoz.Application.FileStorage;
using Vodovoz.Core.Domain.Cash;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Cash.FinancialCategoriesGroups;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Organizations;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.Filters.ViewModels;
using Vodovoz.Journals.JournalViewModels.Organizations;
using Vodovoz.Presentation.ViewModels.AttachedFiles;
using Vodovoz.ViewModels.Cash;
using Vodovoz.ViewModels.Cash.FinancialCategoriesGroups;
using Vodovoz.ViewModels.Cash.Payments;
using Vodovoz.ViewModels.Extensions;
using Vodovoz.ViewModels.Journals.JournalNodes.Payments;
using Vodovoz.ViewModels.Journals.JournalViewModels.Employees;
using Vodovoz.ViewModels.Journals.JournalViewModels.Payments;
using Vodovoz.ViewModels.Organizations;
using Vodovoz.ViewModels.ViewModels.Employees;
using Vodovoz.ViewModels.ViewModels.Organizations;
using VodovozBusiness.Domain.Cash.CashRequest;
using VodovozBusiness.Domain.Payments;

namespace Vodovoz.ViewModels.ViewModels.Cash
{
	public class CashlessRequestViewModel : EntityTabViewModelBase<CashlessRequest>, IAskSaveOnCloseViewModel
	{
		private readonly bool _canCreateGiveOutSchedulePermissionGranted;

		private bool _canChangeFinancialExpenseCategory
			=> CommonServices.CurrentPermissionService.ValidatePresetPermission(Vodovoz.Core.Domain.Permissions.CashPermissions.FinancialCategory.CanChangeFinancialExpenseCategory);

		private PayoutRequestState[] _expenseCategoriesForAll => new[]
		{
			PayoutRequestState.New,
			PayoutRequestState.OnClarification,
			PayoutRequestState.Submited
		};

		private PayoutRequestState[] _expenseCategoriesWithSpecialPermission => new[]
		{
			PayoutRequestState.Agreed,
			PayoutRequestState.GivenForTake,
			PayoutRequestState.PartiallyClosed
		};

		private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

		private readonly IUserRepository _userRepository;
		private readonly ICashlessRequestCommentFileStorageService _cashlessRequestCommentFileStorageService;
		private readonly IAttachedFileInformationsViewModelFactory _attachedFileInformationsViewModelFactory;
		private readonly IInteractiveService _interactiveService;

		private PayoutRequestUserRole _userRole;
		private readonly Employee _currentEmployee;
		private FinancialExpenseCategory _financialExpenseCategory;
		private FinancialResponsibilityCenter _financialResponsibilityCenter;
		private Account _ourOrganizationBankAccount;
		private Account _supplierBankAccount;

		private bool _createGiveOutSchedule = false;
		private RepeatIntervalTypes _repeatIntervalType;
		private int _repeatsCount;
		private int _daysBetween;
		private string _selectedVatValue;
		private string _newCommentText;
		private AttachedFileInformationsViewModel _attachedFileInformationsViewModel;
		private bool _canEdit;
		private OutgoingPayment _selectedOutgoingPayment;

		public CashlessRequestViewModel(
			IUserRepository userRepository,
			IEmployeeRepository employeeRepository,
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			INavigationManager navigation,
			ICurrentPermissionService currentPermissionService,
			ICashlessRequestCommentFileStorageService cashlessRequestCommentFileStorageService,
			IAttachedFileInformationsViewModelFactory attachedFileInformationsViewModelFactory,
			ViewModelEEVMBuilder<Employee> authorViewModelEEVMBuilder,
			ViewModelEEVMBuilder<Subdivision> subdivisionViewModelEEVMBuilder,
			ViewModelEEVMBuilder<FinancialResponsibilityCenter> financialResponsibilityCenterViewModelEEVMBuilder,
			ViewModelEEVMBuilder<Organization> ourOrganizationViewModelEEVMBuilder,
			ViewModelEEVMBuilder<Account> ourOrganizationBankAccountViewModelEEVMBuilder,
			ViewModelEEVMBuilder<Account> supplierBankAccountViewModelEEVMBuilder,
			ViewModelEEVMBuilder<FinancialExpenseCategory> financialExpenseCategoryViewModelViewModelEEVMBuilder)
			: base(uowBuilder, unitOfWorkFactory, commonServices, navigation)
		{
			#region Services NullChecks

			if(employeeRepository is null)
			{
				throw new ArgumentNullException(nameof(employeeRepository));
			}

			if(currentPermissionService is null)
			{
				throw new ArgumentNullException(nameof(currentPermissionService));
			}

			if(authorViewModelEEVMBuilder is null)
			{
				throw new ArgumentNullException(nameof(authorViewModelEEVMBuilder));
			}

			if(subdivisionViewModelEEVMBuilder is null)
			{
				throw new ArgumentNullException(nameof(subdivisionViewModelEEVMBuilder));
			}

			if(financialResponsibilityCenterViewModelEEVMBuilder is null)
			{
				throw new ArgumentNullException(nameof(financialResponsibilityCenterViewModelEEVMBuilder));
			}

			if(ourOrganizationViewModelEEVMBuilder is null)
			{
				throw new ArgumentNullException(nameof(ourOrganizationViewModelEEVMBuilder));
			}

			if(ourOrganizationBankAccountViewModelEEVMBuilder is null)
			{
				throw new ArgumentNullException(nameof(ourOrganizationBankAccountViewModelEEVMBuilder));
			}

			if(supplierBankAccountViewModelEEVMBuilder is null)
			{
				throw new ArgumentNullException(nameof(supplierBankAccountViewModelEEVMBuilder));
			}

			if(financialExpenseCategoryViewModelViewModelEEVMBuilder is null)
			{
				throw new ArgumentNullException(nameof(financialExpenseCategoryViewModelViewModelEEVMBuilder));
			}

			#endregion Services NullChecks

			_userRepository = userRepository
				?? throw new ArgumentNullException(nameof(userRepository));
			_cashlessRequestCommentFileStorageService = cashlessRequestCommentFileStorageService
				?? throw new ArgumentNullException(nameof(cashlessRequestCommentFileStorageService));
			_attachedFileInformationsViewModelFactory = attachedFileInformationsViewModelFactory
				?? throw new ArgumentNullException(nameof(attachedFileInformationsViewModelFactory));
			_interactiveService = commonServices?.InteractiveService
				?? throw new ArgumentNullException(nameof(commonServices.InteractiveService));

			TabName = base.TabName;

			_currentEmployee = employeeRepository
				.GetEmployeeForCurrentUser(UoW);

			if(Entity.Id == 0)
			{
				Entity.Author = _currentEmployee;
				Entity.Subdivision = _currentEmployee.Subdivision;
				Entity.FinancialResponsibilityCenterId = Entity.Subdivision?.FinancialResponsibilityCenterId;
				Entity.Date = DateTime.Now;
			}

			UserRoles = GetUserRoles(CurrentUser.Id);

			if(!UserRoles.Any())
			{
				Dispose();
				throw new AbortCreatingPageException($"Пользователь не подходит ни под одну из разрешённых для заявок ролей и не является автором заявки", "Невозможно открыть");
			}

			IsRoleChooserSensitive = UserRoles.Count() > 1;
			UserRole = UserRoles.First();

			#region Inner ViewModels configurations

			AuthorViewModel = authorViewModelEEVMBuilder.SetUnitOfWork(UoW)
				.SetViewModel(this)
				.ForProperty(Entity, x => x.Author)
				.UseViewModelJournalAndAutocompleter<EmployeesJournalViewModel>()
				.UseViewModelDialog<EmployeeViewModel>()
				.Finish();

			AuthorViewModel.IsEditable = false;

			FinancialResponsibilityCenterViewModel = financialResponsibilityCenterViewModelEEVMBuilder
				.SetUnitOfWork(UoW)
				.SetViewModel(this)
				.ForProperty(this, x => x.FinancialResponsibilityCenter)
				.UseViewModelJournalAndAutocompleter<FinancialResponsibilityCenterJournalViewModel>()
				.UseViewModelDialog<FinancialResponsibilityCenterViewModel>()
				.Finish();

			OrganizationViewModel = ourOrganizationViewModelEEVMBuilder
				.SetUnitOfWork(UoW)
				.SetViewModel(this)
				.ForProperty(Entity, x => x.Organization)
				.UseViewModelJournalAndAutocompleter<OrganizationJournalViewModel>()
				.UseViewModelDialog<OrganizationViewModel>()
				.Finish();

			OurOrganizationBankAccountViewModel = ourOrganizationBankAccountViewModelEEVMBuilder
				.SetUnitOfWork(UoW)
				.SetViewModel(this)
				.ForProperty(this, x => x.OurOrganizationBankAccount)
				.UseViewModelJournalAndAutocompleter<AccountJournalViewModel, AccountJournalFilterViewModel>(filter =>
				{
					filter.RestrictOrganizationId = Entity.Organization?.Id;
				})
				.Finish();

			OurOrganizationBankAccountViewModel.IsEditable =
				CanEdit
				&& Entity.Organization != null;

			SupplierBankAccountViewModel = supplierBankAccountViewModelEEVMBuilder
				.SetUnitOfWork(UoW)
				.SetViewModel(this)
				.ForProperty(this, x => x.SupplierBankAccount)
				.UseViewModelJournalAndAutocompleter<AccountJournalViewModel, AccountJournalFilterViewModel>(filter =>
				{
					filter.RestrictCounterpartyId = Entity.Counterparty?.Id;
				})
				.Finish();

			SupplierBankAccountViewModel.IsEditable =
				CanEdit
				&& Entity.Counterparty != null;

			var expenseCategoryViewModel = financialExpenseCategoryViewModelViewModelEEVMBuilder
				.SetUnitOfWork(UoW)
				.SetViewModel(this)
				.ForProperty(this, x => x.FinancialExpenseCategory)
				.UseViewModelDialog<FinancialExpenseCategoryViewModel>()
				.UseViewModelJournalAndAutocompleter<FinancialCategoriesGroupsJournalViewModel, FinancialCategoriesJournalFilterViewModel>(
					filter =>
					{
						filter.RestrictFinancialSubtype = FinancialSubType.Expense;
						filter.RestrictNodeSelectTypes.Add(typeof(FinancialExpenseCategory));
					})
				.Finish();

			expenseCategoryViewModel.CanViewEntity = CanSetExpenseCategory;

			FinancialExpenseCategoryViewModel = expenseCategoryViewModel;

			SubdivisionViewModel = subdivisionViewModelEEVMBuilder
				.SetUnitOfWork(UoW)
				.SetViewModel(this)
				.ForProperty(Entity, x => x.Subdivision)
				.UseViewModelDialog<SubdivisionViewModel>()
				.UseViewModelJournalAndAutocompleter<SubdivisionsJournalViewModel>()
				.Finish();

			AttachedFileInformationsViewModel = attachedFileInformationsViewModelFactory.Create(
				UoW,
				CashlessRequestComment.AddFileInformation,
				CashlessRequestComment.DeleteFileInformation,
				CashlessRequestComment.AttachedFileInformations);

			#endregion Inner ViewModels configurations

			ConfigureEntityChangingRelations();

			Entity.PropertyChanged += OnCashlessRequestPropertyChanged;

			#region Commands

			SaveCommand = new DelegateCommand(() => Save(true), () => !IsSecurityServiceRole);
			SaveCommand.CanExecuteChangedWith(this, x => x.IsSecurityServiceRole);

			CloseTabCommand = new DelegateCommand(() => Close(AskSaveOnClose, CloseSource.Cancel));

			AddCommentCommand = new DelegateCommand(AddCommentHandler, () => CanAddComment);
			AddCommentCommand.CanExecuteChangedWith(this, x => x.CanAddComment);

			OpenFileCommand = new DelegateCommand<CashlessRequestCommentFileInformation>(OpenFile);

			PayoutCommand = new DelegateCommand(Payout, () => CanPayout && !IsSecurityServiceRole);
			PayoutCommand.CanExecuteChangedWith(this, x => x.CanPayout, x => x.IsSecurityServiceRole);

			AcceptCommand = new DelegateCommand(Accept, () => CanAccept && !IsSecurityServiceRole);
			AcceptCommand.CanExecuteChangedWith(this, x => x.CanAccept, x => x.IsSecurityServiceRole);

			ApproveCommand = new DelegateCommand(Approve, () => CanApprove && !IsSecurityServiceRole);
			ApproveCommand.CanExecuteChangedWith(this, x => x.CanApprove, x => x.IsSecurityServiceRole);

			SendToWaitingForAgreedByExecutiveDirectorCommand = new DelegateCommand(SendToWaitingForAgreedByExecutiveDirector, () => CanSendToWaitingForAgreedByExecutiveDirector);
			SendToWaitingForAgreedByExecutiveDirectorCommand.CanExecuteChangedWith(this, x => x.CanSendToWaitingForAgreedByExecutiveDirector);

			CancelRequestCommand = new DelegateCommand(CancelRequest, () => CanCancel && !IsSecurityServiceRole);
			CancelRequestCommand.CanExecuteChangedWith(this, x => x.CanCancel, x => x.IsSecurityServiceRole);

			SendToClarificationCommand = new DelegateCommand(SendToClarification, () => CanSendToClarification && !IsSecurityServiceRole);
			SendToClarificationCommand.CanExecuteChangedWith(this, x => x.CanSendToClarification, x => x.IsSecurityServiceRole);

			ConveyForPayoutCommand = new DelegateCommand(ConveyForPayout, () => CanConveyForPayout && !IsSecurityServiceRole);
			ConveyForPayoutCommand.CanExecuteChangedWith(this, x => x.CanConveyForPayout, x => x.IsSecurityServiceRole);

			CreateCalendarCommand = new DelegateCommand(CreateCalendar, () => CanCreateCalendar);
			CreateCalendarCommand.CanExecuteChangedWith(this, x => x.CanCreateCalendar);

			OpenOutgoingPaymentsCommand = new DelegateCommand(OpenOutgoingPayment, () => true);

			AddOutgoingPaymentCommand = new DelegateCommand(AddOutgoingPayment, () => CanEdit);
			AddOutgoingPaymentCommand.CanExecuteChangedWith(this, x => x.CanEdit);

			RemoveOutgoingPaymentCommand = new DelegateCommand(RemoveOutgoingPayment, () => CanRemoveOutgoingPayment);
			RemoveOutgoingPaymentCommand.CanExecuteChangedWith(this, x => x.CanRemoveOutgoingPayment);

			#endregion Commands

			SelectedVatValue =
				VatValues.Values.Contains(Entity.VatValue)
				? VatValues.First(x => x.Value == Entity.VatValue).Key
				: VatValues.Keys.Last();

			_canCreateGiveOutSchedulePermissionGranted = currentPermissionService.ValidatePresetPermission(Vodovoz.Core.Domain.Permissions.CashPermissions.CashlessRequest.CanCreateGiveOutSchedule);

			Entity.OutgoingPayments.CollectionChanged += OnOutgoingPaymentsChanged;

			UpdateCanEdit();

			PrefetchCommentsAuthorsTitles();
		}

		private void OnOutgoingPaymentsChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			OnPropertyChanged(nameof(CanPayout));
		}

		private void OpenOutgoingPayment()
		{
			NavigationManager.OpenViewModel<OutgoingPaymentEditViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForOpen(SelectedOutgoingPayment.Id));
		}

		private void OnCashlessRequestPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(CashlessRequest.PayoutRequestState))
			{
				UpdateCanEdit();
			}

			if(e.PropertyName == nameof(CashlessRequest.Organization))
			{
				OurOrganizationBankAccountViewModel.IsEditable =
					CanEdit
					&& Entity.Organization != null;

				if(OurOrganizationBankAccount != null
					&& (Entity.Organization is null
						|| !Entity.Organization.Accounts
							.Select(x => x.Id)
							.Contains(OurOrganizationBankAccount.Id)))
				{
					OurOrganizationBankAccount = null;
				}
			}

			if(e.PropertyName == nameof(CashlessRequest.Counterparty))
			{
				SupplierBankAccountViewModel.IsEditable =
					CanEdit
					&& Entity.Counterparty != null;

				if(SupplierBankAccount != null
					&& (Entity.Counterparty is null
						|| !Entity.Counterparty.Accounts
							.Select(x => x.Id)
							.Contains(SupplierBankAccount.Id)))
				{
					SupplierBankAccount = null;
				}
			}
		}

		public IEnumerable<PayoutRequestUserRole> UserRoles { get; }

		[PropertyChangedAlso(nameof(SelectedOutgoingPayment))]
		[PropertyChangedAlso(nameof(CanRemoveOutgoingPayment))]
		public object SelectedOutgoingPaymentObject
		{
			get => SelectedOutgoingPayment;
			set
			{
				if(value is OutgoingPayment outgoingPayment)
				{
					SelectedOutgoingPayment = outgoingPayment;
				}
				else
				{
					SelectedOutgoingPayment = null;
				}
			}
		}

		[PropertyChangedAlso(nameof(CanRemoveOutgoingPayment))]
		public OutgoingPayment SelectedOutgoingPayment
		{
			get => _selectedOutgoingPayment;
			set => SetField(ref _selectedOutgoingPayment, value);
		}

		#region Inner ViewModels

		public IEntityEntryViewModel AuthorViewModel { get; }
		public IEntityEntryViewModel SubdivisionViewModel { get; }
		public IEntityEntryViewModel FinancialResponsibilityCenterViewModel { get; }
		public IEntityEntryViewModel FinancialExpenseCategoryViewModel { get; }
		public IEntityEntryViewModel CounterpartyViewModel { get; set; }
		public IEntityEntryViewModel OrganizationViewModel { get; }
		public IEntityEntryViewModel OurOrganizationBankAccountViewModel { get; }
		public IEntityEntryViewModel SupplierBankAccountViewModel { get; }

		public AttachedFileInformationsViewModel AttachedFileInformationsViewModel
		{
			get => _attachedFileInformationsViewModel;
			private set => SetField(ref _attachedFileInformationsViewModel, value);
		}

		#endregion Inner ViewModels

		#region Passthrough Properties

		/// <summary>
		/// Статья расхода
		/// </summary>
		public FinancialExpenseCategory FinancialExpenseCategory
		{
			get => this.GetIdRefField(ref _financialExpenseCategory, Entity.ExpenseCategoryId);
			set => this.SetIdRefField(SetField, ref _financialExpenseCategory, () => Entity.ExpenseCategoryId, value);
		}

		/// <summary>
		/// Статья расхода
		/// </summary>
		public FinancialResponsibilityCenter FinancialResponsibilityCenter
		{
			get => this.GetIdRefField(ref _financialResponsibilityCenter, Entity.FinancialResponsibilityCenterId);
			set => this.SetIdRefField(SetField, ref _financialResponsibilityCenter, () => Entity.FinancialResponsibilityCenterId, value);
		}

		/// <summary>
		/// Статья расхода
		/// </summary>
		public Account OurOrganizationBankAccount
		{
			get => this.GetIdRefField(ref _ourOrganizationBankAccount, Entity.OurOrganizationBankAccountId);
			set => this.SetIdRefField(SetField, ref _ourOrganizationBankAccount, () => Entity.OurOrganizationBankAccountId, value);
		}

		/// <summary>
		/// Расчетный счет поставщика
		/// </summary>
		public Account SupplierBankAccount
		{
			get => this.GetIdRefField(ref _supplierBankAccount, Entity.SupplierBankAccountId);
			set => this.SetIdRefField(SetField, ref _supplierBankAccount, () => Entity.SupplierBankAccountId, value);
		}

		#endregion Passthrough Properties

		public bool CanAddComment =>
			CanEdit
			&& !string.IsNullOrWhiteSpace(NewCommentText);

		public bool CanRemoveOutgoingPayment =>
			CanEdit
			&& SelectedOutgoingPayment != null;

		public bool CanCreateCalendar => CanCreateGiveOutSchedule && !IsSecurityServiceRole;

		public string VatString => Entity.VatValue == 0 ? "Без НДС" : $"{Entity.VatValue}% НДС";

		[PropertyChangedAlso(nameof(CanCreateCalendar))]
		public bool CanCreateGiveOutSchedule { get; private set; }

		[PropertyChangedAlso(nameof(CanAddComment))]
		public string NewCommentText
		{
			get => _newCommentText;
			set => SetField(ref _newCommentText, value);
		}

		public Dictionary<Func<int>, Dictionary<string, byte[]>> FilesToUploadOnSave { get; }
			= new Dictionary<Func<int>, Dictionary<string, byte[]>>();

		public CashlessRequestComment CashlessRequestComment { get; set; } = new CashlessRequestComment();

		#region Календарь платежей

		[PropertyChangedAlso(nameof(ShowDaysBetween))]
		public bool CreateGiveOutSchedule
		{
			get => _createGiveOutSchedule;
			set => SetField(ref _createGiveOutSchedule, value);
		}

		[PropertyChangedAlso(nameof(ShowDaysBetween))]
		public RepeatIntervalTypes RepeatIntervalType
		{
			get => _repeatIntervalType;
			set => SetField(ref _repeatIntervalType, value);
		}

		public int RepeatsCount
		{
			get => _repeatsCount;
			set => SetField(ref _repeatsCount, value);
		}

		public int DaysBetween
		{
			get => _daysBetween;
			set => SetField(ref _daysBetween, value);
		}

		public bool ShowDaysBetween =>
			CreateGiveOutSchedule
			&& RepeatIntervalType == RepeatIntervalTypes.NDays;

		private void CreateCalendar()
		{
			ValidateAndSaveCalendar(PayoutRequestState.Submited);
		}

		#endregion Календарь платежей

		#region Commands

		public DelegateCommand SaveCommand { get; }
		public DelegateCommand CloseTabCommand { get; }

		public DelegateCommand AddCommentCommand { get; }
		public DelegateCommand<CashlessRequestCommentFileInformation> OpenFileCommand { get; }

		public DelegateCommand PayoutCommand { get; }
		public DelegateCommand AcceptCommand { get; }
		public DelegateCommand ApproveCommand { get; }
		public DelegateCommand SendToWaitingForAgreedByExecutiveDirectorCommand { get; }
		public DelegateCommand CancelRequestCommand { get; }
		public DelegateCommand SendToClarificationCommand { get; }
		public DelegateCommand ConveyForPayoutCommand { get; }

		public DelegateCommand CreateCalendarCommand { get; }

		public DelegateCommand AddOutgoingPaymentCommand { get; }
		public DelegateCommand RemoveOutgoingPaymentCommand { get; }
		public DelegateCommand OpenOutgoingPaymentsCommand { get; }

		#endregion Commands

		#region Настройки кнопок смены состояний

		/// <summary>
		/// Можно ли изменить дату платежа
		/// </summary>
		public bool CanEditPaymentDatePlanned =>
			CanEdit
			&& (Entity.PayoutRequestState == PayoutRequestState.New
			|| Entity.PayoutRequestState == PayoutRequestState.Submited);

		/// <summary>
		/// Можно ли принять заявку <see cref="PayoutRequestState.Submited"/>
		/// </summary>
		public bool CanAccept =>
			CanEdit
			&& (Entity.PayoutRequestState == PayoutRequestState.New
			|| Entity.PayoutRequestState == PayoutRequestState.OnClarification
			|| Entity.PayoutRequestState == PayoutRequestState.Canceled);

		/// <summary>
		/// Можно ли согласовать заявку
		/// <see cref="PayoutRequestState.AgreedBySubdivisionChief"/>
		/// <see cref="PayoutRequestState.AgreedByFinancialResponsibilityCenter"/>
		/// </summary>
		public bool CanApprove =>
			CanEdit
			&& (Entity.PayoutRequestState == PayoutRequestState.Submited
				|| (Entity.PayoutRequestState == PayoutRequestState.AgreedBySubdivisionChief
					&& !FinancialResponsibilityCenter.RequestApprovalDenied));

		/// <summary>
		/// Можно ли отправить на согласование исполнительному директору
		/// <see cref="PayoutRequestState.WaitingForAgreedByExecutiveDirector"/>
		/// </summary>
		public bool CanSendToWaitingForAgreedByExecutiveDirector =>
			CanEdit
			&& Entity.PayoutRequestState == PayoutRequestState.AgreedByFinancialResponsibilityCenter;

		/// <summary>
		/// Можно ли отменить заявку
		/// <see cref="PayoutRequestState.Canceled"/>
		/// </summary>
		public bool CanCancel =>
			CanEdit
			&& (Entity.PayoutRequestState == PayoutRequestState.New
			|| Entity.PayoutRequestState == PayoutRequestState.Submited
			|| Entity.PayoutRequestState == PayoutRequestState.AgreedBySubdivisionChief
			|| Entity.PayoutRequestState == PayoutRequestState.AgreedByFinancialResponsibilityCenter
			|| Entity.PayoutRequestState == PayoutRequestState.WaitingForAgreedByExecutiveDirector
			|| Entity.PayoutRequestState == PayoutRequestState.GivenForTake);

		/// <summary>
		/// Можно ли отправить на уточнение
		/// <see cref="PayoutRequestState.OnClarification"/>
		/// </summary>
		public bool CanSendToClarification =>
			CanEdit
			&& (Entity.PayoutRequestState == PayoutRequestState.Submited
			|| Entity.PayoutRequestState == PayoutRequestState.AgreedBySubdivisionChief
			|| Entity.PayoutRequestState == PayoutRequestState.AgreedByFinancialResponsibilityCenter
			|| Entity.PayoutRequestState == PayoutRequestState.WaitingForAgreedByExecutiveDirector);

		/// <summary>
		/// Можно ли передать на выдачу
		/// <see cref="PayoutRequestState.GivenForTake"/>
		/// </summary>
		public bool CanConveyForPayout =>
			CanEdit
			&& Entity.PayoutRequestState == PayoutRequestState.WaitingForAgreedByExecutiveDirector;

		/// <summary>
		/// Можно ли выдать
		/// <see cref="PayoutRequestState.PartiallyClosed"/>
		/// <see cref="PayoutRequestState.Closed"/>
		/// </summary>
		public bool CanPayout =>
			CanEdit
			&& (Entity.PayoutRequestState == PayoutRequestState.GivenForTake
				|| Entity.PayoutRequestState == PayoutRequestState.PartiallyClosed)
			&& UserRole == PayoutRequestUserRole.Cashier
			&& Entity.OutgoingPayments.Sum(x => x.Sum) == Entity.Sum;

		#endregion Настройки кнопок смены состояний

		#region Настройки остальных виджетов

		[PropertyChangedAlso(
			nameof(CanChangeFinancialResponsibilityCenter),
			nameof(CanPayout),
			nameof(CanAccept),
			nameof(CanApprove),
			nameof(CanCancel),
			nameof(CanSendToClarification),
			nameof(CanConveyForPayout),
			nameof(CanSeeNotToReconcile),
			nameof(CanSeeExpenseCategory),
			nameof(IsSecurityServiceRole),
			nameof(CanSendToWaitingForAgreedByExecutiveDirector),
			nameof(CanEditPlainProperties),
			nameof(CanRemoveOutgoingPayment))]
		public bool CanEdit
		{
			get => _canEdit;
			private set => SetField(ref _canEdit, value);
		}

		public bool CanEditPlainProperties =>
			CanEdit
			&& CashlessRequest.AllowedToChangePlainPropertiesStates.Contains(Entity.PayoutRequestState);

		public bool IsRoleChooserSensitive { get; }

		public decimal SumGiven => Entity.OutgoingPayments.Sum(p => p.Sum);

		public decimal SumRemaining => Entity.Sum - SumGiven;

		public bool CanSeeNotToReconcile =>
			Entity.PayoutRequestState == PayoutRequestState.Submited
			&& UserRole == PayoutRequestUserRole.Coordinator;

		public bool CanSeeExpenseCategory => true;

		public bool CanSetExpenseCategory =>
			CanEditPlainProperties
			&& (_expenseCategoriesForAll.Contains(Entity.PayoutRequestState)
				|| (_expenseCategoriesWithSpecialPermission.Contains(Entity.PayoutRequestState)
					&& _canChangeFinancialExpenseCategory));

		[PropertyChangedAlso(nameof(CanCreateCalendar))]
		[PropertyChangedAlso(nameof(IsSecurityServiceRole))]
		public PayoutRequestUserRole UserRole
		{
			get => _userRole;
			set
			{
				SetField(ref _userRole, value);
				UpdateCanEdit();
			}
		}

		public bool IsSecurityServiceRole => UserRole == PayoutRequestUserRole.SecurityService;

		[PropertyChangedAlso(nameof(ShowCustomVat))]
		public string SelectedVatValue
		{
			get => _selectedVatValue;
			set
			{
				if(SetField(ref _selectedVatValue, value)
					&& value != VatValues.Keys.Last())
				{
					Entity.VatValue = VatValues[value];
				}
			}
		}

		public bool ShowCustomVat => SelectedVatValue == VatValues.Keys.Last();

		public Dictionary<string, decimal> VatValues => new Dictionary<string, decimal>
		{
			{ "Без НДС", 0m },
			{ "НДС 5%", 5m },
			{ "НДС 10%", 10m },
			{ "НДС 12%", 12m },
			{ "НДС 20%", 20m },
			{ "Другой НДС", 0m }
		};

		#region IAskSaveOnCloseViewModel

		public bool AskSaveOnClose => !IsSecurityServiceRole;

		public Dictionary<int, string> CachedAuthorsCommentTitles { get; set; }
			= new Dictionary<int, string>();

		public bool CanChangeFinancialResponsibilityCenter => CanEdit
			&& CashlessRequest.AllowedToChangeFinancialResponsibilityCenterIdStates
				.Contains(Entity.PayoutRequestState);

		#endregion IAskSaveOnCloseViewModel

		#endregion Настройки остальных виджетов

		#region Command Handlers

		/// <summary>
		/// Добавление комментария
		/// </summary>
		private void AddCommentHandler()
		{
			if(_currentEmployee == null)
			{
				CommonServices.InteractiveService.ShowMessage(ImportanceLevel.Warning, "Невозможно добавить комментарий так как к вашему пользователю не привязан сотрудник");
				return;
			}

			CashlessRequestComment.AuthorId = _currentEmployee.Id;
			CashlessRequestComment.CreatedAt = DateTime.Now;
			CashlessRequestComment.Text = NewCommentText;

			Entity.AddComment(CashlessRequestComment);

			NewCommentText = string.Empty;

			var newComment = CashlessRequestComment;
			FilesToUploadOnSave.Add(() => newComment.Id, AttachedFileInformationsViewModel.AttachedFiles.ToDictionary(kv => kv.Key, kv => kv.Value));

			AttachedFileInformationsViewModel.ClearPersistentInformationCommand.Execute();

			CashlessRequestComment = new CashlessRequestComment();

			AttachedFileInformationsViewModel = _attachedFileInformationsViewModelFactory.Create(
				UoW,
				CashlessRequestComment.AddFileInformation,
				CashlessRequestComment.DeleteFileInformation,
				CashlessRequestComment.AttachedFileInformations);

			AttachedFileInformationsViewModel.ReadOnly = !CanEdit;
		}

		/// <summary>
		/// Открытие файла
		/// </summary>
		/// <param name="cashlessRequestCommentFileInformation">Информация о файле</param>
		public void OpenFile(CashlessRequestCommentFileInformation cashlessRequestCommentFileInformation)
		{
			byte[] blob;

			if(cashlessRequestCommentFileInformation.CashlessRequestCommentId == 0)
			{
				blob = FilesToUploadOnSave.FirstOrDefault(actionCommentIdToFile => actionCommentIdToFile.Value.ContainsKey(cashlessRequestCommentFileInformation.FileName))
					.Value[cashlessRequestCommentFileInformation.FileName];
			}
			else
			{
				var comment = Entity.Comments.FirstOrDefault(cdc => cdc.Id == cashlessRequestCommentFileInformation.CashlessRequestCommentId);

				var fileResult = _cashlessRequestCommentFileStorageService.GetFileAsync(comment, cashlessRequestCommentFileInformation.FileName, _cancellationTokenSource.Token)
					.GetAwaiter()
					.GetResult();

				if(fileResult.IsFailure)
				{
					return;
				}

				using(var ms = new MemoryStream())
				{
					fileResult.Value.CopyTo(ms);

					blob = ms.ToArray();
				}
			}

			var vodovozUserTempDirectory = _userRepository.GetTempDirForCurrentUser(UoW);

			if(string.IsNullOrWhiteSpace(vodovozUserTempDirectory))
			{
				return;
			}

			var tempFilePath = Path.Combine(Path.GetTempPath(), vodovozUserTempDirectory, cashlessRequestCommentFileInformation.FileName);

			if(!File.Exists(tempFilePath))
			{
				File.WriteAllBytes(tempFilePath, blob);
			}

			var process = new Process
			{
				EnableRaisingEvents = true
			};

			process.StartInfo.FileName = Path.Combine(vodovozUserTempDirectory, cashlessRequestCommentFileInformation.FileName);

			process.Exited += OnProcessExited;
			process.Start();
		}

		private void RemoveOutgoingPayment()
		{
			Entity.RemoveOutgoingPayment(SelectedOutgoingPayment.Id);

			OnPropertyChanged(nameof(SumGiven));
			OnPropertyChanged(nameof(SumRemaining));
		}

		private void AddOutgoingPayment()
		{
			NavigationManager.OpenViewModel<PaymentsJournalViewModel, Action<PaymentsJournalFilterViewModel>>(
				this,
				filter =>
				{
					filter.RestrictDocumentType = typeof(OutgoingPayment);
					filter.OutgoingPaymentsWithoutCashlessRequestAssigned = true;
				},
				OpenPageOptions.AsSlave,
				viewModel =>
				{
					viewModel.SelectionMode = JournalSelectionMode.Single;
					viewModel.OnEntitySelectedResult += OnOutgoingPaymentToAddSelected;
				});
		}

		private void OnOutgoingPaymentToAddSelected(object sender, JournalSelectedNodesEventArgs e)
		{
			var outgoingPaymentsIds = e.SelectedNodes
				.Cast<PaymentJournalNode>()
				.Select(x => x.Id)
				.ToArray();

			var outgoignPaymentsToAdd = UoW.Session
				.Query<OutgoingPayment>()
				.Where(x => outgoingPaymentsIds.Contains(x.Id))
				.ToArray();

			var usedOutgoingPayments = outgoignPaymentsToAdd.Where(x => x.CashlessRequestId != null);

			if(usedOutgoingPayments.Any())
			{
				_interactiveService.ShowMessage(
					ImportanceLevel.Warning,
					"Платежи:\n" +
					$"{string.Join("", usedOutgoingPayments.Select(x => $"[{x.Id}]: №{x.PaymentNumber} - {x.Sum}\n"))}\n" +
					$"Уже привязаны к другим заявкам и не будут добавлены");
			}

			Entity.AddOutgoingPayments(outgoignPaymentsToAdd.Where(x => x.CashlessRequestId == null));

			OnPropertyChanged(nameof(SumGiven));
			OnPropertyChanged(nameof(SumRemaining));
		}

		#endregion Command Handlers

		#region StateChanges

		private void Approve()
		{
			if(Entity.PayoutRequestState == PayoutRequestState.Submited)
			{
				ValidateAndSave(PayoutRequestState.AgreedBySubdivisionChief);
			}
			else if(Entity.PayoutRequestState == PayoutRequestState.AgreedBySubdivisionChief)
			{
				ValidateAndSave(PayoutRequestState.AgreedByFinancialResponsibilityCenter);
			}
			else if(Entity.PayoutRequestState == PayoutRequestState.WaitingForAgreedByExecutiveDirector)
			{
				ValidateAndSave(PayoutRequestState.Agreed);
			}
		}

		private void SendToWaitingForAgreedByExecutiveDirector() => ValidateAndSave(PayoutRequestState.WaitingForAgreedByExecutiveDirector);

		private void Accept() => ValidateAndSave(PayoutRequestState.Submited);

		private void CancelRequest() => ValidateAndSave(PayoutRequestState.Canceled);

		private void SendToClarification() => ValidateAndSave(PayoutRequestState.OnClarification);

		private void ConveyForPayout() => ValidateAndSave(PayoutRequestState.GivenForTake);

		private void Payout() => ValidateAndSave(PayoutRequestState.Closed);

		#endregion StateChanges

		private void UpdateCanEdit()
		{
			if(IsSecurityServiceRole)
			{
				CanEdit = false;
				UpdateCanCreateGiveOutSchedule();
				return;
			}

			var hasPermissionsToEdit =
				Entity.Id == 0 && PermissionResult.CanCreate
				|| PermissionResult.CanUpdate;

			if(!hasPermissionsToEdit)
			{
				CanEdit = false;
				UpdateCanCreateGiveOutSchedule();
				return;
			}

			switch(Entity.PayoutRequestState)
			{
				case PayoutRequestState.New:
					CanEdit = UserRole == PayoutRequestUserRole.RequestCreator
						&& _currentEmployee == Entity.Author;
					break;
				case PayoutRequestState.Submited:
					CanEdit = _currentEmployee == Entity.Subdivision.Chief;
					break;
				case PayoutRequestState.AgreedBySubdivisionChief:
					CanEdit = _currentEmployee.Id == FinancialResponsibilityCenter.ResponsibleEmployeeId
						|| _currentEmployee.Id == FinancialResponsibilityCenter.ViceResponsibleEmployeeId;
					break;
				case PayoutRequestState.AgreedByFinancialResponsibilityCenter:
					CanEdit = UserRole == PayoutRequestUserRole.Financier;
					break;
				case PayoutRequestState.WaitingForAgreedByExecutiveDirector:
					CanEdit = UserRole == PayoutRequestUserRole.Coordinator;
					break;
				case PayoutRequestState.GivenForTake:
				case PayoutRequestState.PartiallyClosed:
					CanEdit = UserRole == PayoutRequestUserRole.Cashier;
					break;
				case PayoutRequestState.OnClarification:
					CanEdit = (UserRole == PayoutRequestUserRole.RequestCreator
							&& _currentEmployee == Entity.Author)
						|| UserRole == PayoutRequestUserRole.SubdivisionChief
						|| (_currentEmployee.Id == FinancialResponsibilityCenter.ResponsibleEmployeeId
							|| _currentEmployee.Id == FinancialResponsibilityCenter.ViceResponsibleEmployeeId)
						|| UserRole == PayoutRequestUserRole.SecurityService;
					break;
				case PayoutRequestState.Canceled:
					CanEdit = UserRole == PayoutRequestUserRole.RequestCreator
						&& _currentEmployee == Entity.Author;
					break;
				case PayoutRequestState.Closed:
					CanEdit = false;
					break;
			}

			UpdateCanCreateGiveOutSchedule();
		}

		private void UpdateCanCreateGiveOutSchedule()
		{
			CanCreateGiveOutSchedule = CanEdit
				&& Entity.PayoutRequestState == PayoutRequestState.New
				&& _canCreateGiveOutSchedulePermissionGranted;
		}

		public override bool Save(bool close)
		{
			if(Entity.Id == 0 && string.IsNullOrWhiteSpace(Entity.PaymentPurpose))
			{
				Entity.PaymentPurpose = $"Оплата по счету № {Entity.BillNumber} от {Entity.BillDate:dd.MM.yyyy}. Сумма {Entity.Sum:# ###.##} руб., в том числе {VatString} - {Entity.Sum / 100 * VatValues[SelectedVatValue]:# ###.##} руб.";
			}

			if(!base.Save(false))
			{
				return false;
			}

			AddCommentFilesIfNeeded();
			AttachedFileInformationsViewModel.ClearPersistentInformationCommand.Execute();

			return base.Save(close);
		}

		private void PrefetchCommentsAuthorsTitles()
		{
			var authorsIds = Entity.Comments
				.Select(c => c.AuthorId)
				.Concat(new[] { _currentEmployee.Id })
				.Distinct()
				.ToArray();

			var results =
				(from employee in UoW.Session.Query<Employee>()
				 join subdivision in UoW.Session.Query<Subdivision>()
				 on employee.Subdivision.Id equals subdivision.Id
				 into employeeSubdivision
				 from subdivision in employeeSubdivision.DefaultIfEmpty()
				 where authorsIds.Contains(employee.Id)
				 select new
				 {
					 employee.Id,
					 Title = $"{employee.GetPersonNameWithInitials()}{(subdivision != null && !string.IsNullOrWhiteSpace(subdivision.ShortName) ? $"\n{subdivision.ShortName}" : "")}"
				 })
				.ToArray();

			foreach(var result in results)
			{
				CachedAuthorsCommentTitles.Add(result.Id, result.Title);
			}
		}

		#region Private методы

		private void ConfigureEntityChangingRelations()
		{
			SetPropertyChangeRelation(e => e.PayoutRequestState,
				() => CanAccept,
				() => CanApprove,
				() => CanCancel,
				() => CanConveyForPayout,
				() => CanSendToClarification,
				() => CanPayout,
				() => CanSetExpenseCategory,
				() => CanSendToWaitingForAgreedByExecutiveDirector,
				() => CanSeeNotToReconcile,
				() => CanEditPaymentDatePlanned);

			SetPropertyChangeRelation(e => e.VatValue,
				() => VatString,
				() => SelectedVatValue);

			SetPropertyChangeRelation(e => e.ExpenseCategoryId,
				() => FinancialExpenseCategory);

			SetPropertyChangeRelation(e => e.PayoutRequestState,
				() => CanChangeFinancialResponsibilityCenter);

			SetPropertyChangeRelation(e => e.Sum,
				() => SumGiven,
				() => SumRemaining,
				() => CanPayout);
		}

		private bool ValidateForNextState(PayoutRequestState nextState)
		{
			ValidationContext.Items.Add("next_state", nextState);

			var valid = Validate();

			ValidationContext.Items.Remove("next_state");

			if(valid)
			{
				Entity.ChangeState(nextState);
			}

			return valid;
		}

		private IEnumerable<PayoutRequestUserRole> GetUserRoles(int userId)
		{
			var roles = new List<PayoutRequestUserRole>();

			if(Entity.Author == null || Entity.Author.Id == _currentEmployee.Id)
			{
				roles.Add(PayoutRequestUserRole.RequestCreator);
			}

			foreach(var permissionToRole in Vodovoz.Core.Domain.Permissions.CashPermissions.CashlessRequest.PermissionsToRoles)
			{
				if(CommonServices.PermissionService
					.ValidateUserPresetPermission(permissionToRole.Key, userId))
				{
					roles.Add(permissionToRole.Value);
				}
			}

			return roles;
		}

		private void AddCommentFilesIfNeeded()
		{
			var errors = new Dictionary<string, string>();
			var repeat = false;

			do
			{
				foreach(var keyValuePair in FilesToUploadOnSave)
				{
					var commentId = keyValuePair.Key.Invoke();

					var comment = Entity.Comments
						?.FirstOrDefault(c => c.Id == commentId);

					foreach(var fileToUploadPair in keyValuePair.Value)
					{
						using(var ms = new MemoryStream(fileToUploadPair.Value))
						{
							var result = _cashlessRequestCommentFileStorageService
								.CreateFileAsync(
									comment,
									fileToUploadPair.Key,
									ms,
									_cancellationTokenSource.Token)
								.GetAwaiter()
								.GetResult();

							if(result.IsFailure && !result.Errors
								.All(x => x.Code == Application.Errors.S3.FileAlreadyExists.ToString()))
							{
								errors.Add(
									fileToUploadPair.Key,
									string.Join(
										", ",
										result.Errors.Select(e => e.Message)));
							}
						}
					}
				}

				if(errors.Any())
				{
					repeat = _interactiveService.Question(
						"Не удалось загрузить файлы:\n" +
						string.Join("\n- ", errors.Select(fekv => $"{fekv.Key} - {fekv.Value}")) + "\n" +
						"\n" +
						"Повторить попытку?",
						"Ошибка загрузки файлов");

					errors.Clear();
				}
				else
				{
					repeat = false;
				}
			}
			while(repeat);
		}

		#endregion

		private void ValidateAndSave(PayoutRequestState nextState)
		{
			if(ValidateForNextState(nextState))
			{
				Save(true);
			}
		}

		private void ValidateAndSaveCalendar(PayoutRequestState nextState)
		{
			if(ValidateForNextState(nextState))
			{
				for(int i = 1; i <= RepeatsCount; i++)
				{
					var newCashlessRequest = Entity.Copy1To11();

					switch(RepeatIntervalType)
					{
						case RepeatIntervalTypes.Day:
							newCashlessRequest.PaymentDatePlanned = Entity.PaymentDatePlanned?.AddDays(RepeatsCount * i);
							break;
						case RepeatIntervalTypes.Week:
							newCashlessRequest.PaymentDatePlanned = Entity.PaymentDatePlanned?.AddDays(RepeatsCount * 7 * i);
							break;
						case RepeatIntervalTypes.Month:
							newCashlessRequest.PaymentDatePlanned = Entity.PaymentDatePlanned?.AddMonths(RepeatsCount * i);
							break;
						case RepeatIntervalTypes.Year:
							newCashlessRequest.PaymentDatePlanned = Entity.PaymentDatePlanned?.AddYears(RepeatsCount * i);
							break;
						case RepeatIntervalTypes.NDays:
							newCashlessRequest.PaymentDatePlanned = Entity.PaymentDatePlanned?.AddDays(RepeatsCount * DaysBetween * i);
							break;
					}

					UoW.Save(newCashlessRequest);
				}

				Save(true);
			}
		}

		private void OnProcessExited(object sender, EventArgs e)
		{
			if(sender is Process process)
			{
				File.Delete(process.StartInfo.FileName);
				process.Exited -= OnProcessExited;
			}
		}

		public override void Dispose()
		{
			Entity.OutgoingPayments.CollectionChanged -= OnOutgoingPaymentsChanged;
			base.Dispose();
		}
	}
}
