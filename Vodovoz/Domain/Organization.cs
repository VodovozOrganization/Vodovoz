using System;
using System.ComponentModel;
using System.Collections.Generic;
using QSOrmProject;
using QSBanks;

namespace Vodovoz
{
	[OrmSubjectAttibutes("Организации")]
	public class Organization : QSBanks.IAccountOwner //: PropertyChangedBase
	{

		#region Свойства
		public virtual int Id { get; set; }
		public virtual string Name { get; set; }
		public virtual string FullName { get; set; }
		public virtual string INN { get; set; }
		public virtual string KPP { get; set; }
		public virtual string OGRN { get; set; }
		public virtual string Phone { get; set; }
		public virtual string Email { get; set; }
		public virtual string Address { get; set; }
		public virtual string JurAddress { get; set; }

		//Банковские счета
		public virtual IList<Account> Accounts { get; set;}
		public virtual Account DefaultAccount { get; set;}
		#endregion

		public Organization()
		{
			Name = "Новая организация";
			FullName = String.Empty;
			INN = String.Empty;
			KPP = String.Empty;
			OGRN = String.Empty;
			Phone = String.Empty;
			Email = String.Empty;
			Address = String.Empty;
			JurAddress = String.Empty;
		}
	}
}

