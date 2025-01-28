using System.ComponentModel;
using Gamma.ColumnConfig;
using QS.DomainModel.Entity;
using QS.Views.GtkUI;
using Vodovoz.Presentation.ViewModels.Common;

namespace Vodovoz.Presentation.Views
{
	[ToolboxItem(true)]
	public partial class AddOrRemoveIDomainObjectView : WidgetViewBase<AddOrRemoveIDomainObjectViewModel>
	{
		public AddOrRemoveIDomainObjectView(AddOrRemoveIDomainObjectViewModel viewModel) : base(viewModel)
		{
			Build();
			ConfigureWidget();
		}

		protected override void ConfigureWidget()
		{
			lblTitle.Binding
				.AddBinding(ViewModel, vm => vm.Title, w => w.LabelProp)
				.InitializeFromSource();

			treeDomainObjects.ColumnsConfig =
				FluentColumnsConfig<INamedDomainObject>.Create()
				.AddColumn("Код").AddNumericRenderer(x => x.Id)
				.AddColumn("Наименование").AddTextRenderer(x => x.Name)
				.AddColumn("")
				.Finish();

			treeDomainObjects.ItemsDataSource = ViewModel.Entities;
			treeDomainObjects.Binding
				.AddBinding(ViewModel, vm => vm.SelectedEntity, w => w.SelectedRow)
				.InitializeFromSource();

			btnAdd.BindCommand(ViewModel.AddCommand);
			btnRemove.BindCommand(ViewModel.RemoveCommand);
		}
	}
}
