using System;
using QSOrmProject;

namespace Vodovoz
{
	[OrmSubject ("Единицы измерения")]
	public class MeasurementUnits : PropertyChangedBase, IDomainObject
	{
		#region Свойства

		public virtual int Id { get; set; }

		string name;

		public virtual string Name {
			get { return name; }
			set { SetField (ref name, value, () => Name); }
		}

		#endregion

		public MeasurementUnits ()
		{
			Name = String.Empty;
		}
	}
}

