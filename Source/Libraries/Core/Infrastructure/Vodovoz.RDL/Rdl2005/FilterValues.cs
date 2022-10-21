using System;
using System.Xml.Serialization;
using System.Xml;

namespace Vodovoz.RDL.Rdl2005
{
	[Serializable()]
	[XmlType()]
	public partial class FilterValues
	{
		private string[] filterValueField;
		private XmlAttribute[] anyAttrField;

		[XmlElement("FilterValue")]
		public string[] FilterValue
		{
			get => filterValueField;
			set => filterValueField = value;
		}

		[XmlAnyAttribute()]
		public XmlAttribute[] AnyAttr
		{
			get => anyAttrField;
			set => anyAttrField = value;
		}
	}
}