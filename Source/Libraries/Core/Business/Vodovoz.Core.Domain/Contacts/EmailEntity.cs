using GMap.NET;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Vodovoz.Core.Domain.Contacts
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "E-mail адреса",
		Nominative = "E-mail адрес")]
	[HistoryTrace]
	public class EmailEntity : PropertyChangedBase, IDomainObject
	{
		public const string EmailRegEx = @"^[a-zA-Z0-9]+([\._-]?[a-zA-Z0-9]+)*@[a-zA-Z0-9]+([\.-]?[a-zA-Z0-9]+)*(\.[a-zA-Z]{2,10})+$";
		private TimeSpan _emailMatchingProcessTimeout = TimeSpan.FromSeconds(1);

		private int _id;
		private string _address;
		private EmailTypeEntity _emailType;

		[Display(Name = "Код")]
		public virtual int Id
		{
			get => _id;
			set => SetField(ref _id, value);
		}

		[Display(Name = "Электронный адрес")]
		public virtual string Address
		{
			get => _address;
			set => SetField(ref _address, value);
		}


		[Display(Name = "Тип адреса")]
		public virtual EmailTypeEntity EmailType
		{
			get => _emailType;
			set => SetField(ref _emailType, value);
		}

		public virtual bool IsValidEmail => CheckEmailFormatIsValid();

		private bool CheckEmailFormatIsValid()
		{
			try
			{
				if(Regex.IsMatch(Address, EmailRegEx, RegexOptions.None, _emailMatchingProcessTimeout))
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
