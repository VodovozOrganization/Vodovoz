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
				var fileChooser = new Gtk.FileChooserDialog("Выберите папку для сохранения выгрузки",
					(Window)this.Toplevel,
				                                            Gtk.FileChooserAction.SelectFolder,
					"Отмена", ResponseType.Cancel,
					"Выбрать", ResponseType.Accept
				);

				if(fileChooser.Run() == (int)ResponseType.Cancel)
				{
					fileChooser.Destroy();
					return;
				}

				var directory = fileChooser.Filename;
				fileChooser.Destroy();

				var export = new Export(uow);
				export.ProgressUpdated += Export_ProgressUpdated;

				export.RunToDirectory(directory);

				if(UpdateErrors(export.Errors))
					return;

                UpdateResults(export.Results);
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

                UpdateResults(export.Results);
				progressbarTotal.Text = "Готово";
				progressbarTotal.Adjustment.Value = progressbarTotal.Adjustment.Upper;
			}
		}


		void Export_ProgressUpdated(object sender, EventArgs e)
		{
			var export = sender as Export;
			progressbarTotal.Text = export.CurrentTaskText + export.CurrentStepText;
			progressbarTotal.Adjustment.Upper = export.TotalTasks;
			progressbarTotal.Adjustment.Value = export.CurrentTask;
            UpdateErrors(export.Errors);
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

		private void UpdateResults(List<string> results)
        {
            GtkScrolledWindowErrors.Visible = true;
            TextTagTable textResultTags = new TextTagTable();
            var tagResult = new TextTag("blue");
            tagResult.Foreground = "blue";
            textResultTags.Add(tagResult);
            TextBuffer tempBuffer = new TextBuffer(textResultTags);
            TextIter iter = tempBuffer.EndIter;
            tempBuffer.InsertWithTags(ref iter, String.Join("\n", results), tagResult);
            textviewErrors.Buffer = tempBuffer;
        }
    }
}