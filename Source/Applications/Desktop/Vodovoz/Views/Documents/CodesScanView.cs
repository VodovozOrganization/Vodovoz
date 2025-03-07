using System;
using System.Linq;
using Gamma.Binding;
using Gamma.ColumnConfig;
using Gdk;
using Gtk;
using QS.Views.Dialog;
using Vodovoz.Infrastructure;
using Vodovoz.ViewModels.ViewModels.Documents.SelfDeliveryCodesScan;
using WrapMode = Pango.WrapMode;

namespace Vodovoz.Views.Documents
{
	[WindowSize(1280, 1024)]
	public partial class CodesScanView : DialogViewBase<CodesScanViewModel>
	{
		private readonly Color _colorGreen = GdkColors.SuccessBase;
		private readonly Color _colorLightRed = GdkColors.DangerBase;
		private readonly Color _colorBase = GdkColors.PrimaryBase;
		private readonly Color _colorAggregate = GdkColors.InsensitiveBase;

		public CodesScanView(CodesScanViewModel viewModel) : base(viewModel)
		{
			Build();
			ConfigureView();
		}

		private void ConfigureView()
		{
			yentryCode.FocusOutEvent += OnYentryCodeOnFocusOutEvent;

			yentryCode.TextInserted += OnYentryCodeOnTextInserted;

			treeViewCodes.ColumnsConfig = FluentColumnsConfig<CodesScanViewModel.CodeScanRow>.Create()
				.AddColumn("№")
				.AddTextRenderer(n =>
					n.Parent == null && n.Children.Any()
						? $"{n.RowNumber} ╗"
						: n.Parent != null
							? "╠"
							: $"{n.RowNumber}.   ")
				.XAlign(1)
				.AddColumn("Код")
				.AddTextRenderer(n => n.CodeNumber)
				.AddSetter((c, n) =>
					c.CellBackgroundGdk = n.Children.Any() ? _colorAggregate : _colorBase)
				.AddColumn("Номенклатура")
				.AddTextRenderer(n => n.NomenclatureName)
				.WrapMode(WrapMode.Word).WrapWidth(400)
				.AddColumn("Наличие в заказе")
				.AddTextRenderer(n => n.HasInOrder.HasValue ? (n.HasInOrder.Value ? "Да" : "Нет") : "")
				.AddSetter((c, n) =>
					c.CellBackgroundGdk = n.Children.Any() ? _colorAggregate :
						n.HasInOrder.HasValue ? n.HasInOrder.Value ? _colorGreen : _colorLightRed : _colorBase)
				.AddColumn("Валиден в ЧЗ")
				.AddTextRenderer(n => n.IsTrueMarkValid.HasValue ? (n.IsTrueMarkValid.Value ? "Да" : "Нет") : "")
				.AddSetter((c, n) =>
					c.CellBackgroundGdk = n.Children.Any() ? _colorAggregate :
						n.IsTrueMarkValid.HasValue ? n.IsTrueMarkValid.Value ? _colorGreen : _colorLightRed : _colorBase)
				.AddSetter((c, n) =>
					c.CellBackgroundGdk = n.Children.Any() ? _colorAggregate :
						n.IsTrueMarkValid.HasValue ? n.IsTrueMarkValid.Value ? _colorGreen : _colorLightRed : _colorBase)
				.AddColumn("Доп.информация")
				.AddTextRenderer(n => n.AdditionalInformation)
				.WrapMode(WrapMode.Word).WrapWidth(400)
				.AddColumn("")
				.Finish();

			treeViewCodes.YTreeModel =
				new RecursiveTreeModel<CodesScanViewModel.CodeScanRow>(ViewModel.CodeScanRows, x => x.Parent,
					x => x.Children);

			ytreeviewProgress.ColumnsConfig = FluentColumnsConfig<CodesScanViewModel.CodesScanProgressRow>.Create()
				.AddColumn("GTIN")
				.AddTextRenderer(n => n.Gtin)
				.AddColumn("Номенклатура")
				.AddTextRenderer(n => n.NomenclatureName)
				.WrapMode(WrapMode.Word).WrapWidth(400)
				.AddColumn("Осталось отсканировать")
				.AddNumericRenderer(n => n.LeftToScan)
				.AddSetter((c, n) => c.CellBackgroundGdk = n.LeftToScan == 0 ?  _colorGreen : _colorBase)
				.AddColumn("В самовывозе")
				.AddNumericRenderer(n => n.InSelfDelivery)
				.AddColumn("")
				.Finish();

			ytreeviewProgress.ItemsDataSource = ViewModel.CodesScanProgressRows;
			
			ybuttonOk.Binding.AddBinding(ViewModel, vm => vm.IsAllCodesScanned, w => w.Sensitive).InitializeFromSource();
			ybuttonOk.BindCommand(ViewModel.CloseCommand);

			ViewModel.RefreshScanningNomenclaturesAction = OnRefreshScanningNomenclatures;
		}

		private void OnRefreshScanningNomenclatures()
		{
				treeViewCodes.YTreeModel?.EmitModelChanged();
				treeViewCodes.ExpandAll();
				ytreeviewProgress.YTreeModel?.EmitModelChanged();
		}


		private void OnYentryCodeOnTextInserted(object o, TextInsertedArgs args)
		{
			yentryCode.TextInserted -= OnYentryCodeOnTextInserted;

			if(args.Text.Length < 20 || args.Text.Length > 55)
			{
				yentryCode.Text = string.Empty;
				yentryCode.TextInserted += OnYentryCodeOnTextInserted;
				return;
			}

			yentryCode.Text = args.Text;

			yentryCode.TextInserted += OnYentryCodeOnTextInserted;

			ViewModel.CheckCode(args.Text);

			yentryCode.Text = string.Empty;
		}

		private void OnYentryCodeOnFocusOutEvent(object o, FocusOutEventArgs args)
		{
			yentryCode.GrabFocus();
		}

		public override void Destroy()
		{
			yentryCode.FocusOutEvent -= OnYentryCodeOnFocusOutEvent;
			yentryCode.TextInserted -= OnYentryCodeOnTextInserted;

			base.Destroy();
		}
	}
}
