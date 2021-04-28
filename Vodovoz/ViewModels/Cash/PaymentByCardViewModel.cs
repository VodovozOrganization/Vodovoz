using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Services;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Orders;
using Vodovoz.Services;
using Vodovoz.Tools.CallTasks;
using Order = Vodovoz.Domain.Orders.Order;

namespace Vodovoz.ViewModels.Cash
{
    public class PaymentByCardViewModel: EntityTabViewModelBase<Order> 
    {
        private readonly IOrderPaymentSettings orderPaymentSettings;

        private readonly CallTaskWorker callTaskWorker;

        public PaymentByCardViewModel(
            IEntityUoWBuilder uowBuilder, 
            IUnitOfWorkFactory unitOfWorkFactory, 
            ICommonServices commonServices, 
            CallTaskWorker callTaskWorker, 
            IOrderPaymentSettings orderPaymentSettings
        ) 
            : base(uowBuilder, unitOfWorkFactory, commonServices) {
            this.orderPaymentSettings = orderPaymentSettings ?? throw new ArgumentNullException(nameof(orderPaymentSettings));
            this.callTaskWorker = callTaskWorker ?? throw new ArgumentNullException(nameof(callTaskWorker));
            TabName = "Оплата по карте";

            ItemsList = UoW.GetAll<PaymentFrom>().ToList();

            if (PaymentByCardFrom==null){
                PaymentByCardFrom = ItemsList.FirstOrDefault(p => p.Id == orderPaymentSettings.DefaultSelfDeliveryPaymentFromId);
            }

            Entity.PropertyChanged += Entity_PropertyChanged;
        }

        void Entity_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Entity.PaymentByCardFrom)){
                OnPropertyChanged(nameof(PaymentByCardFrom));
            }
        }

        public PaymentFrom PaymentByCardFrom
        {
            get => Entity.PaymentByCardFrom;
            set => Entity.PaymentByCardFrom = value;
        }

        public List<PaymentFrom> ItemsList { get; private set; }

        protected override void BeforeSave(){
            Entity.ChangePaymentTypeToByCard(callTaskWorker);

            if (!Entity.PayAfterShipment){
                Entity.SelfDeliveryToLoading(ServicesConfig.CommonServices.CurrentPermissionService, callTaskWorker);
            }

            if (Entity.SelfDelivery){
                Entity.IsSelfDeliveryPaid = true;
            }
        }
    }
}