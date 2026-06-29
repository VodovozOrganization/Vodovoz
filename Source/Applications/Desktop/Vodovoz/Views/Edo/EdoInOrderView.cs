using Gamma.ColumnConfig;
using QS.Views.GtkUI;
using System;
using Vodovoz.Core.Domain.Edo;
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

			ytreeviewDocTypes1.ColumnsConfig = FluentColumnsConfig<EdoInOrderDocumentTypeViewModel>.Create()
				.AddColumn("Тип документа")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => x.Title)
					.XAlign(0.5f)
				.Finish();
			ytreeviewDocTypes1.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.DocumentGroupTypes, w => w.ItemsDataSource)
				.AddBinding(vm => vm.SelectedDocumentGroupType, w => w.SelectedRow)
				.InitializeFromSource();

			ytreeview1.ColumnsConfig = FluentColumnsConfig<EdoInOrderDocumentHistoryRowViewModel>.Create()
				.AddColumn("Время начала:")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => x.TimeString)
					.XAlign(0.5f)
				.AddColumn("Кто запустил:")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => x.SourceString)
					.XAlign(0.5f)
				.AddColumn("Статус:")
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
				.AddColumn("Документ:")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => x.DocumentTypeString)
					.XAlign(0.5f)
				.AddColumn("Кодов:")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => x.CodesQuantityString)
					.XAlign(0.5f)
				.Finish();
			ytreeview1.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.Documents, w => w.ItemsDataSource)
				.AddBinding(vm => vm.SelectedDocument, w => w.SelectedRow)
				.InitializeFromSource();

			pipelineDocumentStages.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.PipelineViewModel, w => w.ViewModel)
				.InitializeFromSource();

		}

		void IActivatableOrderTab.Activate()
		{
			ViewModel.Load();
		}
	}

	public interface IActivatableOrderTab
	{
		void Activate();
	}
}
