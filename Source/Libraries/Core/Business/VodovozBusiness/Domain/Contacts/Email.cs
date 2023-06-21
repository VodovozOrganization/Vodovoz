using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using QS.DomainModel.Entity;
using Vodovoz.Domain.Client;

namespace Vodovoz.Domain.Contacts
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "E-mail адреса",
		Nominative = "E-mail адрес")]
	public class Email : PropertyChangedBase, IDomainObject
	{
		private Counterparty _counterparty;

		public virtual int Id { get; set; }

		private string address;
		[Display(Name = "Электронный адрес")]
		public virtual string Address {
			get { return address; }
			set { SetField(ref address, value, () => Address); }
		}

		private EmailType emailType;
		[Display(Name = "Тип адреса")]
		public virtual EmailType EmailType {
			get { return emailType; }
			set { SetField(ref emailType, value, () => EmailType); }
		}

		[Display(Name = "Контрагент")]
		public virtual Counterparty Counterparty
		{
			get { return _counterparty; }
			set { SetField(ref _counterparty, value); }
		}

		public virtual bool IsValidEmail => IsValidEmailFormat();

		private bool IsValidEmailFormat()
		{
			var emailPattern = @"^\w+([\.-]?\w+)*@\w+([\.-]?\w+)*(\.\w{2,8})+$";
			if(Regex.IsMatch(Address, emailPattern))
			{
				return true;
			}

			return false;
		}
	}
}
