namespace Vodovoz.Core.Data.Messages
{
	public class InfoMessage
	{
		protected InfoMessage(string position, string iconName, string title, string description)
		{
			Position = position;
			IconName = iconName;
			Title = title;
			Description = description;
		}
		
		public string Position { get; }
		public string IconName { get; }
		public string Title { get; }
		public string Description { get; }

		public static InfoMessage Create(string position, string iconName, string title, string description) =>
			new InfoMessage(position, iconName, title, description);
	}
}
