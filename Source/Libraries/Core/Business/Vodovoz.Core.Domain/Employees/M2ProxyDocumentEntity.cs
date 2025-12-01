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
			set => SetField(ref _order, value);
		}

		[Display(Name = "Дата доверенности")]
		public override DateTime Date
		{
			get => Order != null ? (Order.DeliveryDate ?? DateTime.Now) : _date;
			set
			{
				SetField(ref _date, value);
				ExpirationDate = _date.AddDays(10);
			}
		}

		/// <summary>
		/// Сотрудник
		/// </summary>
		[Display(Name = "Сотрудник")]
		public virtual EmployeeEntity Employee
		{
			get => _employee;
			set => SetField(ref _employee, value);
		}

		/// <summary>
		/// Номер наряда
		/// </summary>
		[Display(Name = "Номер наряда")]
		public virtual string TicketNumber
		{
			get => _ticketNumber;
			set => SetField(ref _ticketNumber, value);
		}

		/// <summary>
		/// Дата наряда
		/// </summary>
		[Display(Name = "Дата наряда")]
		public virtual DateTime? TicketDate
		{
			get => _ticketDate;
			set => SetField(ref _ticketDate, value);
		}

		/// <summary>
		/// Поставщик
		/// </summary>
		[Display(Name = "Поставщик")]
		public virtual CounterpartyEntity Supplier
		{
			get => _supplier;
			set => SetField(ref _supplier, value);
		}

		public override DateTime ExpirationDate
		{
			get => _expirationDate;
			set => SetField(ref _expirationDate, value);
		}
	}
}
