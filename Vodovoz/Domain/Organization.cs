using System;
using System.ComponentModel;
using System.Collections.Generic;
using QSOrmProject;
using QSBanks;
using System.ComponentModel.DataAnnotations;
using DataAnnotationsExtensions;

namespace Vodovoz
{
	[OrmSubject ("Организации")]
	public class Organization : AccountOwnerBase
	{

		#region Свойства

		public virtual int Id { get; set; }

		string name;

		[Required (ErrorMessage = "Название организации должно быть заполнено.")]
		public virtual string Name {
			get { return name; }
			set { SetField (ref name, value, () => Name); }
		}

		string fullName;

		public virtual string FullName {
			get { return fullName; }
			set { SetField (ref fullName, value, () => FullName); }
		}

		string iNN;

		[Digits (ErrorMessage = "ИНН может содержать только цифры.")]
		[StringLength (12, MinimumLength = 0, ErrorMessage = "Номер ИНН не должен превышать 12.")]
		public virtual string INN {
			get { return iNN; }
			set { SetField (ref iNN, value, () => INN); }
		}

		string kPP;

		[Digits (ErrorMessage = "КПП может содержать только цифры.")]
		[StringLength (9, MinimumLength = 0, ErrorMessage = "Номер КПП не должен превышать 9 цифр.")]
		public virtual string KPP {
			get { return kPP; }
			set { SetField (ref kPP, value, () => KPP); }
		}

		string oGRN;

		[Digits (ErrorMessage = "ОГРН может содержать только цифры.")]
		[StringLength (13, MinimumLength = 0, ErrorMessage = "Номер ОГРН не должен превышать 13 цифр.")]
		public virtual string OGRN {
			get { return oGRN; }
			set { SetField (ref oGRN, value, () => OGRN); }
		}

		IList<QSContacts.Phone> phones;

		public virtual IList<QSContacts.Phone> Phones {
			get { return phones; }
			set { SetField (ref phones, value, () => Phones); }
		}

		string email;

		public virtual string Email {
			get { return email; }
			set { SetField (ref email, value, () => Email); }
		}

		string address;

		public virtual string Address {
			get { return address; }
			set { SetField (ref address, value, () => Address); }
		}

		string jurAddress;

		public virtual string JurAddress {
			get { return jurAddress; }
			set { SetField (ref jurAddress, value, () => JurAddress); }
		}

		Employee leader;

		public virtual Employee Leader {
			get { return leader; }
			set { SetField (ref leader, value, () => Leader); }
		}

		Employee buhgalter;

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

