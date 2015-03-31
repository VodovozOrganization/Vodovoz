using System;
using QSOrmProject;

namespace Vodovoz
{
	[OrmSubject ("Национальности")]
	public class Nationality : PropertyChangedBase, IDomainObject
	{
		#region Свойства

		public virtual int Id { get; set; }

		string name;

		public virtual string Name {
			get { return name; }
			set { SetField (ref name, value, () => Name); }
		}

		#endregion


		public Nationality ()
		{
			Name = String.Empty;
		}
	}
}

