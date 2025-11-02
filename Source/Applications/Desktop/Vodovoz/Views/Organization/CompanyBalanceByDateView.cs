using System;
using System.Globalization;
using System.Linq;
using Gamma.Binding;
using Gamma.Binding.Core.LevelTreeConfig;
using Gamma.ColumnConfig;
using Gtk;
using QS.Views.Dialog;
using Vodovoz.Core.Domain.Organizations;
using Vodovoz.Infrastructure;
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
				.AddColumn("Банк").AddTextRenderer(node => node.Bank)
				.AddColumn("Номер счета").AddTextRenderer(node => node.AccountNumber)
				.AddColumn("Всего")
					.MinWidth(100)
					.AddNumericRenderer(node => node.Total, OnTotalEdited, true)
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
					.AddSetter((cell, node) =>
					{
						if(node is CompanyBalanceByDay companyBalance
						   && companyBalance.FundsSummary
							   .SelectMany(x => x.BusinessActivitySummary)
							   .SelectMany(x => x.BusinessAccountsSummary)
							   .Any(x => !x.Total.HasValue))
						{
							cell.CellBackgroundGdk = GdkColors.DangerBase;
							return;
						}
						
						if(node is FundsSummary fundsSummary
						   && fundsSummary.BusinessActivitySummary
							   .SelectMany(x => x.BusinessAccountsSummary)
							   .Any(x => !x.Total.HasValue))
						{
							cell.CellBackgroundGdk = GdkColors.DangerBase;
							return;
						}
						
						if(node is BusinessActivitySummary activitySummary
						   && activitySummary.BusinessAccountsSummary.Any(x => !x.Total.HasValue))
						{
							cell.CellBackgroundGdk = GdkColors.DangerBase;
							return;
						}
						
						if(node is BusinessAccountSummary accountSummary
							&& !accountSummary.Total.HasValue)
						{
							cell.CellBackgroundGdk = GdkColors.DangerBase;
							return;
						}

						cell.CellBackgroundGdk = GdkColors.PrimaryBase;
					})
					.Digits(2)
					.EditingStartedEvent(OnTotalEditingStarted)
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

		private void OnTotalEditingStarted(object o, EditingStartedArgs args)
		{
			if(!(treeComapnyBalanceByDay.YTreeModel.NodeAtPath(new TreePath(args.Path)) is IBankStatementParsingResult accountSummary))
			{
				return;
			}

			if(!accountSummary.Total.HasValue)
			{
				return;
			}
					
			if(o is CellRendererSpin spin)
			{
				spin.Adjustment.Value = Convert.ToDouble(accountSummary.Total, CultureInfo.CurrentUICulture);
			}
		}

		private void OnTotalEdited(object o, EditedArgs args)
		{
			decimal? newTotal = null;
			
			if(!string.IsNullOrWhiteSpace(args.NewText))
			{
				var newText = args.NewText.Replace(',', '.');
				
				if(decimal.TryParse(newText, NumberStyles.Any, CultureInfo.InvariantCulture, out var newValue))
				{
					newTotal = newValue;
				}
			}
			
			var node = treeComapnyBalanceByDay.YTreeModel.NodeAtPath(new TreePath(args.Path));

			if(!(node is BusinessAccountSummary accountSummary))
			{
				return;
			}

			accountSummary.Total = newTotal;
			ViewModel.RecalculateTotal(accountSummary);
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
