using System;
using System.Linq;
using System.Xml;
using Gtk;
using QSProjectsLib;
using QS.Tdi;
using Vodovoz.ExportTo1c;

namespace Vodovoz
{
	public partial class ExportTo1cDialog : QS.Dialog.Gtk.TdiTabBase
	{
		bool exportInProgress;

		public ExportTo1cDialog()
		{
			this.Build();
			this.TabName="Выгрузка в 1с";
		}

		private ExportData exportData;

		protected void OnButtonExportClicked (object sender, EventArgs e)
		{
			var dateStart = dateperiodpicker1.StartDate;
			var dateEnd = dateperiodpicker1.EndDate;
			var exportOperation = new ExportOperation(dateStart, dateEnd);

			this.exportInProgress = true;
			UpdateExportButtonSensitivity();
			LongOperationDlg.StartOperation(exportOperation.Run, "", 1, false);
			this.exportInProgress = false;
			UpdateExportButtonSensitivity();

			exportData = exportOperation.Result;
			this.labelTotalCounterparty.Text = exportData.Objects
				.OfType<CatalogObjectNode>()
				.Count(node => node.Type == Common1cTypes.ReferenceCounterparty)
				.ToString();
			this.labelTotalNomenclature.Text = exportData.Objects
				.OfType<CatalogObjectNode>()
				.Count(node => node.Type == Common1cTypes.ReferenceNomenclature)
				.ToString();
			this.labelTotalSales.Text = exportData.Objects
				.OfType<SalesDocumentNode>()
				.Count()
				.ToString();
			this.labelTotalSum.Text = exportData.OrdersTotalSum.ToString("C");
			this.labelExportedSum.Markup = String.Format("<span foreground=\"{1}\">{0:C}</span>", 
			                                             exportData.ExportedTotalSum,
			                                             exportData.ExportedTotalSum == exportData.OrdersTotalSum ? "green" : "red");

			this.labelTotalInvoices.Text = exportData.Objects
				.OfType<InvoiceDocumentNode>()
				.Count()
				.ToString();

			GtkScrolledWindowErrors.Visible = exportData.Errors.Count > 0;
			if(exportData.Errors.Count > 0)
			{
				TextTagTable textTags = new TextTagTable();
				var tag = new TextTag("Red");
				tag.Foreground = "red";
				textTags.Add(tag);
				TextBuffer tempBuffer = new TextBuffer(textTags);
				TextIter iter = tempBuffer.EndIter;
				tempBuffer.InsertWithTags(ref iter, String.Join("\n", exportData.Errors), tag);
				textviewErrors.Buffer = tempBuffer;
			}

			buttonSave.Sensitive = exportData != null && exportData.Errors.Count == 0;
		}			

		protected void OnButtonSaveClicked (object sender, EventArgs e)
		{			
			var settings = new XmlWriterSettings
			{
				OmitXmlDeclaration = true,
				Indent = true,
				Encoding = System.Text.Encoding.UTF8,
				NewLineChars = "\r\n"
			};			
			var fileChooser = new Gtk.FileChooserDialog("Выберите файл для сохранения выгрузки",
				(Window)this.Toplevel,
				Gtk.FileChooserAction.Save,
				"Отмена",ResponseType.Cancel,
				"Сохранить",ResponseType.Accept
			);
			var dateText = exportData.EndPeriodDate.ToShortDateString().Replace(System.IO.Path.DirectorySeparatorChar, '-');

			fileChooser.CurrentName = $"Выгрузка 1с на {dateText}.xml";
			var filter = new FileFilter();
			filter.AddPattern("*.xml");
			fileChooser.Filter = filter;
			if (fileChooser.Run() == (int)ResponseType.Accept)
			{
				var filename = fileChooser.Filename.EndsWith(".xml") ? fileChooser.Filename : fileChooser.Filename + ".xml";
				using (XmlWriter writer = XmlWriter.Create(filename, settings))
				{				
					exportData.ToXml().WriteTo(writer);
				}
			}
			fileChooser.Destroy();
		}

		private void UpdateExportButtonSensitivity(){
			buttonExport.Sensitive = !exportInProgress 
				&& dateperiodpicker1.EndDateOrNull != null 
				&& dateperiodpicker1.StartDateOrNull != null 
				&& dateperiodpicker1.StartDate <= dateperiodpicker1.EndDate;
		}

		protected void OnDateperiodpicker1PeriodChanged (object sender, EventArgs e)
		{
			UpdateExportButtonSensitivity();
		}
	}
}

