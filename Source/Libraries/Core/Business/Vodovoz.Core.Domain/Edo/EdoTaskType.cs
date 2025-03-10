﻿namespace Vodovoz.Core.Domain.Edo
{
	public enum EdoTaskType
	{
		/// <summary>
		/// Задача отправки документов клиенту
		/// </summary>
		Document,

		/// <summary>
		/// Задача перемещения ТМЦ
		/// </summary>
		Transfer,

		/// <summary>
		/// Задача отправки чека
		/// </summary>
		Receipt,

		/// <summary>
		/// Задача сохранения кодов
		/// </summary>
		SaveCode,

		/// <summary>
		/// Задача объемно-сортового учета
		/// </summary>
		BulkAccounting,

		/// <summary>
		/// Вывод из оборота
		/// </summary>
		Withdrawal
	}
}
