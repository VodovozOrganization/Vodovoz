using Vodovoz.RDL.Facades.Utils;

namespace Vodovoz.RDL.Rdl2005
{
	public partial class TableGroup
	{
		private bool _footerInit;
		private Footer _footer;

		private bool _groupingInit;
		private Grouping _grouping;

		private bool _headerInit;
		private Header _header;

		private bool _sortingInit;
		private Sorting _sorting;

		private bool _visibilityInit;
		private Visibility _visibility;

		public TableGroup()
		{
		}

		public Footer Footer => Utils.GetProperty(ref _footerInit, ref _footer, Items);
		public Grouping Grouping => Utils.GetProperty(ref _groupingInit, ref _grouping, Items);
		public Header Header => Utils.GetProperty(ref _headerInit, ref _header, Items);
		public Sorting Sorting => Utils.GetProperty(ref _sortingInit, ref _sorting, Items);
		public Visibility Visibility => Utils.GetProperty(ref _visibilityInit, ref _visibility, Items);
	}
}
