using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.DomainModel.UoW;
using QS.Print;
using QSOrmProject;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Domain.Employees
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "Доверенности М-2",
		Nominative = "Доверенность М-2")]
	[EntityPermission]
	public class M2ProxyDocument : ProxyDocument, IValidatableObject
	{
		public virtual string Title {
			get {
				return String.Format("Доверенность по форме № М-2 № {0}", Id);
			}
		}

		public override ProxyDocumentType Type {
			get {
				return ProxyDocumentType.M2Proxy;
			}
		}

		public override PrinterType PrintType {
			get {
				return PrinterType.ODT;
			}
		}

		Order order;

		/// <summary>
		/// Заказ для которого создавался документ
		/// </summary>
		/// <value>Заказ</value>
		[Display(Name = "Заказ")]
		public virtual Order Order {
			get { return order; }
			set { SetField(ref order, value, () => Order); }
		}

		DateTime date = DateTime.Now;

		[Display(Name = "Дата доверенности")]
		public override DateTime Date {
			get  => Order != null ? (Order.DeliveryDate ?? DateTime.Now) : date;
			set {
				SetField(ref date, value, () => Date);
				ExpirationDate = date.AddDays(10);
			}
		}

		Employee employee;
		[Display(Name = "Сотрудник")]
		public virtual Employee Employee {
			get { return employee; }
			set { SetField(ref employee, value, () => Employee); }
		}

		String ticketNumber;
		[Display(Name = "Номер наряда")]
		public virtual String TicketNumber {
			get { return ticketNumber; }
			set { SetField(ref ticketNumber, value, () => TicketNumber); }
		}

		DateTime? ticketDate;
		[Display(Name = "Дата наряда")]
		public virtual DateTime? TicketDate {
			get { return ticketDate; }
			set { SetField(ref ticketDate, value, () => TicketDate); }
		}

		Counterparty supplier;
		[Display(Name = "Поставщик")]
		public virtual Counterparty Supplier {
			get { return supplier; }
			set { SetField(ref supplier, value, () => Supplier); }
		}

		DateTime expirationDate;
		public override DateTime ExpirationDate {
			get => expirationDate;
			set { SetField(ref expirationDate, value, () => ExpirationDate); }
		}

		//Конструкторы
		public static IUnitOfWorkGeneric<M2ProxyDocument> Create()
		{
			var uow = UnitOfWorkFactory.CreateWithNewRoot<M2ProxyDocument>();
			return uow;
		}

		public virtual bool UpdateM2ProxyDocumentTemplate(IUnitOfWork uow)
		{
			if(Id > 0 || Organization == null)
				return false;
			DocumentTemplate = Repository.Client.DocTemplateRepository.GetFirstAvailableTemplate(uow, TemplateType.M2Proxy, Organization);
			return DocumentTemplate != null;
		}

		#region IValidatableObject implementation

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(Organization == null)
				yield return new ValidationResult(String.Format("Не выбрана организация"));

			if(DocumentTemplate == null)
				yield return new ValidationResult(String.Format("Не выбран шаблон доверенности"));
		}

		#endregion
	}
}