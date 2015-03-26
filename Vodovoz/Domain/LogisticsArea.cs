using System;
using QSOrmProject;

namespace Vodovoz
{
	[OrmSubject ("Логистические районы")]
	public class LogisticsArea: IDomainObject
	{
		#region Свойства

		public virtual int Id { get; set; }

		public virtual string Name { get; set; }

		#endregion

		public LogisticsArea ()
		{
			Name = String.Empty;
		}
	}
}

