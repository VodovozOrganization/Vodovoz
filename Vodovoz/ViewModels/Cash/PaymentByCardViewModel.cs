using System;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Services;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Tools.CallTasks;
using Order = Vodovoz.Domain.Orders.Order;

namespace Vodovoz.ViewModels.Cash
{
    public class PaymentByCardViewModel: EntityTabViewModelBase<Order> 
    {
        public PaymentByCardViewModel(IEntityUoWBuilder uowBuilder, IUnitOfWorkFactory unitOfWorkFactory, ICommonServices commonServices, CallTaskWorker callTaskWorker) 
            : base(uowBuilder, unitOfWorkFactory, commonServices) {
            
            this.callTaskWorker = callTaskWorker ?? throw new ArgumentNullException(nameof(callTaskWorker));
            TabName = "Оплата по карте";
        }
        
        private readonly CallTaskWorker callTaskWorker;
        
        protected override void BeforeSave() {
            Entity.ChangePaymentTypeToByCard(callTaskWorker);
            if (!Entity.PayAfterShipment)
            {
                Entity.SelfDeliveryToLoading(ServicesConfig.CommonServices.CurrentPermissionService, callTaskWorker);
            }
        }
    }
}