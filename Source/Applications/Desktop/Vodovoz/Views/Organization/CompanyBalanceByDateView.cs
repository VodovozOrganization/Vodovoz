using Gamma.Binding;
using Gamma.Binding.Core.LevelTreeConfig;
using Gamma.ColumnConfig;
using Gamma.GtkWidgets;
using QS.Views.Dialog;
using Vodovoz.Core.Domain.Organizations;
using Vodovoz.Presentation.ViewModels.Organisations;

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
			var balanceTreeView = new yTreeView();
			balanceTreeView.Show();
			
			vboxMain.Add(balanceTreeView);

			ConfigureBalanceTree(balanceTreeView);
		}

		private void ConfigureBalanceTree(yTreeView treeView)
		{
			var columnsConfig = FluentColumnsConfig<object>.Create()
				.AddColumn("Название").AddTextRenderer(node => GetNodeName(node))
				.AddColumn("Всего").AddNumericRenderer(node => GetTotal(node))
				.Finish();
			
			var levels = LevelConfigFactory
				.FirstLevel<FundsSummary, BusinessActivitySummary>(x => x.BusinessActivitySummary)
				.NextLevel(x => x.FundsSummary, x => x.BusinessAccountsSummary)
				.LastLevel(c => c.BusinessActivitySummary)
				.EndConfig();
			
			treeView.YTreeModel = new LevelTreeModel<FundsSummary>(ViewModel.Entity.FundsSummary, levels);
			
			treeView.ColumnsConfig = columnsConfig;
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
