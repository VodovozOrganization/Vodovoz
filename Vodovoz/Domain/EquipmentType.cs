using System;
using QSOrmProject;

namespace Vodovoz
{
	[OrmSubjectAttibutes("Типы оборудования")]
	public class EquipmentType : IDomainObject
	{
		#region Свойства
		public virtual int Id { get; set; }
		public virtual string Name { get; set; }
		#endregion

		public EquipmentType()
		{
			Name = String.Empty;
		}
	}
}
