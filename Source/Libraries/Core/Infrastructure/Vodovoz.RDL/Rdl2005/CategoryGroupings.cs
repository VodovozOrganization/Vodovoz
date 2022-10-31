using System;
using System.Xml.Serialization;
using System.Xml;

namespace Vodovoz.RDL.Rdl2005
{
	[Serializable()]
	[XmlType()]
	public partial class CategoryGroupings
	{
		private CategoryGrouping[] categoryGroupingField;
		private XmlAttribute[] anyAttrField;

		[XmlElement("CategoryGrouping")]
		public CategoryGrouping[] CategoryGrouping
		{
			get => categoryGroupingField;
			set => categoryGroupingField = value;
		}

		[XmlAnyAttribute()]
		public XmlAttribute[] AnyAttr
		{
			get => anyAttrField;
			set => anyAttrField = value;
		}
	}
}