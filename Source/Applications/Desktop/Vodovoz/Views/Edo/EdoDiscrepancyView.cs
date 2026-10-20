using Gamma.Binding;
using Gamma.Binding.Core.RecursiveTreeConfig;
using Gamma.ColumnConfig;
using Pango;
using QS.ViewModels.Dialog;
using QS.Views.Dialog;
using QS.Views.GtkUI;
using System;
using Vodovoz.ViewModels.Edo;
using static Org.BouncyCastle.Math.EC.ECCurve;
using static Vodovoz.Cash.Reports.CashFlowAnalysisView;
namespace Vodovoz.Views.Edo
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class EdoDiscrepancyView : DialogViewBase<EdoDiscrepancyViewModel>
	{
		public EdoDiscrepancyView(EdoDiscrepancyViewModel viewModel) : base(viewModel)
		{
			this.Build();
			ConfigureDlg();
		}

		protected void ConfigureDlg()
		{
			yentryErpDocflowStatus.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.ErpDocflowStatus, w => w.Text)
				.InitializeFromSource();

			yentryErpDocflowTime.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.ErpDocflowTime, w => w.Text)
				.InitializeFromSource();

			yentryOrderId.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.OrderId, w => w.Text)
				.InitializeFromSource();

			yentryOrderStatus.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.OrderStatus, w => w.Text)
				.InitializeFromSource();

			yentryDeliveryDate.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.DeliveryDate, w => w.Text)
				.InitializeFromSource();

			ylabelOrderDiscrepancy.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.Discrepancy, w => w.LabelProp)
				.InitializeFromSource();

			yentryTaxcomDocflowId.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.TaxcomDocflowId, w => w.Text)
				.InitializeFromSource();

			yentryTaxcomDocflowStatus.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.TaxcomDocflowStatus, w => w.Text)
				.InitializeFromSource();

			yentryTaxcomStatusTime.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.TaxcomStatusTime, w => w.Text)
				.InitializeFromSource();

			ytextviewTaxcomMessage.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.TaxcomClientMessage, w => w.Buffer.Text)
				.InitializeFromSource();

			ycheckOnlyDifference.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.OnlyDiffs, w => w.Active)
				.InitializeFromSource();

			ytreeviewUpdCompare.EnableGridLines = Gtk.TreeViewGridLines.Vertical;
			ytreeviewUpdCompare.EnableTreeLines = true;
			ytreeviewUpdCompare.ColumnsConfig = FluentColumnsConfig<UpdCompareViewModel>.Create()
				.AddColumn("Поле")
					.HeaderAlignment(0.5f)
					.MinWidth(200)
					.AddTextRenderer(x => x.Name)
					.XAlign(0f)
				.AddColumn("Значение в заказе")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => x.CurrentValue, useMarkup: true)
					.WrapMode(WrapMode.Word)
					.WrapWidth(600)
					.XAlign(0.5f)
				.AddColumn("Значение в ЭДО")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => x.SentValue, useMarkup: true)
					.WrapMode(WrapMode.Word)
					.WrapWidth(600)
					.XAlign(0.5f)
				.AddColumn("")
				.Finish();

			ViewModel.PropertyChanged += ViewModelPropertyChanged;

			ybuttonResend.BindCommand(ViewModel.ResendCommand);
			ybuttonCancelOrder.BindCommand(ViewModel.CancelOrderCommand);

			ShowUpdCompareFields();
		}

		private void ViewModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(ViewModel.UpdCompareFields))
			{
				ShowUpdCompareFields();
			}
		}

		private void ShowUpdCompareFields()
		{
			var config = new RecursiveConfig<UpdCompareViewModel>(
				x => x.ParentNode,
				x => x.ChildNodes
			);

			ytreeviewUpdCompare.YTreeModel = new RecursiveTreeModel<UpdCompareViewModel>(ViewModel.UpdCompareFields, config);
			ytreeviewUpdCompare.ExpandAll();
		}

		override protected void OnDestroyed()
		{
			ViewModel.PropertyChanged -= ViewModelPropertyChanged;
			base.OnDestroyed();
		}
	}
}
