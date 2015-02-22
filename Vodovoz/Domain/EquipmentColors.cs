using System;
using QSOrmProject;

namespace Vodovoz
{
	[OrmSubject("Цвета оборудования")]
	public class EquipmentColors : IDomainObject
	{
		#region Свойства
		public virtual int Id { get; set; }
		public virtual string Name { get; set; }
		#endregion

		public EquipmentColors()
		{
			Name = String.Empty;
		}
	}
}
