using QS.Views.GtkUI;
using System;
using Vodovoz.ViewModels.Fuel.FuelCards;
namespace Vodovoz.Filters.GtkViews
{
	public partial class FuelCardJournalFilterView : FilterViewBase<FuelCardJournalFilterViewModel>
	{
		public FuelCardJournalFilterView(FuelCardJournalFilterViewModel filterViewModel) : base(filterViewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			ycheckbuttonIsShowArchived.Binding
				.AddBinding(ViewModel, vm => vm.IsShowArchived, w => w.Active)
				.InitializeFromSource();
		}
	}
}
