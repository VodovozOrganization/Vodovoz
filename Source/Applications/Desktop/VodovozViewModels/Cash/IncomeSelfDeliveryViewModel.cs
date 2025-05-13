using Autofac;
using FluentNHibernate.Data;
using Microsoft.Extensions.Logging;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.Entity.EntityPermissions.EntityExtendedPermission;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Report;
using QS.Services;
using QS.ViewModels;
using QS.ViewModels.Control.EEVM;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Cash.FinancialCategoriesGroups;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Cash;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.PermissionExtensions;
using Vodovoz.Presentation.ViewModels.Documents;
using Vodovoz.Settings.Cash;
using Vodovoz.TempAdapters;
using Vodovoz.Tools.CallTasks;
using Vodovoz.ViewModels.Cash.FinancialCategoriesGroups;
using Vodovoz.ViewModels.Extensions;
using Vodovoz.ViewModels.Journals.FilterViewModels.Employees;
using Vodovoz.ViewModels.Journals.JournalViewModels.Employees;
using Vodovoz.ViewModels.ViewModels.Employees;

namespace Vodovoz.ViewModels.Cash
{
	public class IncomeSelfDeliveryViewModel : EntityTabViewModelBase<Income>
	{
		private readonly ILogger<IncomeViewModel> _logger;
		private readonly IEmployeeRepository _employeeRepository;
		private readonly ILifetimeScope _lifetimeScope;
		private readonly IEntityExtendedPermissionValidator _entityExtendedPermissionValidator;
		private readonly ICallTaskWorker _callTaskWorker;
		private readonly IReportViewOpener _reportViewOpener;
		private readonly ICashRepository _cashRepository;
		private readonly ISelfDeliveryCashOrganisationDistributor _selfDeliveryCashOrganisationDistributor;
		private readonly IFinancialCategoriesGroupsSettings _financialCategoriesGroupsSettings;
		private readonly IReportInfoFactory _reportInfoFactory;
		private readonly IPermissionResult _entityPermissionResult;
		private FinancialIncomeCategory _financialIncomeCategory;
		private IEntityEntryViewModel _orderViewModel;

