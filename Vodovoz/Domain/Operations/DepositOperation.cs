ousing System;
using QSOrmProject;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings;

namespace Vodovoz
{
	[OrmSubject ("Передвижения залогов")]
	public class DepositOperation: PropertyChangedBase, IDomainObject, IValidatableObject
	{
		public virtual int Id { get; set; }

		DateTime operationTime;

		public virtual DateTime OperationTime {
			get { return operationTime; }
			set { SetField (ref operationTime, value, () => OperationTime); }
		}

		//TODO ID Заказа

		//TODO ID Строки заказа

		DepositType depositType;

		public virtual DepositType DepositType {
			get { return depositType; }
			set { SetField (ref depositType, value, () => DepositType); }
		}

		Counterparty counterparty;

		public virtual Counterparty Counterparty {
			get { return counterparty; }
			set { SetField (ref counterparty, value, () => Counterparty); }
		}

		DeliveryPoint deliveryPoint;

		public virtual DeliveryPoint DeliveryPoint {
			get { return deliveryPoint; }
			set { SetField (ref deliveryPoint, value, () => DeliveryPoint); }
		}

		Decimal receivedDeposit;

		public virtual Decimal ReceivedDeposit {
			get { return receivedDeposit; }
			set { SetField (ref receivedDeposit, value, () => ReceivedDeposit); }
		}

		Decimal refundDeposit;

		public virtual Decimal RefundDeposit {
			get { return refundDeposit; }
			set { SetField (ref refundDeposit, value, () => RefundDeposit); }
		}

		#region IValidatableObject implementation

		public System.Collections.Generic.IEnumerable<ValidationResult> Validate (ValidationContext validationContext)
		{
			return null;
		}

		#endregion
	}

	public enum DepositType
	{
		[ItemTitleAttribute ("Тара")] bottles,
		[ItemTitleAttribute ("Оборудование")] equipment
	}
}

