using Gamma.Binding;
using Gamma.Binding.Core.LevelTreeConfig;
using Gamma.ColumnConfig;
using Gtk;
using QS.Views.Dialog;
using Vodovoz.Core.Domain.Organizations;
using Vodovoz.Presentation.ViewModels.Organisations;
using Vodovoz.ViewWidgets.Profitability;

namespace Vodovoz.Views.Organization
{
	public partial class CompanyBalanceByDateView : DialogViewBase<CompanyBalanceByDateViewModel>
	{
		public CompanyBalanceByDateView(CompanyBalanceByDateViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			btnSave.BindCommand(ViewModel.SaveCommand);
			btnCancel.BindCommand(ViewModel.CancelCommand);
			btnLoadAndProccessData.BindCommand(ViewModel.LoadAndProcessDataCommand);
			btnExport.BindCommand(ViewModel.ExportCommand);
			
			var monthPicker = new MonthPickerView(ViewModel.DatePickerViewModel);
			monthPicker.Show();
			hboxHandle.Add(monthPicker);
			
			var monthPickerBox = (Box.BoxChild)hboxHandle[monthPicker];
			monthPickerBox.Position = 0;
			monthPickerBox.Expand = false;
			monthPickerBox.Fill = false;

			textViewErrors.HeightRequest = 150;
			textViewErrors.Editable = false;
			textViewErrors.WrapMode = WrapMode.Word;
			textViewErrors.Binding
				.AddBinding(ViewModel, vm => vm.ResultMessage, w => w.Buffer.Text)
				.InitializeFromSource();
			
			ConfigureBalanceTree();
		}

		private void ConfigureBalanceTree()
		{
			var columnsConfig = FluentColumnsConfig<IBankStatementParsingResult>.Create()
				.AddColumn("Название").AddTextRenderer(node => node.Name)
				.AddColumn("Номер счета").AddTextRenderer(node => node.AccountNumber)
				.AddColumn("Всего")
					.MinWidth(100)
					.AddNumericRenderer(node => node.Total)
					.Adjustment(new Adjustment(0, -999_999_999_999, 999_999_999_999, 1000, 10000, 0))
					.AddSetter((cell, node) =>
					{
						if(node is BusinessAccountSummary accountSummary)
						{
							cell.Editable = true;
							return;
						}

						cell.Editable = false;
					})
					.Digits(2)
					.EditedEvent(OnTotalChanged)
				.AddColumn("");
			
			var balanceViewLevels = LevelConfigFactory
				.FirstLevel<CompanyBalanceByDay, FundsSummary>(x => x.FundsSummary)
				.NextLevel(x => x.CompanyBalanceByDay, x => x.BusinessActivitySummary)
				.NextLevel(x => x.FundsSummary, x => x.BusinessAccountsSummary)
				.LastLevel(c => c.BusinessActivitySummary)
				.EndConfig();
			
			treeComapnyBalanceByDay.YTreeModel = new LevelTreeModel<CompanyBalanceByDay>(ViewModel.CompanyBalances, balanceViewLevels);
			
			treeComapnyBalanceByDay.EnableGridLines = TreeViewGridLines.Both;
			treeComapnyBalanceByDay.ColumnsConfig = columnsConfig.Finish();

			ViewModel.CompanyBalanceChangedAction += OnCompanyBalanceChanged;
		}

		private void OnTotalChanged(object o, EditedArgs args)
		{
			var node = treeComapnyBalanceByDay.YTreeModel.NodeAtPath(new TreePath(args.Path));

			if(!(node is BusinessAccountSummary accountSummary))
			{
				return;
			}

			Gtk.Application.Invoke((sender, eventArgs) =>
			{
				ViewModel.RecalculateTotal(accountSummary);
			});

			OnCompanyBalanceChanged();
		}

		private void OnCompanyBalanceChanged()
		{
			treeComapnyBalanceByDay.QueueDraw();
		}

		public override void Destroy()
		{
			ViewModel.CompanyBalanceChangedAction -= OnCompanyBalanceChanged;
			base.Destroy();
		}
	}
}
