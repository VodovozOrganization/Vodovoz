using System;
using System.Xml.Serialization;
using System.Xml;

namespace Vodovoz.RDL.Rdl2005
{
	[Serializable()]
	[XmlType()]
	public partial class DataPoint
	{
		private object[] itemsField;
		private XmlAttribute[] anyAttrField;

		[XmlAnyElement()]
		[XmlElement("Action", typeof(Action))]
		[XmlElement("DataElementName", typeof(string))]
		[XmlElement("DataElementOutput", typeof(DataPointTypeDataElementOutput))]
		[XmlElement("DataLabel", typeof(DataLabel))]
		[XmlElement("DataValues", typeof(DataValues))]
		[XmlElement("Marker", typeof(Marker))]
		[XmlElement("Style", typeof(Style))]
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

	[Serializable()]
	[XmlType(AnonymousType = true)]
	public enum DataPointTypeDataElementOutput
	{
		Output,
		NoOutput,
	}

}