using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml;
using Autofac;
using ExportTo1c.Library;
using ExportTo1c.Library.ExportNodes;
using Gtk;
using QS.Dialog.Gtk;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QSProjectsLib;
using Vodovoz.Domain.Organizations;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.Extensions;
using Vodovoz.Infrastructure;
using Vodovoz.ServiceDialogs.ExportTo1c;
using Vodovoz.Settings.Orders;
using Vodovoz.Settings.Organizations;

namespace Vodovoz
{
	public partial class ExportTo1cDialog : TdiTabBase
    {
        bool exportInProgress;
		private ExportData exportData;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;

		public ExportTo1cDialog(IUnitOfWorkFactory unitOfWorkFactory)
        {
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));

			Build();
            TabName = "Выгрузка в 1с 8.3";

            comboOrganization.ItemsList = unitOfWorkFactory.CreateWithoutRoot().GetAll<Organization>();
            
            buttonExportBookkeeping.Clicked += (sender, args) => Export(Export1cMode.BuhgalteriaOOO);
            ybuttonComplexAutomation1CExport.Clicked += (sender, args) => Export(Export1cMode.ComplexAutomation);
            buttonExportIPTinkoff.Clicked += (sender, args) => Export(Export1cMode.IPForTinkoff);
            
            ybuttonExportBookkeepingNew.Clicked += (sender, args) => {
                if(comboOrganization.SelectedItem is Organization) {
                    Export(Export1cMode.BuhgalteriaOOONew);
                }
                else {
                    MessageDialogHelper.RunWarningDialog("Для этой выгрузки необходимо выбрать организацию");
                }
            };
            
            UpdateExportSensitivity();
		}



		private void Export(Export1cMode mode)
        {
			var organizationSettings = ScopeProvider.Scope.Resolve<IOrganizationSettings>();
            var dateStart = dateperiodpicker1.StartDate;
			var dateEnd = dateperiodpicker1.EndDate;

            int? organizationId = null;
            if(mode == Export1cMode.BuhgalteriaOOONew) {
                organizationId = (comboOrganization.SelectedItem as Organization)?.Id;
            }
            else if(mode == Export1cMode.BuhgalteriaOOO ||  mode == Export1cMode.ComplexAutomation) {
                organizationId = organizationSettings.VodovozOrganizationId;
            }

            using(var exportOperation = new ExportOperation(
                mode,
				ScopeProvider.Scope.Resolve<IOrderSettings>(),
				ScopeProvider.Scope.Resolve<IOrderRepository>(),
				_unitOfWorkFactory,
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
                Encoding = Encoding.UTF8,
                NewLineChars = "\r\n"
            };
            var fileChooser = new FileChooserDialog("Выберите файл для сохранения выгрузки",
                (Window)this.Toplevel,
                FileChooserAction.Save,
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
	        var canExport =
		        !exportInProgress
		        && dateperiodpicker1.EndDateOrNull != null
		        && dateperiodpicker1.StartDateOrNull != null
		        && dateperiodpicker1.StartDate <= dateperiodpicker1.EndDate;

	        comboOrganization.Sensitive = canExport;
	        buttonExportBookkeeping.Sensitive = canExport;
	        ybuttonComplexAutomation1CExport.Sensitive = canExport;
	        buttonExportIPTinkoff.Sensitive = canExport;
	        ybuttonExportBookkeepingNew.Sensitive = canExport;
        }

        protected void OnDateperiodpicker1PeriodChanged(object sender, EventArgs e)
        {
            UpdateExportSensitivity();
        }
    }
}
