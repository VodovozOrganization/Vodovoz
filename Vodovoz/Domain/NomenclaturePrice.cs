using System;
using QSOrmProject;

namespace Vodovoz
{
	[OrmSubjectAttibutes("Цены")]
	public partial class NomenclaturePrice
	{
		#region Свойства
		public virtual int Id { get; set; }
		public virtual double MinCount { get; set; }
		public virtual double Price { get; set; }
		#endregion

		public NomenclaturePrice ()
		{
		}
	}
}

