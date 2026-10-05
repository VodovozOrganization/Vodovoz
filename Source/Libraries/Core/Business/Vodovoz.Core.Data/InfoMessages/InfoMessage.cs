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
		
		private InfoMessage(string position, int? iconId, string title, string description)
		{
			Position = position;
			IconId = iconId;
			Title = title;
			Description = description;
		}

		public string Entity { get; }
		public string Position { get; }
		public int? RefId { get;}
		public int? IconId { get;}
		public string Title { get; }
		public string Description { get; }

		public static InfoMessage Create(string position, int? iconId, string title, string description)
			=> new InfoMessage(position, iconId, title, description);
		public static InfoMessage Create(
			string entity,
			int? refId,
			string position,
			int? iconId,
			string title,
			string description)
			=> new InfoMessage(entity, refId, position, iconId, title, description);
	}
}
