using QS.Views.GtkUI;
using System;
using System.ComponentModel;
using Vodovoz.Presentation.ViewModels.Common;

namespace Vodovoz.Presentation.Views
{
	[ToolboxItem(true)]
	public partial class IncludeExcludeFilterGroupsView : WidgetViewBase<IncludeExludeFiltersViewModel>
	{
		[Obsolete("Не использовать, только для дизайнера!!")]
		public IncludeExcludeFilterGroupsView() { }

		public IncludeExcludeFilterGroupsView(IncludeExludeFiltersViewModel viewModel)
			: base(viewModel)
		{
			Build();

			Initialize();
		}

		private void Initialize()
		{
			
		}
	}
}
