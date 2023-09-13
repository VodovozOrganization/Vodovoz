using System;
using System.Globalization;
using System.Linq;
using System.Xml;
using Gtk;
using QS.DomainModel.UoW;
using QSProjectsLib;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.Extensions;
using Vodovoz.Infrastructure;
using Vodovoz.Parameters;

namespace Vodovoz.Old1612ExportTo1c
{
    public partial class Old1612ExportTo1cDialog : QS.Dialog.Gtk.TdiTabBase
    {
        bool exportInProgress;

        public Old1612ExportTo1cDialog(IUnitOfWorkFactory unitOfWorkFactory)
        {
            Build();
            TabName = "Выгрузка в 1с 8.3 (до 16.12.2020)";            
            buttonExportBookkeeping.Clicked += (sender, args) => Export(Export1cMode.BuhgalteriaOOO);            
        }

        private ExportData exportData;

        private void Export(Export1cMode mode)
        {
            var dateStart = dateperiodpicker1.StartDate;
            var dateEnd = dateperiodpicker1.EndDate;

            using(var exportOperation = new ExportOperation(
                mode,
                new OrderParametersProvider(new ParametersProvider()),
                dateStart,
                dateEnd,
                null))
            {
                this.exportInProgress = true;
                UpdateExportSensitivity();
                LongOperationDlg.StartOperation(exportOperation.Run, "", 1, false);
                this.exportInProgress = false;
                UpdateExportSensitivity();
                exportData = exportOperation.Result;
            }

            labelTotalCounterparty.Text = exportData.Objects
                .OfType<CatalogObjectNode>()
                .Count(node => node.Type == Common1cTypes.ReferenceCounterparty)
                .ToString();

            labelTotalNomenclature.Text = exportData.Objects
                .OfType<CatalogObjectNode>()
                .Count(node => node.Type == Common1cTypes.ReferenceNomenclature)
                .ToString();

            labelTotalSales.Text = exportData.Objects
                .OfType<SalesDocumentNode>()
                .Count()
                .ToString();

            labelTotalSum.Text = exportData.OrdersTotalSum.ToString("C", CultureInfo.GetCultureInfo("ru-RU"));

            labelExportedSum.Markup =
                $"<span foreground=\"{(exportData.ExportedTotalSum == exportData.OrdersTotalSum ? GdkColors.Green.ToHtmlColor() : GdkColors.Red.ToHtmlColor())}\">" +
                $"{exportData.ExportedTotalSum.ToString("C", CultureInfo.GetCultureInfo("ru-RU"))}</span>";

            labelTotalInvoices.Text = exportData.Objects
                .OfType<InvoiceDocumentNode>()
                .Count()
                .ToString();

            GtkScrolledWindowErrors.Visible = exportData.Errors.Count > 0;
            if(exportData.Errors.Count > 0) {
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

        protected void OnButtonSaveClicked(object sender, EventArgs e)
        {
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
            var dateText = exportData.EndPeriodDate.ToShortDateString().Replace(System.IO.Path.DirectorySeparatorChar, '-');

            fileChooser.CurrentName = $"Выгрузка 1с на {dateText}.xml";
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

        private void UpdateExportSensitivity()
        {
            buttonExportBookkeeping.Sensitive = !exportInProgress
                    && dateperiodpicker1.EndDateOrNull != null
                    && dateperiodpicker1.StartDateOrNull != null
                    && dateperiodpicker1.StartDate <= dateperiodpicker1.EndDate;
        }

        protected void OnDateperiodpicker1PeriodChanged(object sender, EventArgs e)
        {
            UpdateExportSensitivity();
        }
    }
}
