using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using DataAnnotationsExtensions;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QSBanks;
using Vodovoz.Domain.Employees;

namespace Vodovoz.Domain
{
	[Appellative (Gender = GrammaticalGender.Feminine,
		NominativePlural = "организации",
		Nominative = "организация")]
	[EntityPermission]
	public class Organization : AccountOwnerBase, IDomainObject
	{

		#region Свойства

		public virtual int Id { get; set; }

		string name;

		[Display (Name = "Название")]
		[Required (ErrorMessage = "Название организации должно быть заполнено.")]
		public virtual string Name {
			get { return name; }
			set { SetField (ref name, value, () => Name); }
		}

		string fullName;

		[Display (Name = "Полное название")]
		public virtual string FullName {
			get { return fullName; }
			set { SetField (ref fullName, value, () => FullName); }
		}

		string iNN;

		[Display (Name = "ИНН")]
		[Digits (ErrorMessage = "ИНН может содержать только цифры.")]
		[StringLength (12, MinimumLength = 0, ErrorMessage = "Номер ИНН не должен превышать 12.")]
		public virtual string INN {
			get { return iNN; }
			set { SetField (ref iNN, value, () => INN); }
		}

		string kPP;

		[Display (Name = "КПП")]
		[Digits (ErrorMessage = "КПП может содержать только цифры.")]
		[StringLength (9, MinimumLength = 0, ErrorMessage = "Номер КПП не должен превышать 9 цифр.")]
		public virtual string KPP {
			get { return kPP; }
			set { SetField (ref kPP, value, () => KPP); }
		}

		string oGRN;

		[Display (Name = "ОГРН/ОГРНИП")]
		[Digits (ErrorMessage = "ОГРН/ОГРНИП может содержать только цифры.")]
		[StringLength (15, MinimumLength = 0, ErrorMessage = "Номер ОГРНИП не должен превышать 15 цифр.")]
		public virtual string OGRN {
			get { return oGRN; }
			set { SetField (ref oGRN, value, () => OGRN); }
		}

		string oKPO;

		[Display(Name = "ОКПО")]
		[Digits(ErrorMessage = "ОКПО может содержать только цифры.")]
		[StringLength(10, MinimumLength = 8, ErrorMessage = "Номер ОКПО не должен превышать 10 цифр.")]
		public virtual string OKPO {
			get { return oKPO; }
			set { SetField(ref oKPO, value, () => OKPO); }
		}

		string oKVED;

		[Display(Name = "ОКВЭД")]
		[StringLength(100, ErrorMessage = "Номера ОКВЭД не должны превышать 100 знаков.")]
		public virtual string OKVED {
			get { return oKVED; }
			set { SetField(ref oKVED, value, () => OKVED); }
		}

		IList<QSContacts.Phone> phones;

		[Display (Name = "Телефоны")]
		public virtual IList<QSContacts.Phone> Phones {
			get { return phones; }
			set { SetField (ref phones, value, () => Phones); }
		}

		string email;

		[Display (Name = "E-mail адреса")]
		public virtual string Email {
			get { return email; }
			set { SetField (ref email, value, () => Email); }
		}

		string address;

		[Display (Name = "Фактический адрес")]
		public virtual string Address {
			get { return address; }
			set { SetField (ref address, value, () => Address); }
		}

		string jurAddress;

		[Display (Name = "Юридический адрес")]
		public virtual string JurAddress {
			get { return jurAddress; }
			set { SetField (ref jurAddress, value, () => JurAddress); }
		}

		Employee leader;

		[Display (Name = "Руководитель")]
		public virtual Employee Leader {
			get { return leader; }
			set { SetField (ref leader, value, () => Leader); }
		}

		Employee buhgalter;

		[Display (Name = "Бухгалтер")]
		public virtual Employee Buhgalter {
			get { return buhgalter; }
			set { SetField (ref buhgalter, value, () => Buhgalter); }
		}

		#endregion

		public Organization ()
		{
			Name = "Новая организация";
			FullName = String.Empty;
			INN = String.Empty;
			KPP = String.Empty;
			OGRN = String.Empty;
			Email = String.Empty;
			Address = String.Empty;
			JurAddress = String.Empty;
		}
	}
}

