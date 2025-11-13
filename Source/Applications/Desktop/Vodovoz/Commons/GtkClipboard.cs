using Core.Infrastructure;
using Gtk;

namespace Vodovoz.Commons
{
	public class GtkClipboard : IClipboard
	{
		private readonly Clipboard _clipboard;

		public GtkClipboard()
		{
			_clipboard = Clipboard.Get(Gdk.Atom.Intern("CLIPBOARD", false));
		}

		public void SetText(string text)
		{
			_clipboard.Text = text;
			_clipboard.Store();
		}
		public string GetText()
		{
			return _clipboard.WaitForText();
		}

		public void Clear()
		{
			_clipboard.Clear();
		}
	}
}
