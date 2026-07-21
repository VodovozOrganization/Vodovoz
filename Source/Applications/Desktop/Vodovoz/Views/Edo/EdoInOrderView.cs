using Gamma.ColumnConfig;
using QS.Views.GtkUI;
using System;
using System.ComponentModel;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Dialogs.OrderWidgets;
using Vodovoz.Infrastructure;
using Vodovoz.ViewModels.Edo;
namespace Vodovoz.Views.Edo
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class EdoInOrderView : WidgetViewBase<EdoInOrderViewModel>, IActivatableOrderTab
	{
		public EdoInOrderView()
		{
			this.Build();
		}

		protected override void ConfigureWidget()
		{
			base.ConfigureWidget();

			ytreeviewDocTypes.HeightRequest = 140;
			ytreeviewDocTypes.ColumnsConfig = FluentColumnsConfig<EdoInOrderDocumentTypeViewModel>.Create()
				.AddColumn("Тип документа")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => x.Title)
					.XAlign(0.5f)
				.AddColumn("Кол-во")
					.HeaderAlignment(0.5f)
					.AddNumericRenderer(x => x.Quantity).Editing(false)
					.XAlign(0.5f)
				.Finish();
			ytreeviewDocTypes.Selection.Mode = Gtk.SelectionMode.Single;
			ytreeviewDocTypes.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.DocumentGroupTypes, w => w.ItemsDataSource)
				.AddBinding(vm => vm.SelectedDocumentGroupType, w => w.SelectedRow)
				.InitializeFromSource();

			ytreeviewDocuments.ColumnsConfig = FluentColumnsConfig<EdoInOrderDocumentHistoryRowViewModel>.Create()
				.AddColumn("Время начала")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => x.TimeString)
					.XAlign(0.5f)
				.AddColumn("Кто запустил")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => x.SourceString)
					.XAlign(0.5f)
				.AddColumn("Статус")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => x.StatusString)
					.XAlign(0.5f)
					.AddSetter((c, n) => {
						if(n.Status == EdoTaskStatus.Problem)
						{
							c.CellBackgroundGdk = GdkColors.DangerBase;
						}
						else
						{
							c.CellBackgroundGdk = GdkColors.PrimaryBase;
						}
					})
				.AddColumn("Документ")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => x.DocumentTypeString)
					.XAlign(0.5f)
				.AddColumn("Кодов")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => x.CodesQuantityString)
					.XAlign(0.5f)
				.AddColumn("")
				.Finish();
			ytreeviewDocuments.Selection.Mode = Gtk.SelectionMode.Single;
			ytreeviewDocuments.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.Documents, w => w.ItemsDataSource)
				.AddBinding(vm => vm.SelectedDocument, w => w.SelectedRow)
				.InitializeFromSource();

			pipelineTransferStages.PipelineVerticalPadding = 5;
			pipelineTransferStages.PipelineSidePadding = 10;
			pipelineTransferStages.HorizontalAlignment = 0f;
			pipelineTransferStages.VerticalAlignment = 0f;
			pipelineTransferStages.HeightRequest = 0;
			pipelineTransferStages.StageCircleRadius = 16;
			pipelineTransferStages.StageAdditionalInfoHeight = 14;
			pipelineTransferStages.TitleHeight = 12;
			pipelineTransferStages.TitleBottomSpacing = 4;
			pipelineTransferStages.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.PipelineViewModel, w => w.ViewModel)
				.InitializeFromSource();

			ytreeviewProblems.ColumnsConfig = FluentColumnsConfig<EdoInOrderProblemViewModel>.Create()
				.AddColumn("Время")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => x.CreationTime).Editable(false)
					.XAlign(0.5f)
				.AddColumn("Состояние")
					.HeaderAlignment(0.5f)
					.AddNumericRenderer(x => x.State).Editing(false)
					.XAlign(0.5f)
					.AddSetter((c, n) =>
					{
						if(n.ProblemNode.State == TaskProblemState.Active)
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
					.XAlign(0.5f)
				.AddColumn("")
				.Finish();

			ytreeviewProblems.Selection.Mode = Gtk.SelectionMode.Single;
			ytreeviewProblems.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.Problems, w => w.ItemsDataSource)
				.AddBinding(vm => vm.SelectedProblem, w => w.SelectedRow)
				.InitializeFromSource();

			textViewProblemDescription.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.ProblemDescription, w => w.Buffer.Text)
				.InitializeFromSource();
			textViewProblemRecommendation.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.ProblemRecommendation, w => w.Buffer.Text)
				.InitializeFromSource();

			ytreeviewProblemItems.ColumnsConfig = FluentColumnsConfig<string>.Create()
				.AddColumn("")
				.HeaderAlignment(0.5f)
				.AddTextRenderer(x => x).Editable(false)
				.XAlign(0.5f)
				.Finish();
			ytreeviewProblemItems.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.ProblemItems, w => w.ItemsDataSource)
				.InitializeFromSource();
			frameProblems.Visible = false;

			edoinorderactionsview.ViewModel = ViewModel.EdoInOrderDocumentActionsViewModel;

			ordercodesview1.ViewModel = ViewModel.OrderCodesViewModel;

			buttonRefresh.BindCommand(ViewModel.RefreshCommnand);

			radiobuttonHelp.Toggled += RadiobuttonHelpToggled;
			radiobuttonDocuments.Toggled += RadiobuttonDocumentsToggled;
			radiobuttonCodes.Toggled += RadiobuttonCodesToggled;
			radiobuttonDocuments.Click();

			ViewModel.PropertyChanged += ViewModelPropertyChanged;
		}

		private void ViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(ViewModel.HasProblems))
			{
				frameProblems.Visible = ViewModel.HasProblems;
			}

			if(e.PropertyName == nameof(ViewModel.DocumentViewModel))
			{
				edoinorderdocumentview1.ViewModel = ViewModel.DocumentViewModel;
			}
		}

		private void RadiobuttonHelpToggled(object sender, EventArgs e)
		{
			if(radiobuttonHelp.Active)
			{
				ynotebookEdoInOrder.CurrentPage = 0;
			}
		}

		private void RadiobuttonDocumentsToggled(object sender, EventArgs e)
		{
			if(radiobuttonDocuments.Active)
			{
				ynotebookEdoInOrder.CurrentPage = 1;
			}
		}
		private void RadiobuttonCodesToggled(object sender, EventArgs e)
		{
			if(radiobuttonCodes.Active)
			{
				ynotebookEdoInOrder.CurrentPage = 2;
				ViewModel.LoadCodes();
			}
		}

		void IActivatableOrderTab.Activate()
		{
			ViewModel.Load();
		}
	}
}
