using System;
using Gamma.GtkWidgets;
using Gamma.Utilities;
using Gtk;
using QS.Navigation;
using QS.Views.GtkUI;
using Vodovoz.Domain.Payments;
using Vodovoz.ViewModels.ViewModels.Payments;

namespace Vodovoz.Views.Payments
{
    [System.ComponentModel.ToolboxItem(true)]
    public partial class ImportRegisterOfPaymentsFromYookassaView : TabViewBase<ImportRegisterOfPaymentsFromYookassaViewModel>
    {
        public ImportRegisterOfPaymentsFromYookassaView(
	        ImportRegisterOfPaymentsFromYookassaViewModel viewModel) : base(viewModel)
        {
            this.Build();
            ConfigureDlg();
        }
            
        void ConfigureDlg()
        {
            var csvFilter = new FileFilter();
            csvFilter.AddPattern("*.csv");
            csvFilter.Name = "Comma Separated Values File (*.csv)";
            var allFilter = new FileFilter();
            allFilter.AddPattern("*");
            allFilter.Name = "Все файлы";
            fChooser.AddFilter(csvFilter);
            fChooser.AddFilter(allFilter);

            treeDocuments.ColumnsConfig = ColumnsConfigFactory.Create<PaymentFromYookassa>()
            	.AddColumn("Загрузить")
            		.AddToggleRenderer(x => x.Selected).Editing()
            		.AddSetter((c, n) => (c as CellRendererToggle).Activatable = n.Selectable)
            	.AddColumn("Дата и\nвремя")
            		.AddTextRenderer(x => $"{x.PaymentTime:M}\n{x.PaymentTime:t}")
            	/*.AddColumn("Номер и\nсумма оплаты")
            		.AddTextRenderer(
            			x => $"{x.PaymentNr.ToString()}\n{CurrencyWorks.GetShortCurrencyString(x.Amount)}")
            	.AddColumn("Контакты")
            		.AddTextRenderer(x => $"{x.Phone}\n{x.Email}")
            	.AddColumn("Магазин")
            		.AddTextRenderer(x => x.Shop)
            	.AddColumn("Статус оплаты")*/
            	.AddTextRenderer(x => x.PaymentStatus.GetEnumTitle())
            	.AddColumn("")
            	.RowCells()
            		.AddSetter<CellRenderer>(
            			(c, n) => c.CellBackground = n.Color
            		)
            	.Finish();

            btnCancel.Clicked += (sender, args) => ViewModel.Close(false, CloseSource.Cancel);
            //btnUpload;
            btnReadFile.Clicked += (sender, args) => ViewModel.ReadFileCommand.Execute(fChooser.Filename);
            
            
            //SetControlsAccessibility();
        }
    }
}
