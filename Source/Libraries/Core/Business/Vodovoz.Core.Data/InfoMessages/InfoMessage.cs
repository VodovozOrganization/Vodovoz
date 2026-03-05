using System.Collections.Generic;

namespace Vodovoz.Core.Data.InfoMessages
{
	/// <summary>
	/// Информационное сообщение
	/// </summary>
	public class InfoMessage
	{
		private InfoMessage(
			string position,
			int iconId,
			string title,
			string description,
			IEnumerable<InfoMessageParameter> descriptionParameters = null)
		{
			Position = position;
			IconId = iconId;
			Title = title;
			Description = description;
			DescriptionParameters = descriptionParameters;
		}

		/// <summary>
		/// Позиция
		/// </summary>
		public string Position { get; }
		/// <summary>
		/// Идентификатор иконки
		/// </summary>
		public int IconId { get; }
		/// <summary>
		/// Заголовок
		/// </summary>
		public string Title { get; }
		/// <summary>
		/// Описание(само ссобщение)
		/// </summary>
		public string Description { get; }
		/// <summary>
		/// Параметры описания
		/// </summary>
		public IEnumerable<InfoMessageParameter> DescriptionParameters { get; }
		public static InfoMessage Create(
			string position,
			int iconId,
			string title,
			string description,
			IEnumerable<InfoMessageParameter> descriptionParameters = null)
			=> new InfoMessage(position, iconId, title, description, descriptionParameters);
	}

	/// <summary>
	/// Параметр информационного сообщения
	/// </summary>
	public class InfoMessageParameter
	{
		private InfoMessageParameter(string name, string value, InfoMessageParameterAction action = null)
		{
			Name = name;
			Value = value;
			Action = action;
		}
		
		/// <summary>
		/// Название
		/// </summary>
		public string Name { get; }
		/// <summary>
		/// Значение
		/// </summary>
		public string Value { get; }
		/// <summary>
		/// Действие
		/// </summary>
		public InfoMessageParameterAction Action { get; }
		
		public static InfoMessageParameter Create(string name, string value, InfoMessageParameterAction action = null)
			=> new InfoMessageParameter(name, value, action);
	}

	/// <summary>
	/// Действие для параметра из сообщения
	/// </summary>
	public class InfoMessageParameterAction
	{
		private InfoMessageParameterAction(string action, string payload)
		{
			Action = action;
			Payload = payload;
		}
		
		/// <summary>
		/// Действие
		/// </summary>
		public string Action { get; }
		/// <summary>
		/// Ссылка или название объекта(например карточка, которую нужно открыть)
		/// </summary>
		public string Payload { get; }
		
		public static InfoMessageParameterAction Create(string action, string payload) =>
			new InfoMessageParameterAction(action, payload);
	}
}
