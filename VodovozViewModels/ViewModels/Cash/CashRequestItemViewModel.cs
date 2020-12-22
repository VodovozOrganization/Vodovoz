using System;
using QS.Commands;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Employees;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.ViewModelBased;

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
                        Entity.Expense == null &&
                        (UserRole == UserRole.RequestCreator
                         || UserRole == UserRole.Coordinator)
                        ||
                        (Entity.CashRequest.State == CashRequest.States.Agreed &&
                         UserRole == UserRole.Coordinator));
                }
            }
        }

        #region Commands

        private DelegateCommand saveCommand;
        public DelegateCommand SaveCommand => saveCommand ?? (saveCommand = new DelegateCommand(
            () => {
                SaveAndClose();

            }, () => true
        ));

        #endregion Commands
        
        
        
    }
}