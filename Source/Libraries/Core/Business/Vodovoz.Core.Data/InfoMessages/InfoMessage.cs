using CustomerOrders.Abstractions.Common;

namespace Vodovoz.Core.Data.InfoMessages
{
	public class InfoMessage : IInfoMessage
	{
		private InfoMessage(
			string entity,
			int? refId,
			string position,
			int? iconId,
			string title,
			string description)
		{
			Entity = entity;
			RefId = refId;
			Position = position;
			IconId = iconId;
			Title = title;
			Description = description;
		}
		
		private InfoMessage(string position, int? iconId, string title, string description, ProgressBarInfo progressBar = null)
		{
			Position = position;
			IconId = iconId;
			Title = title;
			Description = description;
			ProgressBar = progressBar;
		}
		
		/// <summary>
		/// Наименование сущности
		/// </summary>
		public string Entity { get; }
		
		/// <summary>
		/// Идентификатор сущности
		/// </summary>
		public int? RefId { get; }

		/// <summary>
		/// Позиция на экране
		/// </summary>
		public string Position { get; }
		
		/// <summary>
		/// Идентификатор иконки
		/// </summary>
		public int? IconId { get; }
		
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
		
		public static InfoMessage Create(
			string entity,
			int? refId,
			string position,
			int? iconId,
			string title,
			string description)
			=> new InfoMessage(entity, refId, position, iconId, title, description);
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
