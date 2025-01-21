using Gamma.Utilities;
using QS.Banks.Domain;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Extension;
using System;
using System.Collections.Generic;
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
using Vodovoz.Journals.JournalViewModels.Organizations;
using Vodovoz.Presentation.ViewModels.AttachedFiles;
using Vodovoz.ViewModels.Cash;
using Vodovoz.ViewModels.Cash.FinancialCategoriesGroups;
using Vodovoz.ViewModels.Extensions;
using Vodovoz.ViewModels.Journals.JournalViewModels.Employees;
using Vodovoz.ViewModels.Organizations;
using Vodovoz.ViewModels.ViewModels.Employees;
using Vodovoz.ViewModels.ViewModels.Organizations;
using VodovozBusiness.Domain.Cash.CashRequest;

namespace Vodovoz.ViewModels.ViewModels.Cash
{
	public class CashlessRequestViewModel : EntityTabViewModelBase<CashlessRequest>, IAskSaveOnCloseViewModel
	{
		private bool _canChangeFinancialExpenseCategory
			=> CommonServices.CurrentPermissionService.ValidatePresetPermission(Vodovoz.Permissions.Cash.FinancialCategory.CanChangeFinancialExpenseCategory);

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
		private readonly ICashlessRequestFileStorageService _cashlessRequestFileStorageService;
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
		private int _intervals;

		public CashlessRequestViewModel(
			IUserRepository userRepository,
			IEmployeeRepository employeeRepository,
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			INavigationManager navigation,
			ICurrentPermissionService currentPermissionService,
			ICashlessRequestFileStorageService cashlessRequestFileStorageService,
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
			_cashlessRequestFileStorageService = cashlessRequestFileStorageService
				?? throw new ArgumentNullException(nameof(cashlessRequestFileStorageService));
			_attachedFileInformationsViewModelFactory = attachedFileInformationsViewModelFactory
				?? throw new ArgumentNullException(nameof(attachedFileInformationsViewModelFactory));
			_interactiveService = commonServices?.InteractiveService
				?? throw new ArgumentNullException(nameof(commonServices.InteractiveService));

			TabName = base.TabName;

			_currentEmployee = employeeRepository
				.GetEmployeeForCurrentUser(UoW);

			CanCreateGiveOutSchedule = currentPermissionService.ValidatePresetPermission(Vodovoz.Permissions.Cash.CashlessRequest.CanCreateGiveOutSchedule);

			if(Entity.Id == 0)
			{
				Entity.Author = _currentEmployee;
				Entity.Subdivision = _currentEmployee.Subdivision;
				Entity.FinancialResponsibilityCenterId = Entity.Subdivision?.FinancialResponsibilityCenterId;
				Entity.Date = DateTime.Now;
				Entity.PayoutRequestState = PayoutRequestState.New;
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

			SupplierBankAccountViewModel = supplierBankAccountViewModelEEVMBuilder
				.SetUnitOfWork(UoW)
				.SetViewModel(this)
				.ForProperty(this, x => x.SupplierBankAccount)
				.UseViewModelJournalAndAutocompleter<AccountJournalViewModel, AccountJournalFilterViewModel>(filter =>
				{
					filter.RestrictCounterpartyId = Entity.Counterparty?.Id;
				})
				.Finish();

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

			FinancialExpenseCategoryViewModel.IsEditable = false;

			SubdivisionViewModel = subdivisionViewModelEEVMBuilder
				.SetUnitOfWork(UoW)
				.SetViewModel(this)
				.ForProperty(Entity, x => x.Subdivision)
				.UseViewModelDialog<SubdivisionViewModel>()
				.UseViewModelJournalAndAutocompleter<SubdivisionsJournalViewModel>()
				.Finish();

			SubdivisionViewModel.IsEditable = false;

			AttachedFileInformationsViewModel = attachedFileInformationsViewModelFactory.Create(
				UoW,
				CashlessRequestComment.AddFileInformation,
				CashlessRequestComment.DeleteFileInformation);

			AttachedFileInformationsViewModel.ReadOnly = !IsNotClosed || IsSecurityServiceRole;

			#endregion Inner ViewModels configurations

			SetPropertyChangeRelation(
				e => e.ExpenseCategoryId,
				() => FinancialExpenseCategory);

			ConfigureEntityChangingRelations();

			Entity.PropertyChanged += OnCashlessRequestPropertyChanged;

			AddCommentCommand = new DelegateCommand(AddCommentHandler, () => CanAddComment);
			AddCommentCommand.CanExecuteChangedWith(this, x => x.CanAddComment);

			OpenFileCommand = new DelegateCommand<CashlessRequestCommentFileInformation>(OpenFile);
		}

		private void OnCashlessRequestPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if(string.IsNullOrEmpty(Entity.PaymentPurpose)
				&& (e.PropertyName == nameof(CashlessRequest.BillNumber)
					|| e.PropertyName == nameof(CashlessRequest.BillDate)
					|| e.PropertyName == nameof(CashlessRequest.Sum)
					|| e.PropertyName == nameof(CashlessRequest.VatType))
				&& !string.IsNullOrEmpty(Entity.BillNumber)
				&& Entity.BillDate != null
				&& Entity.Sum != 0m)
			{
				Entity.PaymentPurpose = $"Оплата по счету № {Entity.BillNumber} от {Entity.BillDate:dd.MM.yyyy}.Сумма {Entity.Sum:# ###.##}.{Entity.VatType.GetEnumTitle()}";
			}
		}