		public IncomeSelfDeliveryViewModel(
			ILogger<IncomeViewModel> logger,
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			IEntityExtendedPermissionValidator entityExtendedPermissionValidator,
			IUserService userService,
			INavigationManager navigation,
			IEmployeeRepository employeeRepository,
			ILifetimeScope lifetimeScope,
			ICallTaskWorker callTaskWorker,
			ICashRepository cashRepository,
			ISelfDeliveryCashOrganisationDistributor selfDeliveryCashOrganisationDistributor,
			IReportViewOpener reportViewOpener,
			IFinancialCategoriesGroupsSettings financialCategoriesGroupsSettings,
			IFinancialIncomeCategoriesRepository financialIncomeCategoriesRepository,
			IReportInfoFactory reportInfoFactory
			)
			: base(uowBuilder, unitOfWorkFactory, commonServices, navigation)
		{
			if(financialIncomeCategoriesRepository is null)
			{
				throw new ArgumentNullException(nameof(financialIncomeCategoriesRepository));
			}

			TabName = IsNew ? "Приходный кассовый ордер самовывоза" : $"Приходный кассовый ордер самовывоза {Entity.Id}";

			if(userService is null)
			{
				throw new ArgumentNullException(nameof(userService));
			}

			if(navigation is null)
			{
				throw new ArgumentNullException(nameof(navigation));
			}

			_logger = logger
				?? throw new ArgumentNullException(nameof(logger));
			_entityExtendedPermissionValidator = entityExtendedPermissionValidator
				?? throw new ArgumentNullException(nameof(entityExtendedPermissionValidator));
			_employeeRepository = employeeRepository
				?? throw new ArgumentNullException(nameof(employeeRepository));
			_lifetimeScope = lifetimeScope
				?? throw new ArgumentNullException(nameof(lifetimeScope));
			_callTaskWorker = callTaskWorker
				?? throw new ArgumentNullException(nameof(callTaskWorker));
			_cashRepository = cashRepository
				?? throw new ArgumentNullException(nameof(cashRepository));
			_selfDeliveryCashOrganisationDistributor = selfDeliveryCashOrganisationDistributor
				?? throw new ArgumentNullException(nameof(selfDeliveryCashOrganisationDistributor));
			_reportViewOpener = reportViewOpener
				?? throw new ArgumentNullException(nameof(reportViewOpener));
			_financialCategoriesGroupsSettings = financialCategoriesGroupsSettings
				?? throw new ArgumentNullException(nameof(financialCategoriesGroupsSettings));
			_reportInfoFactory = reportInfoFactory ?? throw new ArgumentNullException(nameof(reportInfoFactory));
			_entityPermissionResult = commonServices.CurrentPermissionService.ValidateEntityPermission(typeof(Income));
			CanEditRectroactively =
				_entityExtendedPermissionValidator.Validate(
					typeof(Income), userService.CurrentUserId, nameof(RetroactivelyClosePermission));

			if(IsNew)
			{
				Entity.Casher = _employeeRepository.GetEmployeeForCurrentUser(UoW);

				if(Entity.Casher is null)
				{
					InitializationFailed("Ошибка",
						  "Ваш пользователь не привязан к действующему сотруднику, вы не можете создавать кассовые документы, так как некого указывать в качестве кассира.");
					return;
				}

				if(!CanCreate)
				{
					InitializationFailed("Ошибка",
						 "Отсутствуют права на создание приходного ордера");
					return;
				}

				Entity.TypeDocument = IncomeInvoiceDocumentType.IncomeInvoiceSelfDelivery;
				Entity.TypeOperation = IncomeType.Payment;

				Entity.Date = DateTime.Now;
			}

			CashierViewModel = BuildCashierEntryViewModel();

			FinancialIncomeCategoryViewModel = BuildFinancialIncomeCategoryViewModelEntryViewModel();

			SetPropertyChangeRelation(
				e => e.IncomeCategoryId,
				() => FinancialIncomeCategory);

			SetPropertyChangeRelation(
				e => e.Id,
				() => IsNew);

			SetPropertyChangeRelation(
				e => e.Date,
				() => CanEdit);

			SetPropertyChangeRelation(
				e => e.Money,
				() => Money);

			ValidationContext.Items.Add("IsSelfDelivery", true);
			ValidationContext.ServiceContainer.AddService(typeof(IUnitOfWork), UoW);
			ValidationContext.ServiceContainer.AddService(typeof(IFinancialIncomeCategoriesRepository), financialIncomeCategoriesRepository);

			Entity.PropertyChanged += OnEntityPropertyChanged;

			Entity.IncomeCategoryId = _financialCategoriesGroupsSettings.SelfDeliveryDefaultFinancialIncomeCategoryId;

			PrintCommand = new DelegateCommand(Print);
			SaveCommand = new DelegateCommand(SaveHandler, () => CanEdit);
			CloseCommand = new DelegateCommand(() => Close(true, CloseSource.Self));
		}

		public decimal Money
		{
			get => Entity.Money;
			set => Entity.Money = value;
		}

		public bool IsNew => UoWGeneric.IsNew;
		
		public ILifetimeScope Scope => _lifetimeScope; // убрать при обновлении Counterparty на MVVM

		public bool CanEditTypeOperation = false;

		public bool CanEditRectroactively { get; }

		public bool CanCreate => _entityPermissionResult.CanCreate;

		public bool CanEdit => (IsNew && CanCreate)
			|| (_entityPermissionResult.CanUpdate && Entity.Date.Date == DateTime.Now.Date)
			|| CanEditRectroactively;

		#region Id Ref Propeties

		public FinancialIncomeCategory FinancialIncomeCategory
		{
			get => this.GetIdRefField(ref _financialIncomeCategory, Entity.IncomeCategoryId);
			set => this.SetIdRefField(SetField, ref _financialIncomeCategory, () => Entity.IncomeCategoryId, value);
		}

		#endregion Id Ref Propeties

		#region Commands

		public DelegateCommand SaveCommand { get; }

		public DelegateCommand CloseCommand { get; }

		public DelegateCommand PrintCommand { get; }

		#endregion Commands

		#region EntityEntry ViewModels

		public IEntityEntryViewModel CashierViewModel { get; }

		public IEntityEntryViewModel BuildCashierEntryViewModel()
		{
			var cashierEntryViewModelBuilder = new CommonEEVMBuilderFactory<Income>(this, Entity, UoW, NavigationManager, _lifetimeScope);

			var cashierViewModel = cashierEntryViewModelBuilder
				.ForProperty(x => x.Casher)
				.UseViewModelDialog<EmployeeViewModel>()
				.UseViewModelJournalAndAutocompleter<EmployeesJournalViewModel, EmployeeFilterViewModel>(
					filter =>
					{
						filter.Status = EmployeeStatus.IsWorking;
					})
				.Finish();

			cashierViewModel.IsEditable = false;

			return cashierViewModel;
		}

