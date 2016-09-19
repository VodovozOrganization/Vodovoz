using System;
using QSOrmProject;

namespace Vodovoz.ExportTo1c
{
	public class Warehouse1c : IDomainObject
	{
		public int Id{ get; set;}
		public string ExportId{ get; set;}
		public string Name{ get; set; }
		public string Type{ get; set;}

		public static Warehouse1c Default { get; private set;}

		static Warehouse1c(){
			Default = new Warehouse1c
				{
					Id = 1,
					ExportId = "00001",
					Name = "Основной склад",
					Type = "Оптовый"
				};
		}
	}
}

