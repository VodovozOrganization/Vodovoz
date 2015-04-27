using System;
using System.ComponentModel.DataAnnotations;
using QSOrmProject;

namespace Vodovoz
{
	[OrmSubject (JournalName = "Логистические районы", ObjectName = "логистический район")]
	public class LogisticsArea: PropertyChangedBase, IDomainObject
	{
		#region Свойства

		public virtual int Id { get; set; }

		string name;
		[Required (ErrorMessage = "Название должно быть заполнено.")]
		[Display(Name = "Название")]
		public virtual string Name {
			get { return name; }
			set { SetField (ref name, value, () => Name); }
		}

		#endregion

		public LogisticsArea ()
		{
			Name = String.Empty;
		}
	}
}

