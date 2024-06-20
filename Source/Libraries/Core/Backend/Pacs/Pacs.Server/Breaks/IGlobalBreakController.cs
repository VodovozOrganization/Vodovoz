using Pacs.Core.Messages.Events;
using System;
using Vodovoz.Core.Domain.Pacs;

namespace Pacs.Server.Breaks
{
	public interface IGlobalBreakController
	{
		GlobalBreakAvailabilityEvent BreakAvailability { get; }

		event EventHandler<SettingsChangedEventArgs> SettingsChanged;

		OperatorsOnBreakEvent GetOperatorsOnBreak();

		/// <summary>
		/// Не использовать вне сборки Pacs.Server!!
		/// Должен быть помечен internal при обновлении версии языка до 8.0 !!!
		/// </summary>
		void UpdateBreakAvailability();

		/// <summary>
		/// Не использовать вне сборки Pacs.Server!!
		/// Должен быть помечен internal при обновлении версии языка до 8.0 !!!
		/// </summary>
		void UpdateSettings(IPacsDomainSettings domainSettings);
	}
}
