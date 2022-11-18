using System.Collections.Generic;
using System.Xml.Serialization;
using Vodovoz.RDL.Elements.Base;

namespace Vodovoz.RDL.Elements
{
	public partial class Table : BaseElementWithEnumedItems<ItemsChoiceType21>
	{
		[XmlIgnore]
		public IList<TableGroup> TableRows => GetEnumedItemsList<TableGroup, TableGroups>(x => x.TableGroup);

		[XmlIgnore]
		public IList<TableColumn> TableColumns => GetEnumedItemsList<TableColumn, TableColumns>(x => x.TableColumn);
	}
}
