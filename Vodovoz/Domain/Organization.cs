using System;
using System.ComponentModel;

namespace Vodovoz
{

	public class Organization //: PropertyChangedBase
	{

		#region Свойства
		public int Id { get; set; }
		public string Name { get; set; }
		public string FullName { get; set; }
		public string INN { get; set; }
		public string KPP { get; set; }
		public string OGRN { get; set; }
		public string Phone { get; set; }
		public string Email { get; set; }
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

