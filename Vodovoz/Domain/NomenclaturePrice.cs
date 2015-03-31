using System;
using QSOrmProject;

namespace Vodovoz
{
	[OrmSubject ("Цены")]
	public partial class NomenclaturePrice: PropertyChangedBase, IDomainObject
	{
		#region Свойства

		public virtual int Id { get; set; }

		int minCount;

		public virtual int MinCount {
			get { return minCount; }
			set { SetField (ref minCount, value, () => MinCount); }
		}

		decimal price;

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

