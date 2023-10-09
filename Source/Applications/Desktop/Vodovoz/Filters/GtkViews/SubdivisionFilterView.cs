using System;
using System.ComponentModel;
using QS.Views.GtkUI;
using Vodovoz.FilterViewModels.Organization;

namespace Vodovoz.Filters.GtkViews
{
	[ToolboxItem(true)]
	public partial class SubdivisionFilterView : FilterViewBase<SubdivisionFilterViewModel>
	{
		public SubdivisionFilterView(SubdivisionFilterViewModel filterViewModel) : base(filterViewModel)
		{
			Build();

			Initialize();
		}

		private void Initialize()
		{
			ycheckArchieve.Binding
				.AddBinding(ViewModel, vm => vm.ShowArchieved, w => w.Active)
				.InitializeFromSource();
		}
	}
}
