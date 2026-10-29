namespace Edo.Problems
{
	/// <summary>
	/// Источник проблемы контакта, обрабатываемой воркером повторной отправки чека.
	/// </summary>
	public interface IReceiptContactProblemSource
	{
		/// <summary>
		/// Имя источника проблемы.
		/// </summary>
		string Name { get; }

		/// <summary>
		/// Читабельное название проблемы для уведомления.
		/// </summary>
		string NotificationName { get; }
	}
}