		public IEntityEntryViewModel OrderViewModel
		{
			get => _orderViewModel;
			set // при обновлении версии языка - заменить на init или при смене countarparty на mvvm - заменить билдер
			{
				if(_orderViewModel is null)
				{
					_orderViewModel = value;
				}
			}
		}

		public IEntityEntryViewModel FinancialIncomeCategoryViewModel { get; }

		public IEntityEntryViewModel BuildFinancialIncomeCategoryViewModelEntryViewModel()
		{
			var financialIncomeCategoryViewModelEntryViewModelBuilder = new CommonEEVMBuilderFactory<IncomeSelfDeliveryViewModel>(this, this, UoW, NavigationManager, _lifetimeScope);

			return financialIncomeCategoryViewModelEntryViewModelBuilder
				.ForProperty(x => x.FinancialIncomeCategory)
				.UseViewModelJournalAndAutocompleter<FinancialCategoriesGroupsJournalViewModel, FinancialCategoriesJournalFilterViewModel>(
					filter =>
					{
						filter.RestrictFinancialSubtype = FinancialSubType.Income;
						filter.RestrictTargetDocument = TargetDocument.SelfDelivery;
						filter.RestrictNodeSelectTypes.Add(typeof(FinancialIncomeCategory));
					})
				.Finish();
		}

		#endregion EntityEntry ViewModels

		public string CurrencySymbol => NumberFormatInfo.CurrentInfo.CurrencySymbol;

		public void InitializationFailed(
			string title = "",
			string message = "")
		{
			if(!string.IsNullOrWhiteSpace(title) && !string.IsNullOrWhiteSpace(message))
			{
				CommonServices.InteractiveService.ShowMessage(ImportanceLevel.Error, message, title);
			}

			FailInitialize = true;
		}

		private void OnEntityPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(Entity.Order)
				&& Entity.Order != null)
			{
				Entity.FillFromOrder(UoW, _cashRepository);
			}
		}

		private void SaveHandler()
		{
			if(!Save(false))
			{
				CommonServices.InteractiveService.ShowMessage(ImportanceLevel.Warning, "Не удалось сохранить документ");
				return;
			}

			var document = Entity.Order.OrderDocuments
				.FirstOrDefault(x =>
					x.Type == OrderDocumentType.Invoice
					|| x.Type == OrderDocumentType.InvoiceBarter
					|| x.Type == OrderDocumentType.InvoiceContractDoc)
				as IPrintableRDLDocument;

			var page = NavigationManager
				.OpenViewModel<PrintableRdlDocumentViewModel<IPrintableRDLDocument>, IPrintableRDLDocument>(this, document, OpenPageOptions.AsSlave);

			page.PageClosed += OnInvoiceDocumentPrintViewClosed;
		}

		private void OnInvoiceDocumentPrintViewClosed(object sender, EventArgs eventArgs)
		{
			if(sender is IPage page)
			{
				page.PageClosed -= OnInvoiceDocumentPrintViewClosed;
			}

			Close(false, CloseSource.Save);
		}

		protected override bool BeforeSave()
		{
			Entity.AcceptSelfDeliveryPaid(_callTaskWorker);

			if(UoW.IsNew)
			{
				_logger.LogInformation("Создаем документ распределения налички по юр лицу...");
				_selfDeliveryCashOrganisationDistributor.DistributeIncomeCash(UoW, Entity.Order, Entity);
			}
			else
			{
				_logger.LogInformation("Меняем документ распределения налички по юр лицу...");
				_selfDeliveryCashOrganisationDistributor.UpdateRecords(
					UoW, Entity.Order, Entity, _employeeRepository.GetEmployeeForCurrentUser(UoW));
			}

			return true;
		}

		public void SetOrderById(int orderId)
		{
			Entity.Order = UoW.GetById<Order>(orderId);
		}

		private void Print()
		{
			if(UoWGeneric.HasChanges
				&& (!AskQuestion("Сохранить изменения перед печатью?") || !Save()))
			{
				return;
			}

			var reportInfo = _reportInfoFactory.Create();
			reportInfo.Title = $"Квитанция №{Entity.Id} от {Entity.Date:d}";
			reportInfo.Identifier = "Cash.ReturnTicket";
			reportInfo.Parameters = new Dictionary<string, object>
			{
				{ "id", Entity.Id }
			};

			_reportViewOpener.OpenReport(this, reportInfo);
		}
	}
}
