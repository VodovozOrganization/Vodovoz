﻿using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Complaints
{
	public enum ComplaintGuiltyTypes
	{
		[Display(Name = "Нет")]
		None,
		[Display(Name = "Клиент")]
		Client,
		[Display(Name = "Подразделение")]
		Subdivision,
		[Display(Name = "Сотрудник")]
		Employee
	}

	public class ComplaintGuiltyTypesStringType : NHibernate.Type.EnumStringType
	{
		public ComplaintGuiltyTypesStringType() : base(typeof(ComplaintGuiltyTypes))
		{
		}
	}
}
