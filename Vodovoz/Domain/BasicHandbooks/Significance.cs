using System;
using QSOrmProject;

namespace Vodovoz
{
	[OrmSubject ("Значимость контрагента")]
	public class Significance : PropertyChangedBase, IDomainObject
	{
		#region Свойства

		public virtual int Id { get; set; }

		string name;

		public virtual string Name {
			get { return name; }
			set { SetField (ref name, value, () => Name); }
		}

		#endregion

		public Significance ()
		{
			Name = String.Empty;
		}
	}
}

