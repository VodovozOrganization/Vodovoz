namespace Vodovoz.Core.Data.InfoMessages
{
	public class InfoMessage
	{
		private InfoMessage(string position, int iconId, string title, string description)
		{
			Position = position;
			IconId = iconId;
			Title = title;
			Description = description;
		}

		public string Position { get; }
		public int IconId { get;}
		public string Title { get; }
		public string Description { get; }

		public static InfoMessage Create(string position, int iconId, string title, string description)
			=> new InfoMessage(position, iconId, title, description);
	}
}
