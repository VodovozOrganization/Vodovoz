using System;
using QSOrmProject;

namespace Vodovoz
{
	[OrmSubject ("Склады")]
	public class Warehouse : PropertyChangedBase, IDomainObject
	{
		#region Свойства

		public virtual int Id { get; set; }

		string name;

		public virtual string Name {
			get { return name; }
			set { SetField (ref name, value, () => Name); }
		}

		#endregion

		public Warehouse ()
		{
			Name = String.Empty;
		}
	}
}