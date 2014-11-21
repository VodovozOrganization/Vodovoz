using System;
using QSOrmProject;

namespace Vodovoz
{
	[OrmSubjectAttributes("Значимость контрагента")]
	public class Significance : IDomainObject
	{
		#region Свойства
		public virtual int Id { get; set; }
		public virtual string Name { get; set; }
		#endregion

		public Significance()
		{
			Name = String.Empty;
		}
	}
}

