using System;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Complaints
{
	public enum ComplaintGuiltyTypes
	{
		[Display(Name = "Клиент")]
		Client,
		[Display(Name = "Подразделение")]
		Subdivision,
		[Display(Name = "Сотрудник")]
		Employee,
		[Display(Name = "Нет")]
		None
	}

	public class ComplaintGuiltyTypesStringType : NHibernate.Type.EnumStringType
	{
		public ComplaintGuiltyTypesStringType() : base(typeof(ComplaintGuiltyTypes))
		{
		}
	}
}
