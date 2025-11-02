using System;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Gamma.ColumnConfig;
using QS.DomainModel.UoW;
using QS.Project.Journal;
using QS.Project.Services;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Sale;
using Vodovoz.Journals.FilterViewModels;
using Vodovoz.Journals.JournalViewModels;

namespace Vodovoz.ViewWidgets.Logistics
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class DistrictPriorityView : QS.Dialog.Gtk.WidgetOnDialogBase
	{
		public DistrictPriorityView()
		{
			this.Build();

			ytreeviewDistricts.ColumnsConfig = FluentColumnsConfig<AtWorkDriverDistrictPriority>.Create()
				.AddColumn("Район").AddTextRenderer(x => x.District.DistrictName)
				.AddColumn("Приоритет").AddNumericRenderer(x => x.Priority + 1)
				.Finish();
			ytreeviewDistricts.Reorderable = true;
		}
		
		public AtWorkDriver ListParent;
		
		private GenericObservableList<AtWorkDriverDistrictPriority> observableDistricts;
		public GenericObservableList<AtWorkDriverDistrictPriority> Districts {
			get => observableDistricts;
			set {
				observableDistricts = value;
				ytreeviewDistricts.SetItemsSource(observableDistricts);
			}
		}

		protected void OnButtonAddDistrictClicked(object sender, EventArgs e)
		{
			var filter = new DistrictJournalFilterViewModel { Status = DistrictsSetStatus.Active, OnlyWithBorders = true };
			var journalViewModel = new DistrictJournalViewModel(filter, ServicesConfig.UnitOfWorkFactory, ServicesConfig.CommonServices) {
				EnableDeleteButton = false, EnableEditButton = false, EnableAddButton = false, SelectionMode = JournalSelectionMode.Multiple
			};
			journalViewModel.OnEntitySelectedResult += (o, args) => {
				var addDistricts = args.SelectedNodes;
				addDistricts.Where(x => observableDistricts.All(d => d.District.Id != x.Id))
					.Select(x => new AtWorkDriverDistrictPriority {
						Driver = ListParent,
						District = UoW.GetById<District>(x.Id)
					})
					.ToList()
					.ForEach(x => observableDistricts.Add(x));
			};
			MyTab.TabParent.AddSlaveTab(MyTab, journalViewModel);
		}

		protected void OnButtonRemoveDistrictClicked(object sender, EventArgs e)
		{
			var toRemoveDistricts = ytreeviewDistricts.GetSelectedObjects<AtWorkDriverDistrictPriority>().ToList();
			toRemoveDistricts.ForEach(x => observableDistricts.Remove(x));
		}
	}
}
