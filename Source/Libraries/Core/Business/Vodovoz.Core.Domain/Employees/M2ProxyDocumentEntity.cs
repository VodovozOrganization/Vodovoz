using QS.Print;
using System;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Orders;

namespace Vodovoz.Core.Domain.Employees
{
	public class M2ProxyDocumentEntity : ProxyDocumentEntity
	{
		private OrderEntity _order;
		private DateTime _date = DateTime.Now;
		private EmployeeEntity _employee;
		private string _ticketNumber;
		private DateTime? _ticketDate;
		private CounterpartyEntity _supplier;
		private DateTime _expirationDate;

		/// <summary>
		/// Заголовок
		/// </summary>
		[Display(Name = "Заголовок")]
		public virtual string Title => $"Доверенность по форме № М-2 № {Id}";

		public override ProxyDocumentType Type => ProxyDocumentType.M2Proxy;

		public override PrinterType PrintType => PrinterType.ODT;

		/// <summary>
		/// Заказ для которого создавался документ
		/// </summary>
		/// <value>Заказ</value>
		[Display(Name = "Заказ")]
		public virtual OrderEntity Order
		{
			get => _order;
			set => SetField(ref _order, value, () => Order);
		}

		[Display(Name = "Дата доверенности")]
		public override DateTime Date
		{
			get => Order != null ? (Order.DeliveryDate ?? DateTime.Now) : _date;
			set
			{
				SetField(ref _date, value, () => Date);
				ExpirationDate = _date.AddDays(10);
			}
		}

		/// <summary>
		/// Сотрудник
		/// </summary>
		[Display(Name = "Сотрудник")]
		public virtual EmployeeEntity Employee
		{
			get { return _employee; }
			set { SetField(ref _employee, value, () => Employee); }
		}

		/// <summary>
		/// Номер наряда
		/// </summary>
		[Display(Name = "Номер наряда")]
		public virtual string TicketNumber
		{
			get { return _ticketNumber; }
			set { SetField(ref _ticketNumber, value, () => TicketNumber); }
		}

		/// <summary>
		/// Дата наряда
		/// </summary>
		[Display(Name = "Дата наряда")]
		public virtual DateTime? TicketDate
		{
			get { return _ticketDate; }
			set { SetField(ref _ticketDate, value, () => TicketDate); }
		}

		/// <summary>
		/// Поставщик
		/// </summary>
		[Display(Name = "Поставщик")]
		public virtual CounterpartyEntity Supplier
		{
			get { return _supplier; }
			set { SetField(ref _supplier, value, () => Supplier); }
		}

		public override DateTime ExpirationDate
		{
			get => _expirationDate;
			set { SetField(ref _expirationDate, value, () => ExpirationDate); }
		}
	}
}
