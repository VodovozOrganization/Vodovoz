using System;
using QSOrmProject;

namespace Vodovoz
{
	[OrmSubject ("Цвета оборудования")]
	public class EquipmentColors : PropertyChangedBase, IDomainObject
	{
		#region Свойства

		public virtual int Id { get; set; }

		string name;

		public virtual string Name {
			get { return name; }
			set { SetField (ref name, value, () => Name); }
		}

		#endregion

		public EquipmentColors ()
		{
			Name = String.Empty;
		}
	}
}
