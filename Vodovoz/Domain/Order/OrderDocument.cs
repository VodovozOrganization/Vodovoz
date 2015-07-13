using System;
using QSOrmProject;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings;

namespace Vodovoz.Domain.Orders
{
	[OrmSubject (JournalName = "Документ заказа", ObjectName = "документы заказа")]
	public class OrderDocument : PropertyChangedBase, IDomainObject
	{
		public virtual int Id { get; set; }

		Order order;

		[Display (Name = "Заказ")]
		public virtual Order Order {
			get { return order; }
			set { SetField (ref order, value, () => Order); }
		}

		[Display (Name = "Тип документа заказа")]
		public virtual OrderDocumentType Type {
			get {	 
				if (this is OrderAgreement)
					return OrderDocumentType.AdditionalAgreement;
				return OrderDocumentType.Contract;
			}
		}

		public virtual string Name { get { return "Не указан"; } }

		public virtual string DocumentDate { get { return "Не указано"; } }
	}

	public class OrderAgreement : OrderDocument
	{
		AdditionalAgreement additionalAgreement;

		[Display (Name = "Доп. соглашение")]
		public virtual AdditionalAgreement AdditionalAgreement {
			get { return additionalAgreement; }
			set { SetField (ref additionalAgreement, value, () => AdditionalAgreement); }
		}

		public override string Name {
			get { return String.Format ("Доп. соглашение {0} №{1}", 
				additionalAgreement.AgreementTypeTitle, 
				additionalAgreement.AgreementNumber); }
		}

		public override string DocumentDate {
			get { return AdditionalAgreement.DocumentDate; }
		}
	}

	public class OrderContract : OrderDocument
	{
		CounterpartyContract contract;

		[Display (Name = "Договор")]
		public virtual CounterpartyContract Contract {
			get { return contract; }
			set { SetField (ref contract, value, () => Contract); }
		}

		public override string Name {
			get { return String.Format ("Договор №{0}", contract.Number); }
		}

		public override string DocumentDate {
			get { return String.Format ("От {0}", Contract.IssueDate.ToShortDateString ()); }
		}
	}

	public enum OrderDocumentType
	{
		[ItemTitleAttribute ("Доп. соглашение для заказа")]
		AdditionalAgreement,
		[ItemTitleAttribute ("Договор для заказа")]
		Contract
	}
}

