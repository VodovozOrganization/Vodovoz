using System.ComponentModel.DataAnnotations;
using DataAnnotationsExtensions;
using QSOrmProject;

namespace Vodovoz
{
	[OrmSubject (JournalName = "Производство воды", ObjectName = "Документ производства")]
	public class IncomingWater: Document
	{
		int amount;
		[Min(1)]
		[Display(Name = "Количество")]
		public virtual int Amount {
			get { return amount; }
			set { SetField (ref amount, value, () => Amount); }
		}

		Warehouse warehouse;
		[Required (ErrorMessage = "Склад должен быть указан.")]
		[Display(Name = "Склад")]
		public virtual Warehouse Warehouse {
			get { return warehouse; }
			set { SetField (ref warehouse, value, () => Warehouse); }
		}
	}
}

