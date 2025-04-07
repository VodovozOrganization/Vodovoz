﻿using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Edo
{
	public enum EdoTaskType
	{
		/// <summary>
		/// Задача отправки документов клиенту
		/// </summary>
		[Display(Name = "Документ")]
		Document,

		/// <summary>
		/// Задача перемещения ТМЦ
		/// </summary>
		[Display(Name = "Трансфер")]
		Transfer,

		/// <summary>
		/// Задача отправки чека
		/// </summary>
		[Display(Name = "Чек")]
		Receipt,

		/// <summary>
		/// Задача сохранения кодов
		/// </summary>
		[Display(Name = "Сохранение кодов")]
		SaveCode,

		/// <summary>
		/// Задача объемно-сортового учета
		/// </summary>
		[Display(Name = "Объемно-сортовой учет")]
		BulkAccounting,

		/// <summary>
		/// Вывод из оборота
		/// </summary>
		[Display(Name = "Вывод из оборота")]
		Withdrawal,
		
		/// <summary>
		/// Тендер/госзакупки
		/// </summary>
		[Display(Name = "Тендер/госзакупки")]
		Tender
	}
}
