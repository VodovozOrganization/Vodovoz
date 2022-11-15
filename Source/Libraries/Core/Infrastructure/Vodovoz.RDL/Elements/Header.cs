using System.Collections.Generic;
using System.Xml.Serialization;
using Vodovoz.RDL.Elements.Base;

namespace Vodovoz.RDL.Elements
{
	public partial class Header : BaseElementWithEnumedItems<ItemsChoiceType20>
	{
		[XmlIgnore]
		public IList<TableRow> TableRows => GetEnumedItemsList<TableRow, TableRows>(x => x.TableRow);
	}
}
