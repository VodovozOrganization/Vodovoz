using System;
using System.ComponentModel.DataAnnotations;
using QSOrmProject;

namespace Vodovoz.Domain
{
	[OrmSubject (JournalName = "Склады", ObjectName = "склад")]
	public class Warehouse : PropertyChangedBase, IDomainObject
	{
		#region Свойства

		public virtual int Id { get; set; }

		string name;
		[Required (ErrorMessage = "Название статуса должно быть заполнено.")]
		[Display(Name = "Название")]
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