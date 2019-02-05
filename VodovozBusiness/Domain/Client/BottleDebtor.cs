using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using Vodovoz.Domain.Employees;

namespace Vodovoz.Domain.Client
{
	public class BottleDebtor : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		public virtual int Id { get; set; }

		Counterparty client;
		[Display(Name = "Клиент")]
		public virtual Counterparty Client {
			get { return client; }
			set { SetField(ref client, value, () => Client); }
		}

		String comment;
		[Display(Name = "Комментарий")]
		public virtual String Comment {
			get { return comment; }
			set { SetField(ref comment, value, () => Comment); }
		}

		DeliveryPoint address;
		[Display(Name = "Aдрес клиента")]
		public virtual DeliveryPoint Address {
			get { return address; }
			set { SetField(ref address, value, () => Address); }
		}

		int debtByAdress;
		[Display(Name = "Долг по адресу")]
		public virtual int DebtByAdress {
			get { return debtByAdress; }
			set { SetField(ref debtByAdress, value, () => DebtByAdress); }
		}

		int debtByClient;
		[Display(Name = "Долг по клиенту")]
		public virtual int DebtByClient {
			get { return debtByClient; }
			set { SetField(ref debtByClient, value, () => DebtByClient); }
		}

		DebtorStatus taskState;
		[Display(Name = "Статус")]
		public virtual DebtorStatus TaskState {
			get { return taskState; }
			set { SetField(ref taskState, value, () => TaskState); }
		}

		DateTime dateOfTaskCreation;
		[Display(Name = "Дата создания задачи")]
		public virtual DateTime DateOfTaskCreation {
			get { return dateOfTaskCreation; }
			set { SetField(ref dateOfTaskCreation, value, () => DateOfTaskCreation); }
		}

		DateTime? nextCallDate;
		[Display(Name = "Дата создания задачи")]
		public virtual DateTime? NextCallDate {
			get { return nextCallDate; }
			set { SetField(ref nextCallDate, value, () => NextCallDate); }
		}

		Employee assignedEmployee;
		[Display(Name = "Закреплено за")]
		public virtual Employee AssignedEmployee {
			get { return assignedEmployee; }
			set { SetField(ref assignedEmployee, value, () => AssignedEmployee); }
		}

		bool isTaskComplete;
		[Display(Name = "Статус выполнения задачи")]
		public virtual bool IsTaskComplete {
			get { return isTaskComplete; }
			set { SetField(ref isTaskComplete, value, () => IsTaskComplete); }
		}


		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(Client==null)
				throw new NotImplementedException();
			if(Address == null)
				throw new NotImplementedException();

			throw new NotImplementedException();
		}
	}

	public enum DebtorStatus
	{
		[Display(Name = "Звонок")]
		Call,
		[Display(Name = "Задание")]
		Task,
		[Display(Name = "Сложный клиент")]
		DifficultСlient
	}

}
