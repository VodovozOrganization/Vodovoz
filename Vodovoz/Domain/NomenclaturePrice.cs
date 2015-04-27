using System;
using QSOrmProject;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz
{
	[OrmSubject (JournalName = "Цены", ObjectName = "цена")]
	public partial class NomenclaturePrice: PropertyChangedBase, IDomainObject
	{
		#region Свойства

		public virtual int Id { get; set; }

		int minCount;

		[Display(Name = "Минимальное количество")]
		public virtual int MinCount {
			get { return minCount; }
			set { SetField (ref minCount, value, () => MinCount); }
		}

		decimal price;

		[Display(Name = "Стоимость")]
		public virtual decimal Price {
			get { return price; }
			set { SetField (ref price, value, () => Price); }
		}

		#endregion

		public NomenclaturePrice ()
		{
		}
	}
}

