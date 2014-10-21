using System;
using QSOrmProject;

namespace Vodovoz
{
	[OrmSubjectAttibutes("Контрагенты")]
	public class Counterparty : IDomainObject
	{
		#region Свойства
		public virtual int Id { get; set; }
		public virtual string Name { get; set; }
		public virtual string FullName { get; set; }
		#endregion

		public Counterparty ()
		{
			Name = String.Empty;
			FullName = String.Empty;
		}
	}
}

