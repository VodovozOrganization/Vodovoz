using System;
using System.Xml.Serialization;
using System.Xml;

namespace Vodovoz.RDL.Rdl2005
{
	[Serializable()]
	[XmlType()]
	public partial class ParameterValues
	{
		private ParameterValue[] parameterValueField;
		private XmlAttribute[] anyAttrField;

		[XmlElement("ParameterValue")]
		public ParameterValue[] ParameterValue
		{
			get => parameterValueField;
			set => parameterValueField = value;
		}

		[XmlAnyAttribute()]
		public XmlAttribute[] AnyAttr
		{
			get => anyAttrField;
			set => anyAttrField = value;
		}
	}
}