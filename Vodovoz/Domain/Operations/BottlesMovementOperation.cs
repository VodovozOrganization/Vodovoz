using System;
using QSOrmProject;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz
{
	[OrmSubject ("Передвижения бутылей")]
	public class BottlesMovementOperation: PropertyChangedBase, IDomainObject, IValidatableObject
	{
		public virtual int Id { get; set; }

		DateTime operationTime;

		public virtual DateTime OperationTime {
			get { return operationTime; }
			set { SetField (ref operationTime, value, () => OperationTime); }
		}

		//TODO ID Заказа

		//TODO ID Документа перемещения

		//TODO ID Строки заказа

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

		int movedTo;

		public virtual int MovedTo {
			get { return movedTo; }
			set { SetField (ref movedTo, value, () => MovedTo); }
		}

		int movedFrom;

		public virtual int MovedFrom {
			get { return movedFrom; }
			set { SetField (ref movedFrom, value, () => MovedFrom); }
		}

		#region IValidatableObject implementation

		public System.Collections.Generic.IEnumerable<ValidationResult> Validate (ValidationContext validationContext)
		{
			return null;
		}

		#endregion
	}
}

