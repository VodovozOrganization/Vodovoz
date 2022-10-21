using System;
using System.Xml.Serialization;
using System.Xml;

namespace Vodovoz.RDL.Rdl2005
{
	[Serializable()]
	[XmlType()]
	public partial class Sorting
	{
		private SortBy[] sortByField;
		private XmlAttribute[] anyAttrField;

		[XmlElement("SortBy")]
		public SortBy[] SortBy
		{
			get => sortByField;
			set => sortByField = value;
		}

		[XmlAnyAttribute()]
		public XmlAttribute[] AnyAttr
		{
			get => anyAttrField;
			set => anyAttrField = value;
		}
	}
}