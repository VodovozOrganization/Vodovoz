using System;
using QSOrmProject;

namespace Vodovoz
{
	[OrmSubjectAttibutes("Цвета")]
	public class Colors : IDomainObject
	{
		#region Свойства
		public virtual int Id { get; set; }
		public virtual string Name { get; set; }
		#endregion

		public Colors()
		{
			Name = String.Empty;
		}
	}
}
