using System;
using QSOrmProject;

namespace Vodovoz
{
	[OrmSubjectAttributes("Цены")]
	public partial class NomenclaturePrice
	{
		#region Свойства
		public virtual int Id { get; set; }
		public virtual int MinCount { get; set; }
		public virtual decimal Price { get; set; }
		#endregion

		public NomenclaturePrice ()
		{
		}
	}
}

