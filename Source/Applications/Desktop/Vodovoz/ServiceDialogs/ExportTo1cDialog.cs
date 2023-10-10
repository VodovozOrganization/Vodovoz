using System;
using System.Globalization;
using System.Linq;
using System.Xml;
using Gtk;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QSProjectsLib;
using Vodovoz.Domain.Organizations;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.ExportTo1c;
using Vodovoz.Extensions;
using Vodovoz.Infrastructure;
using Vodovoz.Parameters;

namespace Vodovoz
{
    public partial class ExportTo1cDialog : QS.Dialog.Gtk.TdiTabBase
    {
        bool exportInProgress;
        private readonly IParametersProvider _parametersProvider = new ParametersProvider();

        public ExportTo1cDialog(IUnitOfWorkFactory unitOfWorkFactory)
        {
            Build();
            TabName = "Выгрузка в 1с 8.3";

            comboOrganization.ItemsList = unitOfWorkFactory.CreateWithoutRoot().GetAll<Organization>();
            
            buttonExportBookkeeping.Clicked += (sender, args) => Export(Export1cMode.BuhgalteriaOOO);
            buttonExportIPTinkoff.Clicked += (sender, args) => Export(Export1cMode.IPForTinkoff);
            
            ybuttonExportBookkeepingNew.Clicked += (sender, args) => {
                if(comboOrganization.SelectedItem is Organization) {
                    Export(Export1cMode.BuhgalteriaOOONew);
                }
                else {
                    MessageDialogHelper.RunWarningDialog("Для этой выгрузки необходимо выбрать организацию");
                }
            };
        }

        private ExportData exportData;

        private void Export(Export1cMode mode)
        {
            var dateStart = dateperiodpicker1.StartDate;
            var dateEnd = dateperiodpicker1.EndDate;

            int? organizationId = null;
            if(mode == Export1cMode.BuhgalteriaOOONew) {
                organizationId = (comboOrganization.SelectedItem as Organization)?.Id;
            }
            else if(mode == Export1cMode.BuhgalteriaOOO) {
                organizationId = new OrganizationParametersProvider(_parametersProvider).VodovozOrganizationId;
            }

            using(var exportOperation = new ExportOperation(
                mode,
                new OrderParametersProvider(_parametersProvider),
                dateStart,
                dateEnd,
                organizationId
                ))
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

            labelTotalSales.Text = (exportData.Objects
                .OfType<SalesDocumentNode>()
                .Count()
				+ exportData.Objects.OfType<RetailDocumentNode>().Count())
                .ToString();

            labelTotalSum.Text = exportData.OrdersTotalSum.ToString("C", CultureInfo.GetCultureInfo("ru-RU"));

            labelExportedSum.Markup =
                $"<span foreground=\"{(exportData.ExportedTotalSum == exportData.OrdersTotalSum ? GdkColors.SuccessText.ToHtmlColor() : GdkColors.DangerText.ToHtmlColor())}\">" +
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
            buttonExportBookkeeping.Sensitive = buttonExportIPTinkoff.Sensitive
                = ybuttonExportBookkeepingNew.Sensitive = comboOrganization.Sensitive
                    = !exportInProgress
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
