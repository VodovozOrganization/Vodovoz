using System.Collections.Generic;

namespace Edo.Problem.Routine.Services
{
	public interface IReceiptContactProblemSourceProvider
	{
		/// <summary>
		/// Имена источников проблем, обрабатываемых воркером.
		/// </summary>
		IReadOnlyCollection<string> SourceNames { get; }

		/// <summary>
		/// Возвращает читабельное название проблемы для уведомления.
		/// </summary>
		/// <param name="sourceName">Имя источника проблемы.</param>
		/// <returns>Читабельное название проблемы.</returns>
		string GetNotificationName(string sourceName);
	}
}
