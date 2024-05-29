using QS.Views.GtkUI;
using System;
using System.ComponentModel;
using Vodovoz.Presentation.ViewModels.Common;

namespace Vodovoz.Presentation.Views
{
	[ToolboxItem(true)]
	public partial class IncludeExcludeFilterGroupView : WidgetViewBase<IncludeExludeFilterGroupViewModel>
	{
		[Obsolete("Не использовать, только для дизайнера!!")]
		public IncludeExcludeFilterGroupView() { }

		public IncludeExcludeFilterGroupView(IncludeExludeFilterGroupViewModel viewModel)
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
