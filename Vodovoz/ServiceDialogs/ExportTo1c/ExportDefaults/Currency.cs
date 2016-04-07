using System;
using QSOrmProject;

namespace Vodovoz.ExportTo1c
{
	public class Currency:IDomainObject
	{		
		public int Id{ get; set;}
		public int ExportId{ get; set;}
		public string Name{ get; set; }
		public string FullName{ get; set;}

		public static Currency Default{ get;}

		static Currency()
		{
			Default = new Currency
			{
				Id = 643,
				ExportId = 643,
				Name = "руб.",
				FullName = "Российский рубль"
			};
		}
	}
}

