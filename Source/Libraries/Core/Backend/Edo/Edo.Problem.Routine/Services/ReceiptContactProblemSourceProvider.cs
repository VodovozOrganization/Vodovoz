using System;
using System.Collections.Generic;
using System.Linq;
using Edo.Problems;

namespace Edo.Problem.Routine.Services
{
	public class ReceiptContactProblemSourceProvider : IReceiptContactProblemSourceProvider
	{
		private readonly IReadOnlyDictionary<string, string> _notificationNamesBySourceName;

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

		public IReadOnlyCollection<string> SourceNames { get; }

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
