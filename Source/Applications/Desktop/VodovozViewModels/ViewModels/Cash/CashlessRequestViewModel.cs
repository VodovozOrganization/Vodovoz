using Autofac;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using QS.Project.Services.FileDialog;
using QS.Services;
using QS.ViewModels;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Extension;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Vodovoz.Application.FileStorage;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Cash.FinancialCategoriesGroups;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Organizations;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.Journals.JournalViewModels.Organizations;
using Vodovoz.Presentation.ViewModels.AttachedFiles;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Cash.FinancialCategoriesGroups;
using Vodovoz.ViewModels.Extensions;
using Vodovoz.ViewModels.ViewModels.Organizations;
using VodovozBusiness.Domain.Cash.CashRequest;

namespace Vodovoz.ViewModels.ViewModels.Cash
{
	public class CashlessRequestViewModel : EntityTabViewModelBase<CashlessRequest>, IAskSaveOnCloseViewModel
	{
		private PayoutRequestUserRole _userRole;
		private readonly Employee _currentEmployee;
		private readonly ICashlessRequestFileStorageService _cashlessRequestFileStorageService;
		private ILifetimeScope _lifetimeScope;
		private IInteractiveService _interactiveService;
		private FinancialExpenseCategory _financialExpenseCategory;
		private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

