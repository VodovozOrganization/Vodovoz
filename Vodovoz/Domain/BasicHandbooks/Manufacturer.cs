using System;
using QSOrmProject;

namespace Vodovoz
{
	[OrmSubject ("Производители")]
	public class Manufacturer : PropertyChangedBase, IDomainObject
	{
		#region Свойства

		public virtual int Id { get; set; }

		string name;

		public virtual string Name {
			get { return name; }
			set { SetField (ref name, value, () => Name); }
		}

		#endregion

		public Manufacturer ()
		{
			Name = String.Empty;
		}
	}
}
