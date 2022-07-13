using Gtk;
using System;

namespace Vodovoz.Dialogs
{
	public partial class CastomMessageDlg : Gtk.Dialog
	{
		private readonly int _maxStringLengthWithoutScroll = 500;
		private readonly int _maxLableHeight = 300;
		public CastomMessageDlg(Window parent_window, DialogFlags flags, MessageType type, ButtonsType bt, string format, bool useMarkup = false)
		{
			this.Build();
			
			label1.Visible = textview1.Visible = GtkScrolledWindow.Visible = false;
			imageInfo.Visible = imageWarning.Visible = imageQuestion.Visible = imageError.Visible = false;
			buttonOk.Visible = buttonCancel.Visible = buttonYes.Visible = buttonNo.Visible = false;

			FillLable(format);
			SelectImg(type);
			SelectBtn(bt);
		}

		private void SelectBtn(ButtonsType bt)
		{
			switch(bt)
			{
				case ButtonsType.None:
					break;
				case ButtonsType.Ok:
					buttonOk.Visible = true;
					break;
				case ButtonsType.Close:
					buttonCancel.Visible = true;
					break;
				case ButtonsType.Cancel:
					buttonCancel.Visible = true;
					break;
				case ButtonsType.YesNo:
					buttonYes.Visible = true;
					buttonNo.Visible = true;
					break;
				case ButtonsType.OkCancel:
					buttonOk.Visible = true;
					buttonCancel.Visible = true;
					break;
				default:
					break;
			}
		}

		private void SelectImg(MessageType type)
		{
			switch(type)
			{
				case MessageType.Info:
					imageInfo.Visible = true;
					break;
				case MessageType.Warning:
					imageWarning.Visible = true;
					break;
				case MessageType.Question:
					imageQuestion.Visible = true;
					break;
				case MessageType.Error:
					imageError.Visible = true;
					break;
				case MessageType.Other:
					imageInfo.Visible = true;
					break;
				default:
					imageInfo.Visible = true;
					break;
			}
		}

		private void FillLable(string format)
		{

			if(format?.Length > _maxStringLengthWithoutScroll)
			{
				GtkScrolledWindow.HeightRequest = _maxLableHeight;
				textview1.Buffer.Text = format;
				textview1.Visible = GtkScrolledWindow.Visible = true; 
				
			}
			else
			{
				label1.Wrap = true;
				label1.LineWrap = true;
				label1.Text = format;
				label1.Visible = true;
			}
		}
	}
}