		public CashlessRequestViewModel(
			IFileDialogService fileDialogService,
			IUserRepository userRepository,
			ICounterpartyJournalFactory counterpartyJournalFactory,
			IEmployeeRepository employeeRepository,
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			INavigationManager navigation,
			ICashlessRequestFileStorageService cashlessRequestFileStorageService,
			IAttachedFileInformationsViewModelFactory attachedFileInformationsViewModelFactory,
			ILifetimeScope lifetimeScope)
			: base(uowBuilder, unitOfWorkFactory, commonServices, navigation)
		{
			if(attachedFileInformationsViewModelFactory is null)
			{
				throw new ArgumentNullException(nameof(attachedFileInformationsViewModelFactory));
			}

			TabName = base.TabName;
			_cashlessRequestFileStorageService = cashlessRequestFileStorageService ?? throw new ArgumentNullException(nameof(cashlessRequestFileStorageService));
			_lifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));
			_interactiveService = commonServices?.InteractiveService ?? throw new ArgumentNullException(nameof(commonServices.InteractiveService));
			CounterpartyAutocompleteSelector =
				(counterpartyJournalFactory ?? throw new ArgumentNullException(nameof(counterpartyJournalFactory)))
				.CreateCounterpartyAutocompleteSelectorFactory(_lifetimeScope);
			_currentEmployee =
				(employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository)))
				.GetEmployeeForCurrentUser(UoW);

			if(Entity.Id == 0)
			{
				Entity.Author = _currentEmployee;
				Entity.Subdivision = _currentEmployee.Subdivision;
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

			OurOrganisations = UoW.Session.QueryOver<Organization>().List();

			var expenseCategoryEntryViewModelBuilder = new CommonEEVMBuilderFactory<CashlessRequestViewModel>(this, this, UoW, NavigationManager, _lifetimeScope);

			var expenseCategoryViewModel = expenseCategoryEntryViewModelBuilder
				.ForProperty(x => x.FinancialExpenseCategory)
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

			SetPropertyChangeRelation(
				e => e.ExpenseCategoryId,
				() => FinancialExpenseCategory);

			ConfigureEntityChangingRelations();

			SubdivisionViewModel = new CommonEEVMBuilderFactory<CashlessRequest>(this, Entity, UoW, NavigationManager, _lifetimeScope)
				.ForProperty(x => x.Subdivision)
				.UseViewModelDialog<SubdivisionViewModel>()
				.UseViewModelJournalAndAutocompleter<SubdivisionsJournalViewModel>()
				.Finish();

			SubdivisionViewModel.IsEditable = false;

			AttachedFileInformationsViewModel = attachedFileInformationsViewModelFactory.CreateAndInitialize<CashlessRequest, CashlessRequestFileInformation>(
				UoW,
				Entity,
				_cashlessRequestFileStorageService,
				_cancellationTokenSource.Token,
				Entity.AddFileInformation,
				Entity.RemoveFileInformation);

			AttachedFileInformationsViewModel.ReadOnly = !IsNotClosed || IsSecurityServiceRole;
		}

		#region Статья расхода
		private bool _hasFinancialExpenseCategoryPermission => CommonServices.CurrentPermissionService.ValidatePresetPermission(Vodovoz.Permissions.Cash.FinancialCategory.CanChangeFinancialExpenseCategory);
		private PayoutRequestState[] _expenseCategoriesForAll => new[] { PayoutRequestState.New, PayoutRequestState.OnClarification, PayoutRequestState.Submited };
		private PayoutRequestState[] _expenseCategoriesWithSpecialPermission => new[] { PayoutRequestState.Agreed, PayoutRequestState.GivenForTake, PayoutRequestState.PartiallyClosed };
		#endregion

		#region Инициализация виджетов

		public IEnumerable<Organization> OurOrganisations { get; }
		public IEntityAutocompleteSelectorFactory CounterpartyAutocompleteSelector { get; }

		public IEnumerable<PayoutRequestUserRole> UserRoles { get; }
		public IEntityEntryViewModel SubdivisionViewModel { get; }
		public IEntityEntryViewModel FinancialExpenseCategoryViewModel { get; }

		public FinancialExpenseCategory FinancialExpenseCategory
		{
			get => this.GetIdRefField(ref _financialExpenseCategory, Entity.ExpenseCategoryId);
			set => this.SetIdRefField(SetField, ref _financialExpenseCategory, () => Entity.ExpenseCategoryId, value);
		}

		#endregion

		#region Настройки кнопок смены состояний

		public bool CanPayout => Entity.PayoutRequestState == PayoutRequestState.GivenForTake
		                         && UserRole == PayoutRequestUserRole.Accountant;

		public bool CanAccept => Entity.PayoutRequestState == PayoutRequestState.New
		                         || Entity.PayoutRequestState == PayoutRequestState.OnClarification;

		public bool CanApprove => Entity.PayoutRequestState == PayoutRequestState.Submited
		                          && UserRole == PayoutRequestUserRole.Coordinator;

		public bool CanCancel => Entity.PayoutRequestState == PayoutRequestState.Submited
		                         || Entity.PayoutRequestState == PayoutRequestState.OnClarification
		                         || UserRole == PayoutRequestUserRole.Coordinator
		                         && (Entity.PayoutRequestState == PayoutRequestState.Agreed
		                             || Entity.PayoutRequestState == PayoutRequestState.GivenForTake);

		public bool CanReapprove => Entity.PayoutRequestState == PayoutRequestState.Agreed ||
		                            Entity.PayoutRequestState == PayoutRequestState.GivenForTake ||
		                            Entity.PayoutRequestState == PayoutRequestState.Canceled;

		public bool CanConveyForPayout => Entity.PayoutRequestState == PayoutRequestState.Agreed
		                                  && UserRole == PayoutRequestUserRole.Financier;

		#endregion

		#region Настройки остальных виджетов

		public bool IsRoleChooserSensitive { get; }
		public bool IsNotClosed => Entity.PayoutRequestState != PayoutRequestState.Closed;
		public bool IsNotNew => Entity.PayoutRequestState != PayoutRequestState.New;

		public bool CanSeeNotToReconcile => Entity.PayoutRequestState == PayoutRequestState.Submited
		                                    && UserRole == PayoutRequestUserRole.Coordinator;

		public bool CanSeeOrganisation => UserRole == PayoutRequestUserRole.Financier;

		public bool CanSetOrganisaton => Entity.PayoutRequestState == PayoutRequestState.New
		                                 || Entity.PayoutRequestState == PayoutRequestState.Agreed
		                                 || Entity.PayoutRequestState == PayoutRequestState.GivenForTake;

		public bool CanSeeExpenseCategory => true;

		public bool CanSetExpenseCategory => _expenseCategoriesForAll.Contains(Entity.PayoutRequestState)
				|| (_expenseCategoriesWithSpecialPermission.Contains(Entity.PayoutRequestState) && _hasFinancialExpenseCategoryPermission);

		public bool CanSetCancelReason => UserRole == PayoutRequestUserRole.Coordinator && IsNotClosed;

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

		public AttachedFileInformationsViewModel AttachedFileInformationsViewModel { get; }

		#endregion

		#endregion

		#region Public методы

		public void Approve()
		{
			if(ValidateForNextState(PayoutRequestState.Agreed))
			{
				Save(true);
			}
		}

		public void Accept()
		{
			if(ValidateForNextState(PayoutRequestState.Submited))
			{
				Save(true);
			}
		}

		public void Cancel()
		{
			if(ValidateForNextState(PayoutRequestState.Canceled))
			{
				Save(true);
			}
		}

		public void Reapprove()
		{
			if(ValidateForNextState(PayoutRequestState.OnClarification))
			{
				Save(true);
			}
		}

		public void ConveyForPayout()
		{
			if(ValidateForNextState(PayoutRequestState.GivenForTake))
			{
				Save(true);
			}
		}

		public void Payout()
		{
			if(ValidateForNextState(PayoutRequestState.Closed))
			{
				Save(true);
			}
		}

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

		#endregion

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
			bool CheckRole(string roleName, int id) =>
				ServicesConfig.CommonServices.PermissionService.ValidateUserPresetPermission(roleName, id);

			var roles = new List<PayoutRequestUserRole>();

			if(Entity.Author == null || Entity.Author.Id == _currentEmployee.Id)
			{
				roles.Add(PayoutRequestUserRole.RequestCreator);
			}

			if(CheckRole("role_financier_cash_request", userId))
			{
				roles.Add(PayoutRequestUserRole.Financier);
			}

			if(CheckRole("role_coordinator_cash_request", userId))
			{
				roles.Add(PayoutRequestUserRole.Coordinator);
			}

			if(CheckRole("role_cashless_payout_accountant", userId))
			{
				roles.Add(PayoutRequestUserRole.Accountant);
			}

			if(CheckRole("role_security_service_cash_request", userId))
			{
				roles.Add(PayoutRequestUserRole.SecurityService);
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

		public override void Dispose()
		{
			_lifetimeScope = null;
			base.Dispose();
		}
	}
}
