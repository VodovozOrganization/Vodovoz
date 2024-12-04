namespace Pacs.Core.Messages.Commands
{
	/// <summary>
	/// Результат выполнения команды
	/// </summary>
	public abstract class CommandResult
	{
		/// <summary>
		/// Результат
		/// </summary>
		public virtual Result Result { get; set; }

		/// <summary>
		/// Описание ошибки
		/// </summary>
		public virtual string FailureDescription { get; set; }
	}
}