		public IEnumerable<PayoutRequestUserRole> UserRoles { get; }

		#region Inner ViewModels

		public IEntityEntryViewModel AuthorViewModel { get; }
		public IEntityEntryViewModel SubdivisionViewModel { get; }
		public IEntityEntryViewModel FinancialResponsibilityCenterViewModel { get; }
		public IEntityEntryViewModel FinancialExpenseCategoryViewModel { get; }
		public IEntityEntryViewModel CounterpartyViewModel { get; set; }
		public IEntityEntryViewModel OrganizationViewModel { get; }
		public IEntityEntryViewModel OurOrganizationBankAccountViewModel { get; }
		public IEntityEntryViewModel SupplierBankAccountViewModel { get; }

		public AttachedFileInformationsViewModel AttachedFileInformationsViewModel { get; private set; }

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

		public bool CanEditPaymentDatePlanned =>
			Entity.PayoutRequestState == PayoutRequestState.New
			|| Entity.PayoutRequestState == PayoutRequestState.Submited;

		public bool CanAddComment => !string.IsNullOrWhiteSpace(NewCommentText);

		public bool CanCreateGiveOutSchedule { get; }

		public string NewCommentText { get; private set; }

		public Dictionary<Func<int>, Dictionary<string, byte[]>> FilesToUploadOnSave { get; }
			= new Dictionary<Func<int>, Dictionary<string, byte[]>>();

		public CashlessRequestComment CashlessRequestComment { get; set; } = new CashlessRequestComment();

		#region Календарь платежей

		public bool CreateGiveOutSchedule
		{
			get => _createGiveOutSchedule;
			set => SetField(ref _createGiveOutSchedule, value);
		}

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

		public int Intervals
		{
			get => _intervals;
			set => SetField(ref _intervals, value);
		}

		#endregion Календарь платежей

		#region Commands

		public DelegateCommand AddCommentCommand { get; }
		public DelegateCommand<CashlessRequestCommentFileInformation> OpenFileCommand { get; }

		#endregion Commands

		#region Настройки кнопок смены состояний

		public bool CanPayout =>
			Entity.PayoutRequestState == PayoutRequestState.GivenForTake
			&& UserRole == PayoutRequestUserRole.Accountant;

		public bool CanAccept =>
			Entity.PayoutRequestState == PayoutRequestState.New
			|| Entity.PayoutRequestState == PayoutRequestState.OnClarification;

		public bool CanApprove =>
			Entity.PayoutRequestState == PayoutRequestState.Submited
			&& UserRole == PayoutRequestUserRole.Coordinator;

