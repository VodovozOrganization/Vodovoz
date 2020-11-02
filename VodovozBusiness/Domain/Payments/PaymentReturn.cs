using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Domain.Payments
{
    [HistoryTrace]
    public class PaymentReturn : PropertyChangedBase, IDomainObject
    {
        public virtual int Id { get; set; }

        Order order;
        [Display(Name = "Заказ")]
        public virtual Order Order {
            get => order;
            set => SetField(ref order, value);
        }
        
        CashlessMovementOperation cashlessMovementOperation;
        
        public virtual CashlessMovementOperation CashlessMovementOperation {
            get => cashlessMovementOperation;
            set => SetField(ref cashlessMovementOperation, value); 
        }

        decimal sum;
        public virtual decimal Sum {
            get => sum;
            set => SetField(ref sum, value);
        }
        
        public virtual decimal ActualSum => CashlessMovementOperation?.Income ?? Sum;

        public virtual void UpdateOperation()
        {
            if (cashlessMovementOperation == null)
            {
				CashlessMovementOperation = new CashlessMovementOperation()
                {
                    Counterparty = Order.Client,
                    Income = Sum,
                    OperationTime = DateTime.Now
                };
            }
        }
        
        public PaymentReturn() { }
    }
}
