using System;
using QSOrmProject;

namespace Vodovoz
{
	[OrmSubjectAttibutes("Статусы")]
	public class Status : IDomainObject
	{
		#region Свойства
		public virtual int Id { get; set; }
		public virtual string Name { get; set; }
		#endregion

		public Status ()
		{
			Name = String.Empty;
		}
	}
}

