using System;
using QSOrmProject;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz
{
	[OrmSubject ("Статусы контрагента")]
	public class CounterpartyStatus : PropertyChangedBase, IDomainObject
	{
		#region Свойства

		public virtual int Id { get; set; }

		string name;

		[Required (ErrorMessage = "Название статуса должно быть заполнено.")]
		public virtual string Name {
			get { return name; }
			set { SetField (ref name, value, () => Name); }
		}

		#endregion

		public CounterpartyStatus ()
		{
			Name = String.Empty;
		}
	}
}

