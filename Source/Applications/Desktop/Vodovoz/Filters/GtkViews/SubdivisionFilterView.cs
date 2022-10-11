using System;
using QS.Views.GtkUI;
using Vodovoz.FilterViewModels.Organization;

namespace Vodovoz.Filters.GtkViews
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class SubdivisionFilterView : FilterViewBase<SubdivisionFilterViewModel>
	{
		public SubdivisionFilterView(SubdivisionFilterViewModel filterViewModel) : base(filterViewModel)
		{
			this.Build();
		}
	}
}
