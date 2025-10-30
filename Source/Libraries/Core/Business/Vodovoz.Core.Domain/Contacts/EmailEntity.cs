using QS.DomainModel.Entity;
using QS.HistoryLog;
using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Vodovoz.Core.Domain.Contacts
{
	/// <summary>
	/// Email-адрес
	/// </summary>
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "E-mail адреса",
		Nominative = "E-mail адрес")]
	[HistoryTrace]
	public class EmailEntity : PropertyChangedBase, IDomainObject
	{
		public const string EmailRegEx = @"^[a-zA-Z0-9]+([\._-]?[a-zA-Z0-9]*)*@[a-zA-Z0-9]+([\.-]?[a-zA-Z0-9]+)*(\.[a-zA-Z]{2,10})+$";
		private TimeSpan _emailMatchingProcessTimeout = TimeSpan.FromSeconds(1);

		private int _id;
		private string _address;
		private EmailTypeEntity _emailType;

		/// <summary>
		/// Идентификатор<br/>
		/// Код
		/// </summary>
		[Display(Name = "Код")]
		public virtual int Id
		{
			get => _id;
			set => SetField(ref _id, value);
		}

		/// <summary>
		/// Электронный адрес<br/>
		/// Адрес электронной почты
		/// </summary>
		[Display(Name = "Электронный адрес")]
		public virtual string Address
		{
			get => _address;
			set => SetField(ref _address, value);
		}

		/// <summary>
		/// Тип адреса
		/// </summary>
		[Display(Name = "Тип адреса")]
		public virtual EmailTypeEntity EmailType
		{
			get => _emailType;
			set => SetField(ref _emailType, value);
		}

		/// <summary>
		/// Проверка корректности формата адреса электронной почты
		/// </summary>
		public virtual bool IsValidEmail => CheckEmailFormatIsValid();

		/// <summary>
		/// Проверка корректности формата адреса электронной почты
		/// </summary>
		/// <returns></returns>
		private bool CheckEmailFormatIsValid()
		{
			try
			{
				if(string.IsNullOrWhiteSpace(Address))
				{
					return false;
				}
				
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
