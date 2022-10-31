using System;
using System.Xml.Serialization;
using System.Xml;

namespace Vodovoz.RDL.Rdl2005
{
	[Serializable()]
	[XmlType()]
	public partial class DataSets
	{
		private DataSet[] dataSetField;
		private XmlAttribute[] anyAttrField;

		[XmlElement("DataSet")]
		public DataSet[] DataSet
		{
			get => dataSetField;
			set => dataSetField = value;
		}

		[XmlAnyAttribute()]
		public XmlAttribute[] AnyAttr
		{
			get => anyAttrField;
			set => anyAttrField = value;
		}
	}
}