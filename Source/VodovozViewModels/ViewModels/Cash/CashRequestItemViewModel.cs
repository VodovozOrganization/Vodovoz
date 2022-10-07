using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Services;
using QS.ViewModels;
using System;
using System.Linq;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Employees;
using Vodovoz.TempAdapters;

namespace Vodovoz.ViewModels.ViewModels.Cash
{
    public class CashRequestItemViewModel: TabViewModelBase, ISingleUoWDialog
    {
        public PayoutRequestUserRole UserRole;

        public IUnitOfWork UoW { get; set; }

        private CashRequestSumItem entity;
        public CashRequestSumItem Entity 
        {
            get => entity;
            set {
                SetField(ref entity, value);
                AccountableEmployee = value.AccountableEmployee;
                Date = value.Date;
                Sum = value.Sum;
                Comment = value.Comment;
            }
        }

        private Employee accountableEmployee;
        public Employee AccountableEmployee
        { 
            get => accountableEmployee; 
            set => SetField(ref accountableEmployee, value);
        }

        private DateTime date;
        public DateTime Date { 
            get => date;
            set => SetField(ref date, value);
        }

        private decimal sum;
        public decimal Sum { 
            get => sum;
            set => SetField(ref sum, value);
        }

        private string comment;
        public string Comment {
            get => comment;
            set => SetField(ref comment, value);
        }

        public CashRequestItemViewModel(
            IUnitOfWork uow,
            IInteractiveService interactiveService, 
            INavigationManager navigation,
            PayoutRequestUserRole userRole,
            IEmployeeJournalFactory employeeJournalFactory) 
            : base(interactiveService, navigation)
        {
	        EmployeeJournalFactory = employeeJournalFactory ?? throw new ArgumentNullException(nameof(employeeJournalFactory));
	        UoW = uow;
            UserRole = userRole;
        }

        public EventHandler EntityAccepted;
        public IEmployeeJournalFactory EmployeeJournalFactory { get; }

        //Создана - только для невыданных сумм - Заявитель, Согласователь
        //Согласована - Согласователь
        public bool CanEditOnlyinStateNRC_OrRoleCoordinator
        {
            get
            {
                //В новой редактирование всегда разрешено
                if (Entity.Id == 0)
                {
                    return true;
                } else {
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

        #region Commands

        private DelegateCommand acceptCommand;
        public DelegateCommand AcceptCommand => acceptCommand ?? (acceptCommand = new DelegateCommand(
            () => {
                Entity.Date = Date;
                Entity.AccountableEmployee = accountableEmployee;
                Entity.Sum = Sum;
                Entity.Comment = Comment;
                Close(true, CloseSource.Self);
                EntityAccepted?.Invoke(this, new CashRequestSumItemAcceptedEventArgs(Entity));
            },
            () => true
        ));

        private DelegateCommand cancelCommand;
        public DelegateCommand CancelCommand => cancelCommand ?? (cancelCommand = new DelegateCommand(
            () => {
                Close(true, CloseSource.Cancel);
            },
            () => true
        ));

        #endregion Commands
    }

    public class CashRequestSumItemAcceptedEventArgs : EventArgs
    {
        public CashRequestSumItemAcceptedEventArgs(CashRequestSumItem cashRequestSumItem)
        {
            AcceptedEntity = cashRequestSumItem;
        }

        public CashRequestSumItem AcceptedEntity { get; private set; }
    }
}
