using System;
using System.ComponentModel;
using System.Collections.Generic;
using QSOrmProject;
using QSBanks;
using System.ComponentModel.DataAnnotations;
using DataAnnotationsExtensions;

namespace Vodovoz
{
	[OrmSubjectAttributes("Организации")]
	public class Organization : QSBanks.AccountOwnerBase
	{

		#region Свойства
		public virtual int Id { get; set; }
		[Required(ErrorMessage = "Название организации должно быть заполнено.")]
		public virtual string Name { get; set; }
		public virtual string FullName { get; set; }
		[Digits(ErrorMessage = "ИНН может содержать только цифры.")]
		[StringLength(12, MinimumLength = 0, ErrorMessage = "Номер ИНН не должен превышать 12.")]
		public virtual string INN { get; set; }
		[Digits(ErrorMessage = "КПП может содержать только цифры.")]
		[StringLength(9, MinimumLength = 0, ErrorMessage = "Номер КПП не должен превышать 9 цифр.")]
		public virtual string KPP { get; set; }
		[Digits(ErrorMessage = "ОГРН может содержать только цифры.")]
		[StringLength(13, MinimumLength = 0, ErrorMessage = "Номер ОГРН не должен превышать 13 цифр.")]
		public virtual string OGRN { get; set; }
		public virtual IList<QSContacts.Phone> Phones { get; set; }
		public virtual string Email { get; set; }
		public virtual string Address { get; set; }
		public virtual string JurAddress { get; set; }
		public virtual Employee Leader{ get; set; }
		public virtual Employee Buhgalter{ get; set; }
		#endregion

		public Organization()
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

