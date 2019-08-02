using Gamma.ColumnConfig;
using QS.Views.GtkUI;
using Vodovoz.Domain.Complaints;
using Vodovoz.ViewModels.Complaints;

namespace Vodovoz.Views.Complaints
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class GuiltyItemsView : EntityWidgetViewBase<GuiltyItemsViewModel>
	{
		public GuiltyItemsView(GuiltyItemsViewModel viewModel) : base(viewModel)
		{
			this.Build();
			ConfigureDlg();
		}

		void ConfigureDlg()
		{
			enumBtnGuiltySide.ItemsEnum = typeof(ComplaintGuiltyTypes);
			enumBtnGuiltySide.EnumItemClicked += (sender, e) => ViewModel.AddGuiltyCommand.Execute((ComplaintGuiltyTypes)e.ItemEnum);

			var colorBlack = new Gdk.Color(0, 0, 0);
			var colorGrey = new Gdk.Color(96, 96, 96);
			var colorWhite = new Gdk.Color(255, 255, 255);
			treeViewGuilty.ColumnsConfig = FluentColumnsConfig<ComplaintGuiltyItem>.Create()
				.AddColumn("Сторона")
					.HeaderAlignment(0.5f)
					.AddEnumRenderer(n => n.GuiltyType)
					.Editing()
				.AddColumn("Отдел ВВ")
					.HeaderAlignment(0.5f)
					.AddComboRenderer(n => n.Subdivision)
					.SetDisplayFunc(x => x.Name)
					.FillItems(ViewModel.AllDepartments)
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
					)
				.Finish();
			treeViewGuilty.HeadersVisible = false;
			treeViewGuilty.Binding.AddBinding(ViewModel.Entity, s => s.ObservableGuilties, w => w.ItemsDataSource).InitializeFromSource();

			btnRemove.Clicked += (sender, e) => ViewModel.RemoveGuiltyCommand.Execute(GetSelectedGuilty());
			ViewModel.RemoveGuiltyCommand.CanExecuteChanged += (sender, e) => btnRemove.Sensitive = ViewModel.CanRemoveGuilty(GetSelectedGuilty());
		}

		ComplaintGuiltyItem GetSelectedGuilty() => treeViewGuilty.GetSelectedObject<ComplaintGuiltyItem>();
	}
}