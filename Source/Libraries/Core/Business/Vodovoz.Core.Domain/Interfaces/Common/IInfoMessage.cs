namespace Vodovoz.Core.Domain.Interfaces.Common
{
	public interface IInfoMessage
	{
		/// <summary>
		/// Позиция сообщения на экране
		/// </summary>
		string Position { get; }
		/// <summary>
		/// Идентификатор иконки для сообщения
		/// </summary>
		int? IconId { get; }
		/// <summary>
		/// Заголовок
		/// </summary>
		string Title { get; }
		/// <summary>
		/// Сообщение
		/// </summary>
		string Description { get; }
	}
}
