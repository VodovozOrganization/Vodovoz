using System;
using System.Xml;
using Gtk;
using QSOrmProject;
using QSProjectsLib;
using Vodovoz.Tools.CommerceML;

namespace Vodovoz.Dialogs.OnlineStore
{
	public partial class ExportToSiteDlg : QSTDI.TdiTabBase
	{
		public ExportToSiteDlg()
		{
			this.Build();
		}

		protected void OnButtonRunToFileClicked(object sender, EventArgs e)
		{
			using(var uow = UnitOfWorkFactory.CreateWithoutRoot())
			{
				var export = new Export(uow);
				export.ProgressUpdated = delegate {
					progressbarTotal.Text = export.CurrentTaskText;
					progressbarTotal.Adjustment.Upper = export.TotalTasks;
					progressbarTotal.Adjustment.Value = export.CurrentTask;
					QSMain.WaitRedraw();
				};;

				export.RunCatalog();

				GtkScrolledWindowErrors.Visible = export.Errors.Count > 0;
				if(export.Errors.Count > 0) {
					TextTagTable textTags = new TextTagTable();
					var tag = new TextTag("Red");
					tag.Foreground = "red";
					textTags.Add(tag);
					TextBuffer tempBuffer = new TextBuffer(textTags);
					TextIter iter = tempBuffer.EndIter;
					tempBuffer.InsertWithTags(ref iter, String.Join("\n", export.Errors), tag);
					textviewErrors.Buffer = tempBuffer;
					return;
				}

				var fileChooser = new Gtk.FileChooserDialog("Выберите файл для сохранения выгрузки",
					(Window)this.Toplevel,
					Gtk.FileChooserAction.Save,
					"Отмена", ResponseType.Cancel,
					"Сохранить", ResponseType.Accept
				);

				var dateText = DateTime.Today.ToShortDateString().Replace(System.IO.Path.DirectorySeparatorChar, '-');
				fileChooser.CurrentName = $"import-{dateText}.xml";
				var filter = new FileFilter();
				filter.AddPattern("*.xml");
				fileChooser.Filter = filter;
				if(fileChooser.Run() == (int)ResponseType.Accept) {
					var filename = fileChooser.Filename.EndsWith(".xml") ? fileChooser.Filename : fileChooser.Filename + ".xml";
					using(XmlWriter writer = XmlWriter.Create(filename, Export.WriterSettings)) {
						export.GetXml().WriteTo(writer);
					}
				}
				fileChooser.Destroy();

				progressbarTotal.Text = "Готово";
				progressbarTotal.Adjustment.Value = progressbarTotal.Adjustment.Upper;
			}
		}
	}
}
