using Gamma.ColumnConfig;
using QS.Views.GtkUI;
using System;
using Vodovoz.Infrastructure;
using Vodovoz.ViewModels.Edo;
namespace Vodovoz.Views.Edo
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class EdoForOrderView : WidgetViewBase<EdoForOrderViewModel>
	{
		public EdoForOrderView()
		{
			this.Build();
		}

		protected override void ConfigureWidget()
		{
			base.ConfigureWidget();

			toggleButtonHelp.Toggled += ButtonHelpToggled;
			ynotebook1.ShowTabs = false;

			treeViewEdoTasks.ColumnsConfig = FluentColumnsConfig<EdoTaskInOrderViewModel>.Create()
				.AddColumn("Время заявки")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => x.RequestTime).Editable(false)
				.AddColumn("Код задачи")
					.HeaderAlignment(0.5f)
					.AddNumericRenderer(x => x.TaskId).Editing(false)
				.AddColumn("Инициатор")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => x.Initiator).Editable(false)
				.AddColumn("Тип задачи")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => x.TaskType).Editable(false)
				.AddColumn("Статус задачи")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => x.Status).Editable(false)
					.AddSetter((c, n) =>
					{
						if(n.EdoTaskNode.EdoTaskStatus == Core.Domain.Edo.EdoTaskStatus.Problem)
						{
							c.BackgroundGdk = GdkColors.DangerBase;
						}
						else
						{
							c.BackgroundGdk = GdkColors.PrimaryBase;
						}
					})
				.AddColumn("")
				.RowCells()
					.XAlign(0.5f)
				.Finish();
			treeViewEdoTasks.Selection.Mode = Gtk.SelectionMode.Single;
			treeViewEdoTasks.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.EdoTasks, w => w.ItemsDataSource)
				.AddBinding(vm => vm.SelectedTask, w => w.SelectedRow)
				.InitializeFromSource();


			treeViewEdoTransferTasks.ColumnsConfig = FluentColumnsConfig<TransferEdoTaskInOrderViewModel>.Create()
				.AddColumn("Время заявки")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => x.RequestTime).Editable(false)
				.AddColumn("Код задачи")
					.HeaderAlignment(0.5f)
					.AddNumericRenderer(x => x.TaskId).Editing(false)
				.AddColumn("Откуда")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => x.From).Editable(false)
				.AddColumn("Куда")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => x.To).Editable(false)
				.AddColumn("Статус")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => x.Status).Editable(false)
				.AddColumn("")
				.RowCells()
					.XAlign(0.5f)
				.Finish();
			treeViewEdoTransferTasks.Selection.Mode = Gtk.SelectionMode.Single;
			treeViewEdoTransferTasks.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.TransferEdoTasks, w => w.ItemsDataSource)
				.AddBinding(vm => vm.SelectedTransferTask, w => w.SelectedRow)
				.InitializeFromSource();

			treeViewProblems.ColumnsConfig = FluentColumnsConfig<EdoProblemInOrderViewModel>.Create()
				.AddColumn("Время создания")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => x.CreationTime).Editable(false)
				.AddColumn("Состояние")
					.HeaderAlignment(0.5f)
					.AddNumericRenderer(x => x.State).Editing(false)
					.AddSetter((c, n) =>
					{
						if(n.ProblemNode.State == Core.Domain.Edo.TaskProblemState.Active)
						{
							c.BackgroundGdk = GdkColors.DangerBase;
						}
						else
						{
							c.BackgroundGdk = GdkColors.SuccessBase;
						}
					})
				.AddColumn("Название")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => x.Message).Editable(false)
				.AddColumn("")
				.RowCells()
					.XAlign(0.5f)
				.Finish();
			treeViewProblems.Selection.Mode = Gtk.SelectionMode.Single;
			treeViewProblems.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.Problems, w => w.ItemsDataSource)
				.AddBinding(vm => vm.SelectedProblem, w => w.SelectedRow)
				.InitializeFromSource();

			textViewProblemDescription.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.ProblemDescription, w => w.Buffer.Text)
				.InitializeFromSource();
			textViewProblemRecommendation.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.ProblemRecommendation, w => w.Buffer.Text)
				.InitializeFromSource();

			edotaskinorderresolveview1.ViewModel = ViewModel.EdoTaskInOrderResolveViewModel;

			buttonRefresh.BindCommand(ViewModel.RefreshCommand);
			buttonResend.Visible = false;
		}

		private void ButtonHelpToggled(object sender, EventArgs e)
		{
			if(toggleButtonHelp.Active)
			{
				ynotebook1.CurrentPage = 1;
			}
			else
			{
				ynotebook1.CurrentPage = 0;
			}
		}
	}
}
