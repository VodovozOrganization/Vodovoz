namespace Vodovoz.Core.Data.InfoMessages
{
	/// <summary>
	/// Предупреждающее сообщение
	/// </summary>
	public class WarningMessage
	{
		/// <summary>
		/// Тип
		/// </summary>
		public string Type { get; set; }
		/// <summary>
		/// Заголовок
		/// </summary>
		public string Title { get; set; }
		/// <summary>
		/// Описание
		/// </summary>
		public string Description { get; set; }
		/// <summary>
		/// Название кнопки
		/// </summary>
		public string Button { get; set; }

		public static WarningMessage Create(string type, string title, string description, string button = null) =>
			new WarningMessage
			{
				Type = type,
				Title = title,
				Description = description,
				Button = button
			};
	}
}
