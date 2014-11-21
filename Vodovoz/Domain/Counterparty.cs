using System;
using QSOrmProject;
using System.Data.Bindings;
using System.Collections.Generic;
using QSContacts;
using QSProxies;
using System.ComponentModel.DataAnnotations;
using DataAnnotationsExtensions;

namespace Vodovoz
{
	[OrmSubjectAttributes("Контрагенты")]
	public class Counterparty : QSBanks.AccountOwnerBase, QSContacts.IContactOwner, QSProxies.IProxyOwner
	{
		private IList<Contact> contact;
		#region IContact implementation
		public virtual IList<Contact> Contacts {
			get {
				return contact;
			}
			set {
				contact = value;
			}
		}

		#endregion

		private IList<Proxy> proxies;
		#region IProxyOwner implementation
		public virtual IList<Proxy> Proxies {
			get {
				return proxies;
			}
			set {
				proxies = value;
			}
		}
		#endregion

		#region Свойства
		public virtual int Id { get; set; }
		public virtual decimal MaxCredit { get; set; }
		[Required(ErrorMessage = "Название контрагента должно быть заполнено.")]
		public virtual string Name { get; set; }
		public virtual string FullName { get; set; }
		public virtual string Comment { get; set; }
		public virtual string WaybillComment { get; set; }
		[Digits(ErrorMessage = "ИНН может содержать только цифры.")]
		[StringLength(12, MinimumLength = 0, ErrorMessage = "Номер ИНН не должен превышать 12.")]
		public virtual string INN { get; set; }
		[Digits(ErrorMessage = "КПП может содержать только цифры.")]
		[StringLength(9, MinimumLength = 0, ErrorMessage = "Номер КПП не должен превышать 9 цифр.")]
		public virtual string KPP { get; set; }
		public virtual string JurAddress { get; set; }
		public virtual Payment PaymentMethod { get; set; }
		public virtual PersonType PersonType { get; set; }
		public virtual Significance Significance { get; set; }
		public virtual Counterparty MainCounterparty { get; set; }
		public virtual CounterpartyContract Contract { get; set; }
		public virtual CounterpartyType CounterpartyType { get; set; }
		public virtual CounterpartyStatus Status { get; set; }
		public virtual IList<QSContacts.Phone> Phones { get; set; }
		public virtual IList<QSContacts.Email> Emails { get; set; }
		public virtual Employee Accountant { get; set; }
		public virtual Employee SalesManager { get; set; }
		public virtual Employee BottlesManager { get; set; }
		public virtual Contact MainContact { get; set; }
		public virtual Contact FinancialContact { get; set; }
		#endregion

		public Counterparty ()
		{
			Name = String.Empty;
			FullName = String.Empty;
			Comment = String.Empty;
			WaybillComment = String.Empty;
			INN = String.Empty;
			KPP = String.Empty;
			JurAddress = String.Empty;
		}
	}
	public enum PersonType{
		[ItemTitleAttribute("Физическая")]
		natural,
		[ItemTitleAttribute("Юридическая")]
		legal
	}

	public class PersonTypeStringType : NHibernate.Type.EnumStringType 
	{
		public PersonTypeStringType() : base(typeof(PersonType))
		{}
	}

	public enum Payment{
		[ItemTitleAttribute("Наличная")]
		cash,
		[ItemTitleAttribute("Безналичная")]
		cashless
	}

	public class PaymentStringType : NHibernate.Type.EnumStringType 
	{
		public PaymentStringType() : base(typeof(Payment))
		{}
	}

	public enum CounterpartyType{
		[ItemTitleAttribute("Покупатель")]
		customer,
		[ItemTitleAttribute("Поставщик")]
		supplier,
		[ItemTitleAttribute("Партнер")]
		partner,
	}

	public class CounterpartyTypeStringType : NHibernate.Type.EnumStringType 
	{
		public CounterpartyTypeStringType() : base(typeof(CounterpartyType))
		{}
	}
}

