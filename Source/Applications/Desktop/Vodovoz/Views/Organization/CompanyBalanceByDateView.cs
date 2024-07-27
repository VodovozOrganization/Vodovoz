using System.Linq;
using Gamma.Binding;
using Gamma.Binding.Core.LevelTreeConfig;
using Gamma.ColumnConfig;
using Gamma.GtkWidgets;
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
			
			var monthPicker = new MonthPickerView(ViewModel.DatePickerViewModel);
			monthPicker.Show();
			hboxHandle.Add(monthPicker);
			
			var monthPickerBox = (Box.BoxChild)hboxHandle[monthPicker];
			monthPickerBox.Position = 0;
			monthPickerBox.Expand = false;
			monthPickerBox.Fill = false;

			textViewErrors.HeightRequest = 150;
			textViewErrors.Binding
				.AddBinding(ViewModel, vm => vm.ResultMessage, w => w.Buffer.Text)
				.InitializeFromSource();
			
			ConfigureBalanceTree();
		}

		private void ConfigureBalanceTree()
		{
			/*var columnsConfig = FluentColumnsConfig<object>.Create()
				.AddColumn("Форма ДС").AddTextRenderer(node => node.FundsName)
				.AddColumn("Всего").AddNumericRenderer(node => node.FundsTotal);

			for(var i = 0; i < ViewModel.BusinessActivities.Count(); i++)
			{
				columnsConfig.AddColumn($"{ViewModel.BusinessActivities[i].Name}")
					.AddTextRenderer(node => ViewModel.BusinessActivities[i].AccountName)
					.AddTextRenderer(node => ViewModel.BusinessActivities[i].Bank)
					.AddNumericRenderer(node => ViewModel.BusinessActivities[i].AccountTotal);
			}*/

			//treeComapnyBalanceByDay.ItemsDataSource = ViewModel.Entity;
			
			var columnsConfig = FluentColumnsConfig<object>.Create()
				.AddColumn("Название").AddTextRenderer(node => GetNodeName(node))
				.AddColumn("Всего").AddNumericRenderer(node => GetTotal(node));
			
			var levels = LevelConfigFactory
				.FirstLevel<FundsSummary, BusinessActivitySummary>(x => x.BusinessActivitySummary)
				.NextLevel(x => x.FundsSummary, x => x.BusinessAccountsSummary)
				.LastLevel(c => c.BusinessActivitySummary)
				.EndConfig();
			
			treeComapnyBalanceByDay.YTreeModel = new LevelTreeModel<FundsSummary>(ViewModel.Entity.FundsSummary, levels);

			treeComapnyBalanceByDay.EnableGridLines = TreeViewGridLines.Both;
			treeComapnyBalanceByDay.ColumnsConfig = columnsConfig.Finish();
		}
		
		private string GetNodeName(object node)
		{
			if(node is FundsSummary fundsSummary)
			{
				return fundsSummary.Funds.Name;
			}

			if(node is BusinessActivitySummary businessActivitySummary)
			{
				return businessActivitySummary.BusinessActivity.Name;
			}

			if(node is BusinessAccountSummary businessAccountSummary)
			{
				var businessAccount = businessAccountSummary.BusinessAccount;
				return $"{businessAccount.Name} {businessAccount.Bank}";
			}

			return string.Empty;
		}
		
		private object GetTotal(object node)
		{
			if(node is FundsSummary fundsSummary)
			{
				return fundsSummary.Total;
			}

			if(node is BusinessActivitySummary businessActivitySummary)
			{
				return businessActivitySummary.Total;
			}

			if(node is BusinessAccountSummary businessAccountSummary)
			{
				return businessAccountSummary.Total;
			}

			return 0m;
		}
	}
}
