using Gamma.ColumnConfig;
using Gtk;
using QS.Views.GtkUI;
using System;
using System.Threading.Tasks;
using Vodovoz.ViewModels.ViewModels.Reports.EdoUpdReport;
using WrapMode = Pango.WrapMode;

namespace Vodovoz.Views.Reports
{
	public partial class EdoUpdReportView : TabViewBase<EdoUpdReportViewModel>
	{
		public EdoUpdReportView(EdoUpdReportViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			rangeDate.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.DateFrom, w => w.StartDateOrNull)
				.AddBinding(vm => vm.DateTo, w => w.EndDateOrNull)
				.InitializeFromSource();

			yenumcomboboxReportType.ItemsEnum = typeof(EdoUpdReportViewModel.EdoUpdReportType);
			yenumcomboboxReportType.Binding.AddBinding(ViewModel, s => s.ReportType, w => w.SelectedItem).InitializeFromSource();

			speccomboOrganization.SetRenderTextFunc<Domain.Organizations.Organization>(o => o.Name);
			speccomboOrganization.ItemsList = ViewModel.Organizations;
			speccomboOrganization.Binding
				.AddBinding(ViewModel, x => x.Organization, x => x.SelectedItem)
				.InitializeFromSource();

			ybuttonCreateReport.Binding
				.AddFuncBinding(ViewModel, vm => !vm.IsRunning, w => w.Sensitive)
				.InitializeFromSource();

			ybuttonCreateReport.Clicked += OnYbtnRunReportClicked;

			ybuttonSave.Binding
				.AddFuncBinding(ViewModel, vm => !vm.IsRunning, w => w.Sensitive)
				.InitializeFromSource();

			ybuttonSave.Clicked += (s, e) => ViewModel.ExportCommand.Execute();

			ConfigureReportTreeView();
		}

		private async void OnYbtnRunReportClicked(object sender, System.EventArgs e)
		{
			SetRunningState(true);

			await Task.Run(() =>
			{
				try
				{
					ViewModel.GenerateCommand.Execute();
				}
				catch(Exception ex)
				{
					Gtk.Application.Invoke((s, eventArgs) => throw ex);
				}

				SetRunningState(false);

				Gtk.Application.Invoke((s, a) =>
				{
					ytreeviewReport.ItemsDataSource = ViewModel.Report.Rows;
					ytreeviewReport.YTreeModel.EmitModelChanged();

				});
			});
		}

		private void ConfigureReportTreeView()
		{
			ytreeviewReport.ColumnsConfig = FluentColumnsConfig<EdoUpdReportRow>.Create()
				.AddColumn("№").AddNumericRenderer(r => ViewModel.Report.Rows.IndexOf(r) + 1)
				.AddColumn("ИНН").AddTextRenderer(r => r.Inn)
				.AddColumn("Название контрагента").AddTextRenderer(r => r.CounterpartyName).WrapWidth(300).WrapMode(WrapMode.WordChar)
				.AddColumn("№ Заказа").AddNumericRenderer(r => r.OrderId)
				.AddColumn("Дата").AddTextRenderer(r => r.UpdDateString)
				.AddColumn("GTIN").AddTextRenderer(r => r.Gtin)
				.AddColumn("Кол-во").AddNumericRenderer(r => r.Count)
				.AddColumn("Цена").AddNumericRenderer(r => r.Price)
				.AddColumn("Стоимость\nстроки с НДС").AddNumericRenderer(r => r.Sum)
				.AddColumn("Статус УПД в ЭДО").AddTextRenderer(r => r.EdoDocFlowStatusString).WrapWidth(300).WrapMode(WrapMode.WordChar)
				.AddColumn("Статус прямого вывода из\nоборота в Честном Знаке").AddTextRenderer(r => r.TrueMarkApiStatusString).WrapWidth(300).WrapMode(WrapMode.WordChar)
				.AddColumn("")
				.Finish();
		}

		protected void OnEventboxArrowButtonPressEvent(object o, ButtonPressEventArgs args)
		{
			parametersContainer.Visible = !parametersContainer.Visible;
			UpdateSliderArrow();
		}

		private void UpdateSliderArrow()
		{
			arrowSlider.ArrowType = parametersContainer.Visible ? ArrowType.Left : ArrowType.Right;
		}

		private void SetRunningState(bool isRunning)
		{
			Gtk.Application.Invoke((s, a) =>
			{
				ViewModel.IsRunning = isRunning;
			});
		}
	}
}
