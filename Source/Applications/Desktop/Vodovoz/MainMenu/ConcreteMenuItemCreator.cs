using System;
using GLib;
using Gtk;
using Action = Gtk.Action;

namespace Vodovoz.MainMenu
{
	/// <summary>
	/// Создатель различных элементов меню
	/// </summary>
	public class ConcreteMenuItemCreator : MenuItemCreator
	{
		/// <summary>
		/// Создание строки меню с возможной привязкой обработчика нажатия кнопки ButtonPressEventHandler
		/// </summary>
		/// <param name="title">Название</param>
		/// <param name="eventHandler">Обработчик нажатия кнопки</param>
		/// <returns></returns>
		public MenuItem CreateMenuItem(
			string title,
			ButtonPressEventHandler eventHandler = null)
		{
			var menuItem = new MenuItem(title);

			if(eventHandler != null)
			{
				menuItem.ButtonPressEvent += eventHandler;
			}

			return menuItem;
		}

		/// <summary>
		/// Создание строки меню с использованием <see cref="Gtk.Action"/>
		/// </summary>
		/// <param name="action">Экшен хранящий название и обработчик</param>
		/// <returns></returns>
		public Widget CreateMenuItem(Action action)
		{
			return action.CreateMenuItem();
		}

		/// <summary>
		/// Создание чек-бокс строки меню
		/// </summary>
		/// <param name="title">Название</param>
		/// <param name="eventHandler">Обработчик действия</param>
		/// <returns></returns>
		public CheckMenuItem CreateCheckMenuItem(
			string title,
			EventHandler eventHandler = null)
		{
			var menuItem = new CheckMenuItem(title);

			if(eventHandler != null)
			{
				menuItem.Toggled += eventHandler;
			}

			return menuItem;
		}

		/// <summary>
		/// Создание строки меню с картинкой
		/// </summary>
		/// <param name="name">Название для виджета</param>
		/// <param name="title">Отображаемое название</param>
		/// <param name="imageId">Зарегистрированный Id картинки</param>
		/// <param name="tooltip">Всплывающее подсказка</param>
		/// <param name="eventHandler">Обработчик</param>
		/// <returns></returns>
		public Widget CreateImageMenuItem(
			string name,
			string title,
			string imageId,
			string tooltip = null,
			EventHandler eventHandler = null)
		{
			var action = new Action(name, title, tooltip, imageId);
			
			if(eventHandler != null)
			{
				action.Activated += eventHandler;
			}

			return action.CreateMenuItem();
		}
		
		/// <summary>
		/// Создание радио строки меню
		/// </summary>
		/// <param name="title">Отображаемое название</param>
		/// <param name="eventHandler">Обработчик</param>
		/// <param name="group">Принадлежность группе</param>
		/// <returns></returns>
		public RadioMenuItem CreateRadioMenuItem(
			string title,
			EventHandler eventHandler,
			RadioMenuItem group = null)
		{
			var menuitem = group is null ? new RadioMenuItem(title) : new RadioMenuItem(group, title);
			menuitem.Toggled += eventHandler;

			return menuitem;
		}
		
		/// <summary>
		/// Создание радио строки меню
		/// </summary>
		/// <param name="name">Название для виджета</param>
		/// <param name="title">Отображаемое название</param>
		/// <param name="tooltip">Всплывающее подсказка</param>
		/// <param name="imageId">Зарегистрированный Id картинки</param>
		/// <param name="actionValue"></param>
		/// <param name="eventHandler">Обработчик</param>
		/// <param name="actionGroup">Принадлежность группе</param>
		/// <returns></returns>
		public RadioAction CreateRadioAction(
			string name,
			string title,
			string tooltip = null,
			string imageId = null,
			int actionValue = 0,
			EventHandler eventHandler = null,
			RadioAction actionGroup = null)
		{
			var action = new RadioAction(name, title, tooltip, imageId, actionValue);
			action.Group = actionGroup != null ? actionGroup.Group : new SList(IntPtr.Zero);
			
			if(eventHandler != null)
			{
				action.Activated += eventHandler;
			}

			return action;
		}

		
		public override MenuItem Create()
		{
			return new MenuItem();
		}
	}
}
