using System.Collections.Generic;
using System.Xml.Serialization;
using Vodovoz.RDL.Elements.Base;

namespace Vodovoz.RDL.Elements
{
	public partial class TableRow : BaseElementWithItems
	{
		[XmlIgnore]
		public IList<TableCell> Cells => GetItemsList<TableCell, TableCells>(x => x.TableCell);
	}
}
