using QS.Views.GtkUI;
using Vodovoz.Filters.ViewModels;

namespace Vodovoz.Filters.GtkViews
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ClientCameFromFilterView : FilterViewBase<ClientCameFromFilterViewModel>
	{
		public ClientCameFromFilterView(ClientCameFromFilterViewModel сlientCameFromFilterViewModel) : base(сlientCameFromFilterViewModel)
		{
			this.Build();
			Configure();
			InitializeRestrictions();
		}

		void Configure()
		{
			yChkShowArchive.Binding.AddBinding(ViewModel, vm => vm.RestrictArchive, w => w.Active).InitializeFromSource();
		}

		void InitializeRestrictions()
		{
			yChkShowArchive.Sensitive = ViewModel.CanChangeShowArchive;
		}
	}
}
