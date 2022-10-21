using System;
using System.Xml.Serialization;
using System.Xml;

namespace Vodovoz.RDL.Rdl2005
{
	[Serializable()]
	[XmlType()]
	public partial class Classes
	{
		private Class[] classField;
		private XmlAttribute[] anyAttrField;

		[XmlElement("Class")]
		public Class[] Class
		{
			get => classField;
			set => classField = value;
		}

		[XmlAnyAttribute()]
		public XmlAttribute[] AnyAttr
		{
			get => anyAttrField;
			set => anyAttrField = value;
		}
	}
}