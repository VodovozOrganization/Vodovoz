using QS.DomainModel.Entity;
using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Vodovoz.Domain.Client;

namespace Vodovoz.Domain.Contacts
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "E-mail адреса",
		Nominative = "E-mail адрес")]
	public class Email : PropertyChangedBase, IDomainObject
	{
		private const string _emailRegEx = @"^[a-zA-Z0-9]+([\._-]?[a-zA-Z0-9]+)*@[a-zA-Z0-9]+([\.-]?[a-zA-Z0-9]+)*(\.[a-zA-Z]{2,10})+$";

		private TimeSpan _emailMatchingProcessTimeout = TimeSpan.FromSeconds(1);

		private string _address;
		private EmailType _emailType;
		private Counterparty _counterparty;

		public virtual int Id { get; set; }

		[Display(Name = "Электронный адрес")]
		public virtual string Address
		{
			get => _address;
			set => SetField(ref _address, value);
		}

		[Display(Name = "Тип адреса")]
		public virtual EmailType EmailType
		{
			get => _emailType;
			set => SetField(ref _emailType, value);
		}

		[Display(Name = "Контрагент")]
		public virtual Counterparty Counterparty
		{
			get => _counterparty;
			set => SetField(ref _counterparty, value);
		}

		public virtual bool IsValidEmail => CheckEmailFormatIsValid();

		private bool CheckEmailFormatIsValid()
		{
			try
			{
				if(Regex.IsMatch(Address, _emailRegEx, RegexOptions.None, _emailMatchingProcessTimeout))
				{
					return true;
				}
			}
			catch(RegexMatchTimeoutException ex)
			{
				return false;
			}
			catch(Exception ex)
			{
				throw ex;
			}

			return false;
		}
	}
}
