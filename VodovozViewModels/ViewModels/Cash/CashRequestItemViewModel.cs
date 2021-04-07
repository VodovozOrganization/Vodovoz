using QS.Commands;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using System;
using System.Linq;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Employees;

namespace Vodovoz.ViewModels.ViewModels.Cash
{
    public class CashRequestItemViewModel: EntityTabViewModelBase<CashRequestSumItem>
    {
        public UserRole UserRole;
     
        public CashRequestItemViewModel(
            IEntityUoWBuilder uowBuilder,
            IUnitOfWorkFactory unitOfWorkFactory,
            ICommonServices commonServices,
            UserRole userRole,
            Employee currentEmployee
        ) 
            : base(uowBuilder, unitOfWorkFactory, commonServices) 
        {
            this.UserRole = userRole;
            Entity.AccountableEmployee = currentEmployee;
        }

        public EventHandler EntityAccepted;

        //Создана - только для невыданных сумм - Заявитель, Согласователь
        //Согласована - Согласователь
        public bool CanEditOnlyinStateNRC_OrRoleCoordinator
        {
            get
            {
                //В новой редактирование всегда разрешено
                if (Entity.CashRequest == null){
                    return true;
                } else {
                    return (
                        Entity.CashRequest.State == CashRequest.States.New &&
                        !Entity.ObservableExpenses.Any() &&
                        (UserRole == UserRole.RequestCreator
                         || UserRole == UserRole.Coordinator)
                        ||
                        (Entity.CashRequest.State == CashRequest.States.Agreed &&
                         UserRole == UserRole.Coordinator));
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