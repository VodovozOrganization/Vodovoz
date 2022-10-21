using System;
using System.Xml.Serialization;
using System.Xml;

namespace Vodovoz.RDL.Rdl2005
{
	[Serializable()]
	[XmlType()]
	public partial class QueryParameters
	{
		private QueryParameter[] queryParameterField;
		private XmlAttribute[] anyAttrField;

		[XmlElement("QueryParameter")]
		public QueryParameter[] QueryParameter
		{
			get => queryParameterField;
			set => queryParameterField = value;
		}

		[XmlAnyAttribute()]
		public XmlAttribute[] AnyAttr
		{
			get => anyAttrField;
			set => anyAttrField = value;
		}
	}
}