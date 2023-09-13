using System;
using QS.DomainModel.Entity;
using QS.Views.GtkUI;
using Vodovoz.ViewModels.ViewModels.Settings;

namespace Vodovoz.Views.Settings
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class NamedDomainEntitiesSettingsView : WidgetViewBase<NamedDomainEntitiesSettingsViewModelBase>
	{
		public NamedDomainEntitiesSettingsView()
		{
			Build();
		}
		
		protected override void ConfigureWidget()
		{
			ybtnAddEntities.Clicked += OnAddEntitiesClicked;
			ybtnDeleteEntities.Clicked += OnDeleteEntitiesClicked;
			ybtnSaveEntities.Clicked += OnSaveEntitiesClicked;
			ybtnSubdivisionsInfo.Clicked += OnInfoClicked;

			ybtnAddEntities.Sensitive = ViewModel.CanEdit;

			ytreeEntities.CreateFluentColumnsConfig<INamedDomainObject>()
				.AddColumn("Номер").AddNumericRenderer(x => x.Id)
				.AddColumn("Наименование").AddTextRenderer(x => x.Name)
				.AddColumn("")
				.Finish();

			ytreeEntities.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.CanEdit, w => w.Sensitive)
				.AddBinding(vm => vm.ObservableEntities, w => w.ItemsDataSource)
				.AddBinding(vm => vm.SelectedEntity, w => w.SelectedRow)
				.InitializeFromSource();

			ybtnSaveEntities.Binding
				.AddBinding(ViewModel, vm => vm.CanSave, w => w.Sensitive)
				.InitializeFromSource();

			ybtnDeleteEntities.Binding
				.AddBinding(ViewModel, vm => vm.CanRemove, w => w.Sensitive)
				.InitializeFromSource();

			ylabelTitle.Binding
				.AddBinding(ViewModel, vm => vm.DetailTitle, w => w.LabelProp)
				.InitializeFromSource();

			((Gtk.Label)frameConfiguration.LabelWidget).LabelProp = ViewModel.MainTitle;
		}

		private void OnInfoClicked(object sender, EventArgs e)
		{
			ViewModel.ShowInfoCommand?.Execute();
		}

		private void OnSaveEntitiesClicked(object sender, EventArgs e)
		{
			ViewModel.SaveEntitiesCommand?.Execute();
		}

		private void OnDeleteEntitiesClicked(object sender, EventArgs e)
		{
			ViewModel.RemoveEntityCommand?.Execute();
		}

		private void OnAddEntitiesClicked(object sender, EventArgs e)
		{
			ViewModel.AddEntityCommand?.Execute();
		}
	}
}
