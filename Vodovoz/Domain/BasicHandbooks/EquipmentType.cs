using System;
using QSOrmProject;

namespace Vodovoz
{
	[OrmSubject ("Типы оборудования")]
	public class EquipmentType : PropertyChangedBase, IDomainObject
	{
		#region Свойства

		public virtual int Id { get; set; }

		string name;

		public virtual string Name {
			get { return name; }
			set { SetField (ref name, value, () => Name); }
		}

		#endregion

		public EquipmentType ()
		{
			Name = String.Empty;
		}
	}
}
