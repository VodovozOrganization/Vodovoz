using Gtk;

namespace Vodovoz.MainMenu
{
	public abstract class MenuItemCreator
	{
		public static SeparatorMenuItem CreateSeparatorMenuItem() => new SeparatorMenuItem();
	}
}
