using Gtk;

namespace Vodovoz.MainMenu
{
	/// <summary>
	/// Общий класс создателя элементов меню
	/// </summary>
	public abstract class MenuItemCreator
	{
		/// <summary>
		/// Создание горизонтального разделителя в меню
		/// </summary>
		/// <returns>Горизонтальный разделитель в меню</returns>
		public static SeparatorMenuItem CreateSeparatorMenuItem() => new SeparatorMenuItem();

		/// <summary>
		/// Создание элемента меню
		/// </summary>
		/// <returns>Элемент меню</returns>
		public abstract MenuItem Create();
	}
}
