using System;
using QS.Commands;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Cash;
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
            UserRole userRole
        ) 
            : base(uowBuilder, unitOfWorkFactory, commonServices) 
        {
            //У новых нужен родитель
            // if (uowBuilder.IsNewEntity)
            //     this.Entity.CashRequest = cashRequest ?? throw new ArgumentNullException(nameof(cashRequest));
            
            this.UserRole = userRole;
            
        }
        
        //Создана - только для невыданных сумм - Заявитель, Согласователь
        //Согласована - Согласователь
        public bool CanEditOnlyinStateNRC_OrRoleCoordinator => (
            Entity.CashRequest.State == CashRequest.States.New &&
            Entity.Expense == null &&
            (UserRole == UserRole.RequestCreator
             || UserRole == UserRole.Coordinator)
            ||
            (Entity.CashRequest.State == CashRequest.States.Agreed &&
             UserRole == UserRole.Coordinator));

        #region Commands

        private DelegateCommand saveCommand;
        public DelegateCommand SaveCommand => saveCommand ?? (saveCommand = new DelegateCommand(
            () => {
                SaveAndClose();
                // if (Validate())
                // {
                //     // Entity.CashRequest.AddItem(Entity);
                //     // UoW.Save(Entity);
                //     
                //     Save();
                //     Close(false, CloseSource.Save);
                //     
                // }

            }, () => true
        ));

        #endregion Commands
        
        
        
    }
}