using System.Collections.Generic;
using Vodovoz.RDL.Facades.Utils;

namespace Vodovoz.RDL.Rdl2005
{
	public partial class Table
	{

		private IEnumerable<TableGroup> _groups;
		private IEnumerable<TableColumn> _columns;

		public IEnumerable<TableGroup> Groups =>
			Utils.GetElements(
				ref _groups, 
				ItemsElementName, 
				ItemsChoiceType21.TableGroups, 
				(index) => ((TableGroups)Items[index]).TableGroup
			);

		public IEnumerable<TableColumn> Columns => 
			Utils.GetElements(
				ref _columns, 
				ItemsElementName, 
				ItemsChoiceType21.TableColumns, 
				(index) => ((TableColumns)Items[index]).TableColumn
			);

	}
}
