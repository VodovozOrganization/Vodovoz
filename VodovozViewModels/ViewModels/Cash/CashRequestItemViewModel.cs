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

namespace Vodovoz.ViewModels.ViewModels.Cash
{
    public class CashRequestItemViewModel: TabViewModelBase, ISingleUoWDialog
    {
        public UserRole UserRole;

        public IUnitOfWork UoW { get; set; }

        private CashRequestSumItem entity;
        public CashRequestSumItem Entity 
        {
            get => entity;
            set {
                SetField(ref entity, value);
                if (value.AccountableEmployee == null)
                {
                    entity.AccountableEmployee = accountableEmployee;
                }
            }
        }
        private Employee accountableEmployee;

        public CashRequestItemViewModel(
            IUnitOfWork uow,
            IInteractiveService interactiveService, 
            INavigationManager navigation,
            UserRole userRole,
            Employee currentEmployee) 
            : base(interactiveService, navigation)
        {
            this.UoW = uow;
            this.UserRole = userRole;
            accountableEmployee = currentEmployee;
        }

        public EventHandler EntityAccepted;

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
                        Entity.CashRequest.State == CashRequest.States.New 
                        && !Entity.ObservableExpenses.Any()
                        && (UserRole == UserRole.RequestCreator
                            || UserRole == UserRole.Coordinator)
                        || (Entity.CashRequest.State == CashRequest.States.Agreed
                            && UserRole == UserRole.Coordinator)
                        );
                }
            }
        }

        #region Commands

        private DelegateCommand acceptCommand;
        public DelegateCommand AcceptCommand => acceptCommand ?? (acceptCommand = new DelegateCommand(
            () => {
                Close(false, CloseSource.Self);
                EntityAccepted?.Invoke(this, new CashRequestSumItemAcceptedEventArgs(Entity));
            },
            () => true
        ));

        private DelegateCommand cancelCommand;
        public DelegateCommand CancelCommand => cancelCommand ?? (cancelCommand = new DelegateCommand(
            () => {
                if(Entity.Id != 0)
                {
                    UoW.Session.Refresh(Entity);
                }
                Close(false, CloseSource.Cancel);
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