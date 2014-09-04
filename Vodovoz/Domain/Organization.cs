using System;
using System.ComponentModel;
using QSOrmProject;

namespace Vodovoz
{
	[OrmSubjectAttibutes("Организации")]
	public class Organization //: PropertyChangedBase
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
		}
	}
}

