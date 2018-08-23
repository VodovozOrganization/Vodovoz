using System;
using System.Collections.Generic;
using System.Xml;
using Gtk;
using QSOrmProject;
using QSProjectsLib;
using QSSupportLib;
using Vodovoz.Tools.CommerceML;

namespace Vodovoz.Dialogs.OnlineStore
{
	public partial class ExportToSiteDlg : QSTDI.TdiTabBase
	{
		public ExportToSiteDlg()
		{
			this.Build();
			TabName = "Экспорт интернет магазин";
			if(MainSupport.BaseParameters.All.ContainsKey(Export.OnlineStoreUrlParameterName))
				entrySitePath.Text = MainSupport.BaseParameters.All[Export.OnlineStoreUrlParameterName];
			if(MainSupport.BaseParameters.All.ContainsKey(Export.OnlineStoreLoginParameterName))
				entryUser.Text = MainSupport.BaseParameters.All[Export.OnlineStoreLoginParameterName];
			if(MainSupport.BaseParameters.All.ContainsKey(Export.OnlineStorePasswordParameterName))
				entryPassword.Text = MainSupport.BaseParameters.All[Export.OnlineStorePasswordParameterName];
		}

		protected void OnButtonRunToFileClicked(object sender, EventArgs e)
		{
			using(var uow = UnitOfWorkFactory.CreateWithoutRoot())
			{
				var export = new Export(uow);
				export.ProgressUpdated += Export_ProgressUpdated;

				export.RunToDirectory();

				if(UpdateErrors(export.Errors))
					return;

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

		protected void OnEntrySitePathFocusOutEvent(object o, FocusOutEventArgs args)
		{
			MainSupport.BaseParameters.UpdateParameter(QSMain.ConnectionDB, Export.OnlineStoreUrlParameterName, entrySitePath.Text);
		}

		protected void OnEntryUserFocusOutEvent(object o, FocusOutEventArgs args)
		{
			MainSupport.BaseParameters.UpdateParameter(QSMain.ConnectionDB, Export.OnlineStoreLoginParameterName, entryUser.Text);
		}

		protected void OnEntryPasswordFocusOutEvent(object o, FocusOutEventArgs args)
		{
			MainSupport.BaseParameters.UpdateParameter(QSMain.ConnectionDB, Export.OnlineStorePasswordParameterName, entryPassword.Text);
		}

		protected void OnButtonExportToSiteClicked(object sender, EventArgs e)
		{
			using(var uow = UnitOfWorkFactory.CreateWithoutRoot()) {
				var export = new Export(uow);
				export.ProgressUpdated += Export_ProgressUpdated;

				export.RunToSite();

				if(UpdateErrors(export.Errors))
					return;

				progressbarTotal.Text = "Готово";
				progressbarTotal.Adjustment.Value = progressbarTotal.Adjustment.Upper;
			}
		}


		void Export_ProgressUpdated(object sender, EventArgs e)
		{
			var export = sender as Export;
			progressbarTotal.Text = export.CurrentTaskText;
			progressbarTotal.Adjustment.Upper = export.TotalTasks;
			progressbarTotal.Adjustment.Value = export.CurrentTask;
			QSMain.WaitRedraw();
		}

		/// <summary>
		/// Если метод вернул true, это значит что есть ошибки.
		/// </summary>
		private bool UpdateErrors(List<string> errors)
		{
			GtkScrolledWindowErrors.Visible = errors.Count > 0;
			if(errors.Count > 0) {
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