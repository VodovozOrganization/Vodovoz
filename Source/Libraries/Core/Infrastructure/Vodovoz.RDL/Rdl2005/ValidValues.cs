using System;
using System.Xml.Serialization;
using System.Xml;

namespace Vodovoz.RDL.Rdl2005
{
	[Serializable()]
	[XmlType()]
	public partial class ValidValues
	{
		private object[] itemsField;
		private XmlAttribute[] anyAttrField;

		[XmlAnyElement()]
		[XmlElement("DataSetReference", typeof(DataSetReference))]
		[XmlElement("ParameterValues", typeof(ParameterValues))]
		public object[] Items
		{
			get => itemsField;
			set => itemsField = value;
		}

		[XmlAnyAttribute()]
		public XmlAttribute[] AnyAttr
		{
			get => anyAttrField;
			set => anyAttrField = value;
		}
	}
}