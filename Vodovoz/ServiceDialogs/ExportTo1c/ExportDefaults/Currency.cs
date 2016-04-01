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

		private static Currency defaultCurrency;
		public static Currency Default
		{
			get
			{
				if (defaultCurrency == null)
				{
					defaultCurrency = new Currency
					{
						Id = 643,
						ExportId = 643,
						Name = "руб.",
						FullName = "Российский рубль"
					};
				}
				return defaultCurrency;
			}
		}
	}
}

