using Gamma.ColumnConfig;
using Gtk;
using QS.Views.Dialog;
using System.Linq;
using Vodovoz.ViewModels.Orders.Reports.PotentialFreePromosets;
using static Vodovoz.ViewModels.Orders.Reports.PotentialFreePromosets.PotentialFreePromosetsReport;
using static Vodovoz.ViewModels.Orders.Reports.PotentialFreePromosets.PotentialFreePromosetsReportViewModel;
namespace Vodovoz.Orders.Reports
{
	public partial class PotentialFreePromosetsReportView : DialogViewBase<PotentialFreePromosetsReportViewModel>
	{
		private int _hpanedDefaultPosition = 428;
		private int _hpanedMinimalPosition = 16;

		public PotentialFreePromosetsReportView(PotentialFreePromosetsReportViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			ytreeviewPromosets.ColumnsConfig = FluentColumnsConfig<PromosetNode>.Create()
				.AddColumn("Выбрать").AddToggleRenderer(x => x.IsSelected)
				.AddColumn("Промонабор").AddTextRenderer(x => x.Name)
				.Finish();
			ytreeviewPromosets.ItemsDataSource = ViewModel.PromotionalSets;

			dateperiodpicker.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.StartDate, w => w.StartDateOrNull)
				.AddBinding(vm => vm.EndDate, w => w.EndDateOrNull)
				.InitializeFromSource();

			ybuttonGenerate.Binding
				.AddFuncBinding(ViewModel, vm => !vm.IsReportGenerationInProgress, w => w.Visible)
				.InitializeFromSource();

			ybuttonAbortCreateReport.Binding
				.AddFuncBinding(ViewModel, vm => vm.IsReportGenerationInProgress, w => w.Visible)
				.InitializeFromSource();

			ybuttonSave.Binding
				.AddFuncBinding(ViewModel, vm => vm.Report != null, w => w.Sensitive)
				.InitializeFromSource();

			ybuttonGenerate.BindCommand(ViewModel.GenerateReportCommand);
			ybuttonAbortCreateReport.BindCommand(ViewModel.AbortReportGenerationCommand);
			ybuttonSave.BindCommand(ViewModel.SaveReportCommand);

			ytreeData.ColumnsConfig = FluentColumnsConfig<PromosetReportRow>.Create()
				.AddColumn("№ п/п").AddNumericRenderer(x => x.SequenceNumber)
				.AddColumn("Адрес").AddTextRenderer(x => x.Address)
				.AddColumn("Тип объекта").AddTextRenderer(x => x.AddressType)
				.AddColumn("Телефон").AddTextRenderer(x => x.Phone)
				.AddColumn("Клиент").AddTextRenderer(x => x.Client)
				.AddColumn("Заказ").AddNumericRenderer(x => x.Order)
				.AddColumn("Дата создания").AddNumericRenderer(x => x.OrderCreationDate.ToShortDateString())
				.AddColumn("Дата доставки").AddNumericRenderer(x => x.OrderDeliveryDate.HasValue ? x.OrderDeliveryDate.Value.ToShortDateString() : string.Empty)
				.AddColumn("Промонабор").AddTextRenderer(x => x.Promoset)
				.AddColumn("Автор").AddTextRenderer(x => x.Author)
				.Finish();

			ytreeData.Binding
				.AddSource(ViewModel)
				.AddFuncBinding(vm => vm.Report != null, w => w.Visible)
				.AddFuncBinding(vm => vm.Report != null ? vm.Report.ReportRows : Enumerable.Empty<PromosetReportRow>(), w => w.ItemsDataSource)
				.InitializeFromSource();

			eventboxArrow.ButtonPressEvent += OnEventboxArrowButtonPressEvent;

			UpdateSliderArrow();
		}

		protected void OnEventboxArrowButtonPressEvent(object o, ButtonPressEventArgs args)
		{
			vboxReportParameters.Visible = !vboxReportParameters.Visible;

			hpanedMain.Position = vboxReportParameters.Visible ? _hpanedDefaultPosition : _hpanedMinimalPosition;

			UpdateSliderArrow();
		}

		private void UpdateSliderArrow()
		{
			arrowSlider.ArrowType = vboxReportParameters.Visible ? ArrowType.Left : ArrowType.Right;
		}
	}
}
