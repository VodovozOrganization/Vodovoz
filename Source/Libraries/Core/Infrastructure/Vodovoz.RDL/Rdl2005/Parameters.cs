using System;
using System.Xml.Serialization;
using System.Xml;

namespace Vodovoz.RDL.Rdl2005
{
	[Serializable()]
	[XmlType()]
	public partial class Parameters
	{
		private Parameter[] parameterField;
		private XmlAttribute[] anyAttrField;

		[XmlElement("Parameter")]
		public Parameter[] Parameter
		{
			get => parameterField;
			set => parameterField = value;
		}

		[XmlAnyAttribute()]
		public XmlAttribute[] AnyAttr
		{
			get => anyAttrField;
			set => anyAttrField = value;
		}
	}
}