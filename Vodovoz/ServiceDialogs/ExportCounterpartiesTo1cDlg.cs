using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Gtk;
using NHibernate.Util;
using QSProjectsLib;
using Vodovoz.ServiceDialogs.ExportTo1c;

namespace Vodovoz.ServiceDialogs
{
	public partial class ExportCounterpartiesTo1cDlg : QS.Dialog.Gtk.TdiTabBase
	{
		bool exportInProgress;

		public ExportCounterpartiesTo1cDlg()
		{
			this.Build();
			TabName = "Экспорт контрагентов в 1С";
		}

		ExportCounterpariesData exportData;

		protected void OnBtnRunToFileClicked(object sender, EventArgs e)
		{
			var exportOperation = new ExportCounterpartiesTo1C();

			this.exportInProgress = true;
			LongOperationDlg.StartOperation(exportOperation.Run, "", 1, false);
			this.exportInProgress = false;

			exportData = exportOperation.Result;

			var hasError = UpdateErrors(exportData.Errors);
			if(hasError)
				return;
			btnRunToFile.Sensitive = !hasError && !exportInProgress;

			var settings = new XmlWriterSettings {
				OmitXmlDeclaration = true,
				Indent = true,
				Encoding = System.Text.Encoding.UTF8,
				NewLineChars = "\r\n"
			};
			var fileChooser = new Gtk.FileChooserDialog("Выберите файл для сохранения выгрузки",
				(Window)this.Toplevel,
				Gtk.FileChooserAction.Save,
				"Отмена", ResponseType.Cancel,
				"Сохранить", ResponseType.Accept
			);
			var dateText = DateTime.Now.ToShortDateString().Replace(System.IO.Path.DirectorySeparatorChar, '-');

			fileChooser.CurrentName = $"Выгрузка контрагентов на {dateText}.xml";
			var filter = new FileFilter();
			filter.AddPattern("*.xml");
			fileChooser.Filter = filter;
			if(fileChooser.Run() == (int)ResponseType.Accept) {
				var filename = fileChooser.Filename.EndsWith(".xml") ? fileChooser.Filename : fileChooser.Filename + ".xml";
				using(XmlWriter writer = XmlWriter.Create(filename, settings)) {
					exportData.ToXml().WriteTo(writer);
				}
			}
			fileChooser.Destroy();
		}

		/// <summary>
		/// Если метод вернул true, это значит что есть ошибки.
		/// </summary>
		bool UpdateErrors(List<string> errors)
		{
			GtkScrolledWindowErrors.Visible = errors.Any();
			if(errors.Any()) {
				TextTagTable textTags = new TextTagTable();
				var tag = new TextTag("Red");
				tag.Foreground = "red";
				textTags.Add(tag);
				TextBuffer tempBuffer = new TextBuffer(textTags);
				TextIter iter = tempBuffer.EndIter;
				tempBuffer.InsertWithTags(ref iter, String.Join("\n", errors), tag);
				textviewErrors.Buffer = tempBuffer;
				return true;
			}
			return false;
		}
	}
}