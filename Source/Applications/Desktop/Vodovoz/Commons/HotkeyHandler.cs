using System;
using Gtk;
using QS.Tdi.Gtk;

namespace Vodovoz
{
    public static class HotKeyHandler
    {
        private static Gtk.Clipboard clipboard = Gtk.Clipboard.Get(Gdk.Atom.Intern("CLIPBOARD", false));
        public static void HandleKeyPressEvent(object o, KeyPressEventArgs args)
        {
            TdiNotebook mainNotebook = Startup.MainWin.TdiMain;
            if (mainNotebook == null)
                throw new InvalidOperationException(
                    "Вызвано событие TDIHandleKeyPressEvent, но для его корректной работы необходимо заполнить TDIMain.MainNotebook.");

            int platform = (int)Environment.OSVersion.Platform;
            int version = Environment.OSVersion.Version.Major;
            Gdk.ModifierType modifier;

            //Kind of MacOSXCou
            if ((platform == 4 || platform == 6 || platform == 128) && version > 8)
                modifier = Gdk.ModifierType.MetaMask | Gdk.ModifierType.Mod1Mask;
            //Kind of Windows or Unix
            else
                modifier = Gdk.ModifierType.ControlMask;

            //CTRL+C	
            if ((args.Event.Key == Gdk.Key.Cyrillic_es || args.Event.Key == Gdk.Key.Cyrillic_ES || args.Event.Key == Gdk.Key.c || args.Event.Key == Gdk.Key.C)
                && args.Event.State.HasFlag(modifier))
            {
                Widget w = (o as Window).Focus;
                CopyToClipboard(w);
            }
            //CTRL+X
            else if ((args.Event.Key == Gdk.Key.Cyrillic_che || args.Event.Key == Gdk.Key.Cyrillic_CHE || args.Event.Key == Gdk.Key.x || args.Event.Key == Gdk.Key.X)
                     && args.Event.State.HasFlag(modifier))
            {
                Widget w = (o as Window).Focus;
                CutToClipboard(w);
            }
            //CTRL+V
            else if ((args.Event.Key == Gdk.Key.Cyrillic_em || args.Event.Key == Gdk.Key.Cyrillic_EM || args.Event.Key == Gdk.Key.v || args.Event.Key == Gdk.Key.V)
                     && args.Event.State.HasFlag(modifier))
            {
                Widget w = (o as Window).Focus;
                PasteFromClipboard(w);
            }
            //CTRL+A
            else if ((args.Event.Key == Gdk.Key.Cyrillic_ef || args.Event.Key == Gdk.Key.Cyrillic_EF || args.Event.Key == Gdk.Key.a || args.Event.Key == Gdk.Key.A)
                     && args.Event.State.HasFlag(modifier))
            {
                Widget w = (o as Window).Focus;
                SelectAllText(w);
            }
        }

        private static void SelectAllText(Widget w)
        {
            if (w is Editable)
                (w as Editable).SelectRegion(0, -1);
            else if (w is TextView textView)
            {
                var start = textView.Buffer.GetIterAtOffset(0);
                var end = textView.Buffer.GetIterAtOffset(0);
                end.ForwardToEnd();
                textView.Buffer.SelectRange(start, end);
            }
        }

        private static void CopyToClipboard(Widget w)
        {
            int start, end;

            if (w is Editable)
                (w as Editable).CopyClipboard();
            else if (w is TextView)
                (w as TextView).Buffer.CopyClipboard(clipboard);
            else if (w is Label)
            {
                (w as Label).GetSelectionBounds(out start, out end);
                if (start != end)
                    clipboard.Text = (w as Label).Text.Substring(start, end - start);
            }
        }

        private static void CutToClipboard(Widget w)
        {
            int start, end;

            if (w is Editable)
                (w as Editable).CutClipboard();
            else if (w is TextView)
                (w as TextView).Buffer.CutClipboard(clipboard, true);
            else if (w is Label)
            {
                (w as Label).GetSelectionBounds(out start, out end);
                if (start != end)
                    clipboard.Text = (w as Label).Text.Substring(start, end - start);
            }
        }

        private static void PasteFromClipboard(Widget w)
        {
            if (w is Editable)
                (w as Editable).PasteClipboard();
            else if (w is TextView)
                (w as TextView).Buffer.PasteClipboard(clipboard);
        }
    }
}

