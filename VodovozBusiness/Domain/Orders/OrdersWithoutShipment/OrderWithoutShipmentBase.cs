using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;

namespace Vodovoz.Domain.Orders.OrdersWithoutShipment
{
	public class OrderWithoutShipmentBase : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		public virtual int Id { get; set; }

		DateTime? createDate;
		[Display(Name = "Дата создания")]
		public virtual DateTime? CreateDate {
			get => createDate;
			set => SetField(ref createDate, value);
		}

		Employee author;
		[Display(Name = "Создатель заказа")]
		public virtual Employee Author {
			get => author;
			set => SetField(ref author, value);
		}

		Counterparty client;
		[Display(Name = "Клиент")]
		public virtual Counterparty Client {
			get => client;
			set 
			{
				if(value == client)
					return;

				SetField(ref client, value);
			}
		}

		bool isOrderWithoutShipmentSent;
		[Display(Name = "Счет отправлен")]
		public virtual bool IsOrderWithoutShipmentSent {
			get => isOrderWithoutShipmentSent;
			set => SetField(ref isOrderWithoutShipmentSent, value);
		}

		public OrderWithoutShipmentBase() { }

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			throw new NotImplementedException();
		}
	}
}
