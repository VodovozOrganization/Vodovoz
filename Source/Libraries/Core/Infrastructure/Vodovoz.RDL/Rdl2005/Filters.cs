using System;
using System.Xml.Serialization;
using System.Xml;

namespace Vodovoz.RDL.Rdl2005
{
	[Serializable()]
	[XmlType()]
	public partial class Filters
	{
		private Filter[] filterField;
		private XmlAttribute[] anyAttrField;

		[XmlElement("Filter")]
		public Filter[] Filter
		{
			get => filterField;
			set => filterField = value;
		}

		[XmlAnyAttribute()]
		public XmlAttribute[] AnyAttr
		{
			get => anyAttrField;
			set => anyAttrField = value;
		}
	}
}