using Gamma.ColumnConfig;
using Gamma.Utilities;
using QS.Views.GtkUI;
using Vodovoz.Domain.Complaints;
using Vodovoz.ViewModels.Complaints;

namespace Vodovoz.Views.Complaints
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class GuiltyItemsView : EntityWidgetViewBase<GuiltyItemsViewModel>
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
			btnAddGuilty.Clicked += (sender, e) => ViewModel.AddGuiltyCommand.Execute();

			itemViewer.Binding.AddBinding(ViewModel, vm => vm.CurrentGuiltyVM, vm => vm.ViewModel).InitializeFromSource();
			itemViewer.Binding.AddBinding(ViewModel, vm => vm.CanEditGuilty, w => w.Visible).InitializeFromSource();

			btnSaveGuilty.Binding.AddBinding(ViewModel, vm => vm.CanEditGuilty, w => w.Visible).InitializeFromSource();
			btnSaveGuilty.Clicked += (sender, e) => ViewModel.SaveGuiltyCommand.Execute();

			btnCancel.Binding.AddBinding(ViewModel, vm => vm.CanEditGuilty, w => w.Visible).InitializeFromSource();
			btnCancel.Clicked += (sender, e) => ViewModel.CancelCommand.Execute();

			btnRemoveGuilty.Clicked += (sender, e) => ViewModel.RemoveGuiltyCommand.Execute(GetSelectedGuilty());
			ViewModel.RemoveGuiltyCommand.CanExecuteChanged += (sender, e) => btnRemoveGuilty.Sensitive = ViewModel.CanRemoveGuilty(GetSelectedGuilty());

			var colorBlack = new Gdk.Color(0, 0, 0);
			var colorGrey = new Gdk.Color(96, 96, 96);
			var colorWhite = new Gdk.Color(255, 255, 255);
			treeViewGuilty.ColumnsConfig = FluentColumnsConfig<ComplaintGuiltyItem>.Create()
				.AddColumn("Сторона")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(n => n.GuiltyType.GetEnumShortTitle())
					//.Editing()
				.AddColumn("Отдел ВВ")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(n => n.GetGuiltySubdivisionOrEmployee)
					/*.SetDisplayFunc(x => x.Name)
					//.FillItems(ViewModel.AllDepartments)
					.AddSetter(
						(c, n) => {
							c.Editable = ViewModel.CanAddSubdivision(n);
							if(n.GuiltyType != ComplaintGuiltyTypes.Subdivision)
								n.Subdivision = null;
							if(n.GuiltyType == ComplaintGuiltyTypes.Subdivision && n.Subdivision == null) {
								c.ForegroundGdk = colorGrey;
								c.Style = Pango.Style.Italic;
								c.Text = "(Нажмите для выбора отдела)";
								c.BackgroundGdk = colorWhite;
							} else {
								c.ForegroundGdk = colorBlack;
								c.Style = Pango.Style.Normal;
								c.Background = null;
							}
						}
					)*/
				.Finish();
			treeViewGuilty.HeadersVisible = false;
			treeViewGuilty.Binding.AddBinding(ViewModel.Entity, s => s.ObservableGuilties, w => w.ItemsDataSource).InitializeFromSource();
		}

		ComplaintGuiltyItem GetSelectedGuilty() => treeViewGuilty.GetSelectedObject<ComplaintGuiltyItem>();
	}
}