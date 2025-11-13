using Gamma.ColumnConfig;
using Gtk;
using QS.Views.GtkUI;
using System.Linq;
using Vodovoz.ViewModels.Orders.Reports.PotentialFreePromosets;
using static Vodovoz.ViewModels.Orders.Reports.PotentialFreePromosets.PotentialFreePromosetsReport;
using static Vodovoz.ViewModels.Orders.Reports.PotentialFreePromosets.PotentialFreePromosetsReportViewModel;
using WrapMode = Pango.WrapMode;

namespace Vodovoz.Orders.Reports
{
	public partial class PotentialFreePromosetsReportView : TabViewBase<PotentialFreePromosetsReportViewModel>
	{
		private const int _hpanedDefaultPosition = 428;
		private const int _hpanedMinimalPosition = 16;

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

			ybuttonGenerate.BindCommand(ViewModel.GenerateReportCommand);
			ybuttonAbortCreateReport.BindCommand(ViewModel.AbortReportGenerationCommand);
			ybuttonSave.BindCommand(ViewModel.SaveReportCommand);

			ytreeData.ColumnsConfig = FluentColumnsConfig<PromosetReportRow>.Create()
				.AddColumn("№ п/п").AddNumericRenderer(x => x.SequenceNumber)
				.AddSetter((cell, node) =>
				{
					if(node.IsRootRow)
					{
						cell.Markup = $"<b>{node.SequenceNumber}</b>";
					}
				})
				.AddColumn("Адрес").AddTextRenderer(x => x.Address)
				.AddSetter((cell, node) =>
				{
					if(node.IsRootRow)
					{
						cell.Markup = $"<b>{node.Address}</b>";
					}
				})
				.WrapWidth(350).WrapMode(WrapMode.WordChar)
				.AddColumn("Тип объекта").AddTextRenderer(x => x.AddressCategory)
				.AddSetter((cell, node) =>
				{
					if(node.IsRootRow)
					{
						cell.Markup = $"<b>{node.AddressCategory}</b>";
					}
				})
				.WrapWidth(120).WrapMode(WrapMode.WordChar)
				.AddColumn("Телефон").AddTextRenderer(x => x.Phone)
				.AddSetter((cell, node) =>
				{
					if(node.IsRootRow)
					{
						cell.Markup = $"<b>{node.Phone}</b>";
					}
				})
				.WrapWidth(150).WrapMode(WrapMode.WordChar)
				.AddColumn("Клиент").AddTextRenderer(x => x.Client)
				.AddSetter((cell, node) =>
				{
					if(node.IsRootRow)
					{
						cell.Markup = $"<b>{node.Client}</b>";
					}
				})
				.WrapWidth(200).WrapMode(WrapMode.WordChar)
				.AddColumn("Заказ").AddNumericRenderer(x => x.Order)
				.AddSetter((cell, node) =>
				{
					if(node.IsRootRow)
					{
						cell.Markup = $"<b>{node.Order}</b>";
					}
				})
				.AddColumn("Дата создания").AddNumericRenderer(x => x.OrderCreationDate.HasValue ? x.OrderCreationDate.Value.ToShortDateString() : string.Empty)
				.AddSetter((cell, node) =>
				{
					var value = node.OrderCreationDate.HasValue ? node.OrderCreationDate.Value.ToShortDateString() : string.Empty;
					if(node.IsRootRow)
					{
						cell.Markup = $"<b>{value}</b>";
					}
				})
				.AddColumn("Дата доставки").AddNumericRenderer(x => x.OrderDeliveryDate.HasValue ? x.OrderDeliveryDate.Value.ToShortDateString() : string.Empty)
				.AddSetter((cell, node) =>
				{
					var value = node.OrderDeliveryDate.HasValue ? node.OrderDeliveryDate.Value.ToShortDateString() : string.Empty;
					if(node.IsRootRow)
					{
						cell.Markup = $"<b>{value}</b>";
					}
				})
				.AddColumn("Промонабор").AddTextRenderer(x => x.Promoset)
				.AddSetter((cell, node) =>
				{
					if(node.IsRootRow)
					{
						cell.Markup = $"<b>{node.Promoset}</b>";
					}
				})
				.WrapWidth(120).WrapMode(WrapMode.WordChar)
				.AddColumn("Автор").AddTextRenderer(x => x.Author)
				.AddSetter((cell, node) =>
				{
					if(node.IsRootRow)
					{
						cell.Markup = $"<b>{node.Author}</b>";
					}
				})
				.Finish();

			ytreeData.Binding
				.AddSource(ViewModel)
				.AddFuncBinding(vm => vm.Report != null, w => w.Visible)
				.AddFuncBinding(vm => vm.Report != null ? vm.Report.Rows : Enumerable.Empty<PromosetReportRow>(), w => w.ItemsDataSource)
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