		public bool CanCancel =>
			Entity.PayoutRequestState == PayoutRequestState.Submited
			|| Entity.PayoutRequestState == PayoutRequestState.OnClarification
			|| UserRole == PayoutRequestUserRole.Coordinator
			&& (Entity.PayoutRequestState == PayoutRequestState.Agreed
				|| Entity.PayoutRequestState == PayoutRequestState.GivenForTake);

		public bool CanReapprove =>
			Entity.PayoutRequestState == PayoutRequestState.Agreed
			|| Entity.PayoutRequestState == PayoutRequestState.GivenForTake
			|| Entity.PayoutRequestState == PayoutRequestState.Canceled;

		public bool CanConveyForPayout =>
			Entity.PayoutRequestState == PayoutRequestState.Agreed
			&& UserRole == PayoutRequestUserRole.Financier;

		#endregion Настройки кнопок смены состояний

		#region Настройки остальных виджетов

		public bool CanEdit { get; private set; }

		public bool IsRoleChooserSensitive { get; }

		public decimal SumGiven => Entity.Payments.Sum(p => p.PaymentItems.Sum(pi => pi.Sum));
		public decimal SumRemaining => Entity.Sum - SumGiven;

		public bool IsNotClosed => Entity.PayoutRequestState != PayoutRequestState.Closed;
		public bool IsNotNew => Entity.PayoutRequestState != PayoutRequestState.New;

		public bool CanSeeNotToReconcile =>
			Entity.PayoutRequestState == PayoutRequestState.Submited
			&& UserRole == PayoutRequestUserRole.Coordinator;

		public bool CanSeeOrganisation => UserRole == PayoutRequestUserRole.Financier;

		public bool CanSetOrganisaton =>
			Entity.PayoutRequestState == PayoutRequestState.New
			|| Entity.PayoutRequestState == PayoutRequestState.Agreed
			|| Entity.PayoutRequestState == PayoutRequestState.GivenForTake;

		public bool CanSeeExpenseCategory => true;

		public bool CanSetExpenseCategory =>
			_expenseCategoriesForAll.Contains(Entity.PayoutRequestState)
			|| (_expenseCategoriesWithSpecialPermission.Contains(Entity.PayoutRequestState)
				&& _canChangeFinancialExpenseCategory);

		public bool CanSetCancelReason =>
			UserRole == PayoutRequestUserRole.Coordinator
			&& IsNotClosed;

		public PayoutRequestUserRole UserRole
		{
			get => _userRole;
			set
			{
				SetField(ref _userRole, value);
				OnPropertyChanged(nameof(CanPayout));
				OnPropertyChanged(nameof(CanAccept));
				OnPropertyChanged(nameof(CanApprove));
				OnPropertyChanged(nameof(CanCancel));
				OnPropertyChanged(nameof(CanReapprove));
				OnPropertyChanged(nameof(CanConveyForPayout));
				OnPropertyChanged(nameof(CanSeeNotToReconcile));
				OnPropertyChanged(nameof(CanSeeOrganisation));
				OnPropertyChanged(nameof(CanSeeExpenseCategory));
				OnPropertyChanged(nameof(CanSetCancelReason));
			}
		}

		public bool IsSecurityServiceRole => UserRole == PayoutRequestUserRole.SecurityService;

		#region IAskSaveOnCloseViewModel

		public bool AskSaveOnClose => !IsSecurityServiceRole;

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

			CashlessRequestComment = new CashlessRequestComment();

			AttachedFileInformationsViewModel.ClearPersistentInformationCommand.Execute();
			AttachedFileInformationsViewModel = _attachedFileInformationsViewModelFactory.CreateAndInitialize<CashlessRequestComment, CashlessRequestCommentFileInformation>(
				UoW,
				CashlessRequestComment,
				_cashlessRequestCommentFileStorageService,
				_cancellationTokenSource.Token,
				CashlessRequestComment.AddFileInformation,
				CashlessRequestComment.DeleteFileInformation);

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

		#endregion Command Handlers

		#region StateChanges

		public void Approve() => ValidateAndSave(PayoutRequestState.Agreed);

		public void Accept() => ValidateAndSave(PayoutRequestState.Submited);

		public void Cancel() => ValidateAndSave(PayoutRequestState.Canceled);

		public void Reapprove() => ValidateAndSave(PayoutRequestState.OnClarification);

