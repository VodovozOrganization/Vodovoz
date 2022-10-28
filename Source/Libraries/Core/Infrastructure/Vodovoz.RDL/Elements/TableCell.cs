using System.Collections.Generic;
using System.Xml.Serialization;
using Vodovoz.RDL.Elements.Base;

namespace Vodovoz.RDL.Elements
{
	public partial class TableCell : BaseElementWithItems
	{
		[XmlIgnore]
		public uint ColSpan
		{
			get => GetItemsValue<uint>();
			set => SetItemsValue(value);
		}

		[XmlIgnore]
		public ReportItems ReportItems
		{
			get => GetItemsValue<ReportItems>();
			set => SetItemsValue(value);
		}
	}
}
