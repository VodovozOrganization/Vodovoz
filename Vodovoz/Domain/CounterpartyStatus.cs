using System;
using QSOrmProject;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz
{
	[OrmSubjectAttributes("Статусы контрагента")]
	public class CounterpartyStatus : IDomainObject
	{
		#region Свойства
		public virtual int Id { get; set; }
		[Required(ErrorMessage = "Название статуса должно быть заполнено.")]
		public virtual string Name { get; set; }
		#endregion

		public CounterpartyStatus ()
		{
			Name = String.Empty;
		}
	}
}

