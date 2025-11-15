using Gtk;

namespace Vodovoz.MainMenu
{
	/// <summary>
	/// Общий класс создателя меню
	/// </summary>
	public abstract class MenuBarCreator
	{
		/// <summary>
		/// Создание главного меню
		/// </summary>
		/// <returns></returns>
		public abstract MenuBar CreateMenuBar();
	}
}
