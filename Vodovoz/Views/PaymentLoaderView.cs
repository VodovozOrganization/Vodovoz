using System;
using Gtk;
using Vodovoz.ViewModels;
using Gamma.ColumnConfig;
using QS.Views.GtkUI;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Payments;
using System.Linq;
using QSProjectsLib;

namespace Vodovoz.Views
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class PaymentLoaderView : TabViewBase<PaymentLoaderVM>
	{
		static Gdk.Color colorPink = new Gdk.Color(0xff, 0xc0, 0xc0);
		static Gdk.Color colorWhite = new Gdk.Color(0xff, 0xff, 0xff);
		static Gdk.Color colorLightGreen = new Gdk.Color(0xc0, 0xff, 0xc0);

		static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
		IUnitOfWork UoW => ViewModel.UoW;

		public PaymentLoaderView(PaymentLoaderVM paymentLoaderVM) : base(paymentLoaderVM)
		{
			this.Build();
			ViewModel.TabName = "Выгрузка выписки из банк-клиента";
			ConfigureDlg();
		}

		private void ConfigureDlg()
		{
			var txtFilter = new FileFilter();
			txtFilter.AddPattern("*.txt");
			txtFilter.Name = "Текстовые файлы (*.txt)";
			var allFilter = new FileFilter();
			allFilter.AddPattern("*");
			allFilter.Name = "Все файлы";
			fileChooserBtn.AddFilter(txtFilter);
			fileChooserBtn.AddFilter(allFilter);

			btnUpload.Clicked += (sender, e) => ViewModel.SaveCommand.Execute(ViewModel.ObservablePayments);
			btnUpload.Binding.AddBinding(ViewModel, v => v.IsNotAutoMatchingMode, w => w.Sensitive).InitializeFromSource();
			btnCancel.Clicked += BtnCancel_Clicked;

			ViewModel.UpdateProgress += UpdateProgress;

			btnReadFile.Clicked += (sender, e) => { ViewModel.Init(fileChooserBtn.Filename);};
			btnReadFile.Sensitive = !string.IsNullOrWhiteSpace(fileChooserBtn.Filename);

			treeDocuments.ColumnsConfig = FluentColumnsConfig<Payment>.Create()
				.AddColumn("№").AddTextRenderer(x => x.PaymentNum.ToString())
				.AddColumn("Дата").AddTextRenderer(x => x.Date.ToShortDateString())
				.AddColumn("Cумма").AddTextRenderer(x => x.Total.ToString())
				.AddColumn("Заказы").AddTextRenderer(x => x.NumOrders)
				.AddColumn("Плательщик").AddTextRenderer(x => x.CounterpartyName)
					.WrapWidth(450).WrapMode(Pango.WrapMode.WordChar)
				.AddColumn("Получатель").AddTextRenderer(x => x.Organization.FullName)
				.AddColumn("Назначение платежа").AddTextRenderer(x => x.PaymentPurpose)
					.WrapWidth(600).WrapMode(Pango.WrapMode.WordChar)
				.AddColumn("Категория дохода/расхода")
					.AddComboRenderer(x => x.ProfitCategory)
					.SetDisplayFunc(x => x.Name)
					.FillItems(UoW.GetAll<CategoryProfit>().ToList())
					.Editing()
				//.AddColumn("")
				.RowCells().AddSetter<CellRenderer>(
					(c, n) => {
						var color = colorLightGreen;
						if(n.Status != PaymentState.distributed)
							color = colorPink;
						c.CellBackgroundGdk = color;})
				.Finish();

			treeDocuments.Binding.AddBinding(ViewModel, vm => vm.ObservablePayments, w => w.ItemsDataSource).InitializeFromSource();
			ViewModel.ObservablePayments.ListChanged += (aList) => treeDocuments.YTreeModel.EmitModelChanged();
		}

		protected void OnFilechooserSelectionChanged(object sender, EventArgs e)
		{
			btnReadFile.Sensitive = !string.IsNullOrWhiteSpace(fileChooserBtn.Filename);
		}

		private void UpdateProgress(string msg, double progress)
		{
			if(progress == 0)
				progressbar1.Fraction = 0;

			progressbar1.Fraction += progress;
			progressbar1.Text = msg;
			QSMain.WaitRedraw();
		}

		void BtnCancel_Clicked(object sender, EventArgs e)
		{
			ViewModel.Close(false);
		}
	}
}
