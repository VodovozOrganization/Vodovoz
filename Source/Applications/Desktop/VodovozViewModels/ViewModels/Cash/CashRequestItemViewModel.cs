using Autofac;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.ViewModels;
using QS.ViewModels.Control.EEVM;
using System;
using System.Linq;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Employees;
using Vodovoz.FilterViewModels.Organization;
using Vodovoz.Journals.JournalViewModels.Organizations;
using Vodovoz.Tools;
using Vodovoz.ViewModels.Journals.FilterViewModels.Employees;
using Vodovoz.ViewModels.Journals.JournalViewModels.Employees;
using Vodovoz.ViewModels.ViewModels.Employees;
using Vodovoz.ViewModels.ViewModels.Organizations;

namespace Vodovoz.ViewModels.ViewModels.Cash
{
	public partial class CashRequestItemViewModel : TabViewModelBase, ISingleUoWDialog
	{
		private CashRequestSumItem _entity;
		private Employee _accountableEmployee;
		private DateTime _date;
		private decimal _sum;
		private string _comment;
		private Subdivision _cfzSubdivision;
		private readonly IInteractiveService _interactiveService;
		private readonly ILifetimeScope _scope;


		public CashRequestItemViewModel(
			IUnitOfWork uow,
			IInteractiveService interactiveService,
			INavigationManager navigation,
			PayoutRequestUserRole userRole,
			ILifetimeScope scope)
			: base(interactiveService, navigation)
		{
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));

			_scope = scope ?? throw new ArgumentNullException(nameof(scope));
			UoW = uow;
			UserRole = userRole;

			TabName = "Cумма заявки на выдачу Д/С";

			var entryViewModelBuilder = new CommonEEVMBuilderFactory<CashRequestItemViewModel>(this, this, UoW, NavigationManager, _scope);

			EmployeeViewModel = entryViewModelBuilder
				.ForProperty(x => x.AccountableEmployee)
				.UseViewModelDialog<EmployeeViewModel>()
				.UseViewModelJournalAndAutocompleter<EmployeesJournalViewModel, EmployeeFilterViewModel>(
					filter =>
					{
					})
				.Finish();

			CfzSubdivisionViewModel = entryViewModelBuilder
				.ForProperty(x => x.CfzSubdivision)
				.UseViewModelDialog<SubdivisionViewModel>()
				.UseViewModelJournalAndAutocompleter<SubdivisionsJournalViewModel, SubdivisionFilterViewModel>(
					filter =>
					{
					})
				.Finish();

			AcceptCommand = new DelegateCommand(Accept, () => true);

			CancelCommand = new DelegateCommand(() =>
			{
				Close(true, CloseSource.Cancel);
			},
			() => true);
		}

		public PayoutRequestUserRole UserRole { get; set; }

		public IUnitOfWork UoW { get; set; }

		[PropertyChangedAlso(nameof(CanEditOnlyinStateNRC_OrRoleCoordinator))]
		public CashRequestSumItem Entity
		{
			get => _entity;
			set
			{
				SetField(ref _entity, value);
				AccountableEmployee = value.AccountableEmployee;
				CfzSubdivision = value.CfzSubdivision;
				Date = value.Date;
				Sum = value.Sum;
				Comment = value.Comment;
			}
		}

		public Employee AccountableEmployee
		{
			get => _accountableEmployee;
			set
			{
				if(SetField(ref _accountableEmployee, value))
				{
					CfzSubdivision = value?.Subdivision;
				}
			}
		}

		/// <summary>
		/// ЦФЗ - центр финансовых затрат, подразделение, на которое относится затрата
		/// </summary>
		public Subdivision CfzSubdivision
		{
			get => _cfzSubdivision;
			set => SetField(ref _cfzSubdivision, value);
		}

		public DateTime Date {
			get => _date;
			set => SetField(ref _date, value);
		}

		public decimal Sum {
			get => _sum;
			set => SetField(ref _sum, value);
		}

		public string Comment {
			get => _comment;
			set => SetField(ref _comment, value);
		}

		public EventHandler EntityAccepted;

		//Создана - только для невыданных сумм - Заявитель, Согласователь
		//Согласована - Согласователь
		public bool CanEditOnlyinStateNRC_OrRoleCoordinator
		{
			get
			{
				//В новой редактирование всегда разрешено
				if(Entity is null || Entity.Id == 0)
				{
					return true;
				}
				else
				{
					return (
						Entity.CashRequest.PayoutRequestState == PayoutRequestState.New
						&& !Entity.ObservableExpenses.Any()
						&& (UserRole == PayoutRequestUserRole.RequestCreator
							|| UserRole == PayoutRequestUserRole.Coordinator)
						|| (Entity.CashRequest.PayoutRequestState == PayoutRequestState.Agreed
							&& UserRole == PayoutRequestUserRole.Coordinator)
						);
				}
			}
		}

		public IEntityEntryViewModel EmployeeViewModel { get; }

		/// <summary>
		/// Выбор ЦФЗ строки
		/// </summary>
		public IEntityEntryViewModel CfzSubdivisionViewModel { get; }

		/// <summary>
		/// Проверка значений окна и перенос их в строку заявки
		/// </summary>
		public DelegateCommand AcceptCommand { get; }

		public DelegateCommand CancelCommand { get; }

		private void Accept()
		{
			if(!IsValuesValid())
			{
				return;
			}

			Entity.Date = Date;
			Entity.AccountableEmployee = AccountableEmployee;
			Entity.Sum = Sum;
			Entity.Comment = Comment;
			Entity.CfzSubdivision = CfzSubdivision;

			Close(true, CloseSource.Self);
			EntityAccepted?.Invoke(this, new CashRequestSumItemAcceptedEventArgs(Entity));
		}

		private bool IsValuesValid()
		{
			var checkedItem = new CashRequestSumItem
			{
				Date = Date,
				AccountableEmployee = AccountableEmployee,
				Sum = Sum,
				Comment = Comment,
				CfzSubdivision = CfzSubdivision
			};

			var validationResult = checkedItem.RaiseValidationAndGetResult();

			if(!string.IsNullOrWhiteSpace(validationResult))
			{
				_interactiveService.ShowMessage(
					ImportanceLevel.Warning,
					$"{validationResult}");

				return false;
			}

			return true;
		}
	}
}