		public void ConveyForPayout() => ValidateAndSave(PayoutRequestState.GivenForTake);

		public void Payout() => ValidateAndSave(PayoutRequestState.Closed);

		#endregion StateChanges

		public override bool Save(bool close)
		{
			if(!base.Save(false))
			{
				return false;
			}

			AddAttachedFilesIfNeeded();
			UpdateAttachedFilesIfNeeded();
			DeleteAttachedFilesIfNeeded();
			AttachedFileInformationsViewModel.ClearPersistentInformationCommand.Execute();

			return base.Save(close);
		}

		#region Private методы

		private void ConfigureEntityChangingRelations()
		{
			SetPropertyChangeRelation(e => e.PayoutRequestState, () => CanAccept);
			SetPropertyChangeRelation(e => e.PayoutRequestState, () => CanApprove);
			SetPropertyChangeRelation(e => e.PayoutRequestState, () => CanCancel);
			SetPropertyChangeRelation(e => e.PayoutRequestState, () => CanConveyForPayout);
			SetPropertyChangeRelation(e => e.PayoutRequestState, () => CanReapprove);
			SetPropertyChangeRelation(e => e.PayoutRequestState, () => CanPayout);
			SetPropertyChangeRelation(e => e.PayoutRequestState, () => CanSetExpenseCategory);
			SetPropertyChangeRelation(e => e.PayoutRequestState, () => CanSetOrganisaton);
			SetPropertyChangeRelation(e => e.PayoutRequestState, () => CanSeeNotToReconcile);
			SetPropertyChangeRelation(e => e.PayoutRequestState, () => IsNotNew);
			SetPropertyChangeRelation(e => e.PayoutRequestState, () => IsNotClosed);
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

			foreach(var permissionToRole in Vodovoz.Permissions.Cash.CashlessRequest.PresetPermissionsRoles.PermissionsToRoles)
			{
				if(CommonServices.PermissionService
					.ValidateUserPresetPermission(permissionToRole.Key, userId))
				{
					roles.Add(permissionToRole.Value);
				}
			}

			return roles;
		}

		private void AddAttachedFilesIfNeeded()
		{
			var errors = new Dictionary<string, string>();
			var repeat = false;

			if(!AttachedFileInformationsViewModel.FilesToAddOnSave.Any())
			{
				return;
			}

			do
			{
				foreach(var fileName in AttachedFileInformationsViewModel.FilesToAddOnSave)
				{
					var result = _cashlessRequestFileStorageService.CreateFileAsync(Entity, fileName,
					new MemoryStream(AttachedFileInformationsViewModel.AttachedFiles[fileName]), _cancellationTokenSource.Token)
						.GetAwaiter()
						.GetResult();

					if(result.IsFailure && !result.Errors.All(x => x.Code == Application.Errors.S3.FileAlreadyExists.ToString()))
					{
						errors.Add(fileName, string.Join(", ", result.Errors.Select(e => e.Message)));
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

		private void UpdateAttachedFilesIfNeeded()
		{
			if(!AttachedFileInformationsViewModel.FilesToUpdateOnSave.Any())
			{
				return;
			}

			foreach(var fileName in AttachedFileInformationsViewModel.FilesToUpdateOnSave)
			{
				_cashlessRequestFileStorageService.UpdateFileAsync(Entity, fileName, new MemoryStream(AttachedFileInformationsViewModel.AttachedFiles[fileName]), _cancellationTokenSource.Token)
					.GetAwaiter()
					.GetResult();
			}
		}

		private void DeleteAttachedFilesIfNeeded()
		{
			if(!AttachedFileInformationsViewModel.FilesToDeleteOnSave.Any())
			{
				return;
			}

			foreach(var fileName in AttachedFileInformationsViewModel.FilesToDeleteOnSave)
			{
				_cashlessRequestFileStorageService.DeleteFileAsync(Entity, fileName, _cancellationTokenSource.Token)
					.GetAwaiter()
					.GetResult();
			}
		}

		#endregion

		private void ValidateAndSave(PayoutRequestState nextState)
		{
			if(ValidateForNextState(nextState))
			{
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
	}
}
