using System;

namespace Vodovoz
{
	public class Organization
	{

		#region Свойства
		public int Id { get; set; }
		public string Name { get; set; }
		public string FullName { get; set; }
		public string INN { get; set; }
		public string KPP { get; set; }
		public string OGRN { get; set; }
		public string Phone { get; set; }
		#endregion

		public Organization()
		{


		}
	}
}

