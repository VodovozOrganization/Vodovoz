﻿using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Application.BankStatements
{
	/// <summary>
	/// Форматы начала строк где находится дата(слово и сама дата в разных ячейках)
	/// </summary>
	public enum BankStatementDateType
	{
		[Display(Name = "за")]
		OnDate,
		[Display(Name = "с")]
		FromDate,
		[Display(Name = "конечная дата")]
		EndDate
	}
}
