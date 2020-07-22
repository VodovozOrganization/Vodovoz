using System;
using System.Collections.Generic;
using NHibernate.Criterion;
using QS.Commands;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using Order = Vodovoz.Domain.Orders.Order;

namespace Vodovoz.ViewModels.Cash
{
    public class PaymentByCardViewModel: EntityTabViewModelBase<Order> 
    {
        public PaymentByCardViewModel(IEntityUoWBuilder uowBuilder, IUnitOfWorkFactory unitOfWorkFactory, ICommonServices commonServices, INavigationManager navigation = null) : base(uowBuilder, unitOfWorkFactory, commonServices, navigation) {
            UoW = uowBuilder.CreateUoW<PaymentFrom>(unitOfWorkFactory);
            CreateCommands();
        }
        
        private IEnumerable<PaymentFrom> paymentsFrom;
        public IEnumerable<PaymentFrom> PaymentsFrom
        {
            get { return paymentsFrom ?? (paymentsFrom = UoW.GetAll<PaymentFrom>()); }
        }
        
        private PaymentFrom selectedPaymentFrom;
        public PaymentFrom SelectedPaymentFrom
        {
            get => selectedPaymentFrom;
            set => SetField(ref selectedPaymentFrom, value);
        }

        private void CreateCommands() {
            #region CreateSaveItemCommand
            SaveItemCommand = new DelegateCommand(
                () =>
                {
                    Entity.PaymentByCardFrom = new PaymentFrom(){Id = selectedPaymentFrom.Id, Name = selectedPaymentFrom.Name};
                    Entity.OnlineOrder = int.Parse(onlineOrderNumber);
                    Entity.PaymentType = PaymentType.ByCard;
                    if (!Entity.PayAfterShipment)
                    {
                        Entity.OrderStatus = OrderStatus.Accepted;
                        Entity.OrderPaymentStatus = OrderPaymentStatus.Paid;
                        
                    }
                    else if(Entity.PayAfterShipment)
                    {
                        Entity.OrderStatus = OrderStatus.Closed;
                    }
                    UoW.Save();
                    UoW.Commit();
                    SaveAndClose();
                },
                () => true
                );
            #endregion
        }

        public DelegateCommand SaveItemCommand { get; private set; }
    

        private string onlineOrderNumber;
        public string OnlineOrderNumber
        {
            get => onlineOrderNumber;
            set => SetField(ref onlineOrderNumber, value);
        }
        
    }
}