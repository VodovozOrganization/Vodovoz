using System.ComponentModel;
using Gamma.ColumnConfig;
using QS.DomainModel.Entity;
using QS.Views.GtkUI;
using Vodovoz.ViewModels.Widgets;

namespace Vodovoz.Views.Common
{
	/// <summary>
	/// Вьюха для работы со списком Сущностей, поддерживающих <c>INamedDomainObject</c>
	/// </summary>
	[ToolboxItem(true)]
	public partial class AddOrRemoveIDomainObjectView : WidgetViewBase<AddOrRemoveIDomainObjectViewModelBase>
	{
		public AddOrRemoveIDomainObjectView()
		{
			Build();
		}

		public AddOrRemoveIDomainObjectView(AddOrRemoveIDomainObjectViewModelBase viewModel) : base(viewModel)
		{
			Build();
		}

		protected override void ConfigureWidget()
		{
			lblTitle.Binding
				.AddBinding(ViewModel, vm => vm.Title, w => w.LabelProp)
				.InitializeFromSource();

			var columnsConfig = 
				FluentColumnsConfig<INamedDomainObject>.Create()
				.AddColumn("Код").AddNumericRenderer(x => x.Id)
				.AddColumn("Наименование").AddTextRenderer(x => x.Name);

			if(ViewModel is OrganizationForOrderOrganizationSettingsViewModel organizationsForSetViewModel)
			{
				columnsConfig.AddColumn("Время выбора")
					.AddTextRenderer(x => GetOrganizationChoiceTime(organizationsForSetViewModel, x.Id));
			}

			columnsConfig.AddColumn("");

			treeDomainObjects.ColumnsConfig = columnsConfig.Finish();
			
			treeDomainObjects.ItemsDataSource = ViewModel.Entities;
			treeDomainObjects.Binding
				.AddBinding(ViewModel, vm => vm.SelectedEntity, w => w.SelectedRow)
				.InitializeFromSource();

			btnAdd.BindCommand(ViewModel.AddCommand);
			btnRemove.BindCommand(ViewModel.RemoveCommand);
		}

		private string GetOrganizationChoiceTime(OrganizationForOrderOrganizationSettingsViewModel viewModel, int organizationId)
		{
			return viewModel.GetOrganizationChoiceTime(organizationId);
		}
	}
}
