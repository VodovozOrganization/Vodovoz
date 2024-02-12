using Gamma.ColumnConfig;
using Gamma.Utilities;
using QS.Dialog.GtkUI;
using QS.ViewModels.Control.EEVM;
using QS.Views.GtkUI;
using Vodovoz.Domain.Complaints;
using Vodovoz.Journals.JournalViewModels.Organizations;
using Vodovoz.Tools;
using Vodovoz.ViewModels.Complaints;
using Vodovoz.ViewModels.ViewModels.Organizations;

namespace Vodovoz.Views.Complaints
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class GuiltyItemsView : WidgetViewBase<GuiltyItemsViewModel>
	{
		public GuiltyItemsView()
		{
			this.Build();
		}

		public GuiltyItemsView(GuiltyItemsViewModel viewModel) : base(viewModel)
		{
			this.Build();
		}

		protected override void ConfigureWidget()
		{
			btnAddGuilty.Binding.AddBinding(ViewModel, vm => vm.CanAddGuilty, w => w.Visible).InitializeFromSource();

			GuiltyItemView wGuiltyItem = null;
			btnAddGuilty.Clicked += (sender, e) => {
				ViewModel.AddGuiltyCommand.Execute();
				CreateGuiltyWidget(ref wGuiltyItem);
			};

			btnSaveGuilty.Binding.AddBinding(ViewModel, vm => vm.CanEditGuilty, w => w.Visible).InitializeFromSource();
			btnSaveGuilty.Clicked += (sender, e) => {
				var err = ValidationHelper.RaiseValidationAndGetResult(ViewModel.CurrentGuiltyVM.Entity);
				if(err == null) {
					DestroyGuiltyWidget(wGuiltyItem);
					ViewModel.SaveGuiltyCommand.Execute();
				} else {
					MessageDialogHelper.RunWarningDialog(err);
				}
			};

			btnCancel.Binding.AddBinding(ViewModel, vm => vm.CanEditGuilty, w => w.Visible).InitializeFromSource();
			btnCancel.Clicked += (sender, e) => {
				DestroyGuiltyWidget(wGuiltyItem);
				ViewModel.CancelCommand.Execute();
			};

			btnRemoveGuilty.Clicked += (sender, e) => ViewModel.RemoveGuiltyCommand.Execute(GetSelectedGuilty());
			btnRemoveGuilty.Binding.AddBinding(ViewModel, vm => vm.CanAddGuilty, w => w.Visible).InitializeFromSource();
			btnRemoveGuilty.Binding.AddBinding(ViewModel, vm => vm.CanRemoveGuilty, w => w.Sensitive).InitializeFromSource();

			treeViewGuilty.ColumnsConfig = FluentColumnsConfig<ComplaintGuiltyItem>.Create()
				.AddColumn("Сторона")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(n => n.Responsible.Name)
				.AddColumn("Отдел ВВ")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(n => n.GetGuiltySubdivisionOrEmployee)
				.AddColumn("")
				.Finish();
			treeViewGuilty.HeadersVisible = false;
			treeViewGuilty.Binding.AddBinding(ViewModel.Entity, s => s.ObservableGuilties, w => w.ItemsDataSource).InitializeFromSource();
			treeViewGuilty.Selection.Changed += (sender, e) => ViewModel.CanRemoveGuilty = GetSelectedGuilty() != null;
		}

		void CreateGuiltyWidget(ref GuiltyItemView wGuiltyItem)
		{
			wGuiltyItem = new GuiltyItemView { ViewModel = ViewModel.CurrentGuiltyVM };

			hbxGuiltyContainer.Add(wGuiltyItem);
			hbxGuiltyContainer.ShowAll();
		}

		void DestroyGuiltyWidget(GuiltyItemView wGuiltyItem)
		{
			hbxGuiltyContainer.HideAll();
			hbxGuiltyContainer.Remove(wGuiltyItem);
			wGuiltyItem.Destroy();
		}

		ComplaintGuiltyItem GetSelectedGuilty() => treeViewGuilty.GetSelectedObject<ComplaintGuiltyItem>();
	}
}
