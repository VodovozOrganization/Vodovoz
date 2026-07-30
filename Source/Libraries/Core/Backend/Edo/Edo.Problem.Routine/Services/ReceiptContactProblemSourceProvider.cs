using System;
using System.Collections.Generic;
using System.Linq;
using Edo.Problems;

namespace Edo.Problem.Routine.Services
{
	/// <summary>
	/// Предоставляет источники проблем контакта, обрабатываемые воркером отправки чека.
	/// </summary>
	public class ReceiptContactProblemSourceProvider : IReceiptContactProblemSourceProvider
	{
		private readonly IReadOnlyDictionary<string, string> _notificationNamesBySourceName;

		/// <summary>
		/// Создает провайдер источников проблем контакта.
		/// </summary>
		/// <param name="sources">Источники проблем контакта, зарегистрированные для воркера.</param>
		public ReceiptContactProblemSourceProvider(IEnumerable<IReceiptContactProblemSource> sources)
		{
			if(sources == null)
			{
				throw new ArgumentNullException(nameof(sources));
			}

			var registeredSources = sources.ToList();

			if(!registeredSources.Any())
			{
				throw new InvalidOperationException("Не зарегистрированы источники проблем контакта для отправки чека");
			}

			_notificationNamesBySourceName = registeredSources.ToDictionary(source => source.Name, source => source.NotificationName);
			SourceNames = _notificationNamesBySourceName.Keys.ToList();
		}

		/// <summary>
		/// Имена источников проблем, обрабатываемых воркером.
		/// </summary>
		public IReadOnlyCollection<string> SourceNames { get; }

		/// <summary>
		/// Возвращает читабельное название проблемы для уведомления.
		/// </summary>
		/// <param name="sourceName">Имя источника проблемы.</param>
		/// <returns>Читабельное название проблемы.</returns>
		public string GetNotificationName(string sourceName)
		{
			if(sourceName == null)
			{
				throw new ArgumentNullException(nameof(sourceName));
			}

			return _notificationNamesBySourceName[sourceName];
		}
	}
}
