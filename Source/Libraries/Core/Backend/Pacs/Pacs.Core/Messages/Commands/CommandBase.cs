using System;

namespace Pacs.Core.Messages.Commands
{
	/// <summary>
	/// Команда
	/// </summary>
	public abstract class CommandBase
	{
		/// <summary>
		/// Идентификатор команды
		/// </summary>
		public Guid EventId { get; set; }
	}
}
