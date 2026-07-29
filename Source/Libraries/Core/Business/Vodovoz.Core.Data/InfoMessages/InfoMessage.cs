namespace Vodovoz.Core.Data.InfoMessages
{
	/// <summary>
	/// Информационные сообщения
	/// </summary>
	public class InfoMessage
	{
		private InfoMessage(string position, int? iconId, string title, string description, ProgressBarInfo progressBar = null)
		{
			Position = position;
			IconId = iconId;
			Title = title;
			Description = description;
			ProgressBar = progressBar;
		}

		/// <summary>
		/// Позиция на экране
		/// </summary>
		public string Position { get; }
		/// <summary>
		/// Идентификатор иконки
		/// </summary>
		public int? IconId { get;}
		/// <summary>
		/// Заголовок
		/// </summary>
		public string Title { get; }
		/// <summary>
		/// Описание
		/// </summary>
		public string Description { get; }
		/// <summary>
		/// Данные прогресс бара
		/// </summary>
		public ProgressBarInfo ProgressBar { get; }

		public static InfoMessage Create(string position, int? iconId, string title, string description, ProgressBarInfo progressBar = null)
			=> new InfoMessage(position, iconId, title, description, progressBar);
	}

	/// <summary>
	/// Данные для прогресс бара
	/// </summary>
	public class ProgressBarInfo
	{
		private ProgressBarInfo(decimal current, decimal max)
		{
			Current = current;
			Max = max;
		}
		
		/// <summary>
		/// Текущее значение
		/// </summary>
		public decimal Current { get; }
		/// <summary>
		/// Максимальное
		/// </summary>
		public decimal Max { get; }

		public static ProgressBarInfo Create(decimal current, decimal max) =>
			new ProgressBarInfo(current, max);
	}
}
