﻿using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Errors.Common
{
	public static class Repository
	{
		/// <summary>
		/// Ошибка получения данных
		/// </summary>
		[Display(Name = "Ошибка получения данных")]		
		public static Error DataRetrievalError => new Error(
			typeof(Repository),
			nameof(DataRetrievalError),
			"Ошибка получения данных");
	}
}
