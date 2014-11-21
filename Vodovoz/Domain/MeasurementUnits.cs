using System;
using QSOrmProject;

namespace Vodovoz
{
	[OrmSubjectAttributes("Единицы измерения")]
	public class MeasurementUnits : IDomainObject
	{
		#region Свойства
		public virtual int Id { get; set; }
		public virtual string Name { get; set; }
		#endregion

		public MeasurementUnits()
		{
			Name = String.Empty;
		}
	}
}

