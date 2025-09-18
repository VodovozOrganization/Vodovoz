﻿using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Logistic
{
	/// <summary>
	/// Зона ответственности
	/// </summary>
	[Appellative(
		Nominative = "Зона ответственности")]
	public enum AreaOfResponsibility
	{
		/// <summary>
		/// Логистический отдел
		/// </summary>
		[Display(Name = "Логистический отдел", ShortName = "ЛО")]
		LogisticDepartment,
		/// <summary>
		/// Транспортный отдел
		/// </summary>
		[Display(Name = "Транспортный отдел", ShortName = "ТРо")]
		TransportDepartment
	}
}
