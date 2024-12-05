using Vodovoz.Core.Domain.Pacs;

namespace Pacs.Core.Messages.Events
{
	/// <summary>
	/// Событие изменения настроек
	/// </summary>
	public class SettingsEvent : EventBase
	{
		/// <summary>
		/// Настройки
		/// </summary>
		public DomainSettings Settings { get; set; }
	}
}
