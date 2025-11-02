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
using Vodovoz.Tools;
using Vodovoz.ViewModels.Journals.FilterViewModels.Employees;
using Vodovoz.ViewModels.Journals.JournalViewModels.Employees;
using Vodovoz.ViewModels.ViewModels.Employees;

namespace Vodovoz.ViewModels.ViewModels.Cash
{
	public partial class CashRequestItemViewModel : TabViewModelBase, ISingleUoWDialog
	{
		private CashRequestSumItem _entity;
		private Employee _accountableEmployee;
		private DateTime _date;
		private decimal _sum;
		private string _comment;
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

			var employeeEntryViewModelBuilder = new CommonEEVMBuilderFactory<CashRequestItemViewModel>(this, this, UoW, NavigationManager, _scope);

			EmployeeViewModel = employeeEntryViewModelBuilder
				.ForProperty(x => x.AccountableEmployee)
				.UseViewModelDialog<EmployeeViewModel>()
				.UseViewModelJournalAndAutocompleter<EmployeesJournalViewModel, EmployeeFilterViewModel>(
					filter =>
					{
					})
				.Finish();

			AcceptCommand = new DelegateCommand(() => {
				Entity.Date = Date;
				Entity.AccountableEmployee = _accountableEmployee;
				Entity.Sum = Sum;
				Entity.Comment = Comment;

				var validationResult = Entity.RaiseValidationAndGetResult();

				if(!string.IsNullOrWhiteSpace(validationResult))
				{
					_interactiveService.ShowMessage(
						ImportanceLevel.Warning,
						$"{validationResult}");

					return;
				}

				Close(true, CloseSource.Self);
				EntityAccepted?.Invoke(this, new CashRequestSumItemAcceptedEventArgs(Entity));
			},
			() => true);

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
				Date = value.Date;
				Sum = value.Sum;
				Comment = value.Comment;
			}
		}

		public Employee AccountableEmployee
		{
			get => _accountableEmployee;
			set => SetField(ref _accountableEmployee, value);
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

		public DelegateCommand AcceptCommand { get; }

		public DelegateCommand CancelCommand { get; }
	}
}
