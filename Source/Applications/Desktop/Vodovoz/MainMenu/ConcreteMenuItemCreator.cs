using System;
using GLib;
using Gtk;
using Action = Gtk.Action;

namespace Vodovoz.MainMenu
{
	public class ConcreteMenuItemCreator : MenuItemCreator
	{
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

		public Widget CreateMenuItem(Action action, EventHandler eventHandler = null)
		{
			if(eventHandler != null)
			{
				action.Activated += eventHandler;
			}
			
			return action.CreateMenuItem();
		}

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
		
		public RadioMenuItem CreateRadioMenuItem(
			string title,
			EventHandler eventHandler,
			RadioMenuItem group = null)
		{
			var menuitem = group is null ? new RadioMenuItem(title) : new RadioMenuItem(group, title);
			menuitem.Toggled += eventHandler;

			return menuitem;
		}
		
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
	}
}
