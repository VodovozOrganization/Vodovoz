using System;
using System.Xml.Serialization;
using System.Xml;

namespace Vodovoz.RDL.Rdl2005
{
	[Serializable()]
	[XmlType()]
	public partial class DataSet
	{
		private object[] itemsField;
		private string nameField;
		private XmlAttribute[] anyAttrField;

		[XmlAnyElement()]
		[XmlElement("AccentSensitivity", typeof(DataSetTypeAccentSensitivity))]
		[XmlElement("CaseSensitivity", typeof(DataSetTypeCaseSensitivity))]
		[XmlElement("Collation", typeof(string))]
		[XmlElement("Fields", typeof(Fields))]
		[XmlElement("Filters", typeof(Filters))]
		[XmlElement("KanatypeSensitivity", typeof(DataSetTypeKanatypeSensitivity))]
		[XmlElement("Query", typeof(Query))]
		[XmlElement("WidthSensitivity", typeof(DataSetTypeWidthSensitivity))]
		public object[] Items
		{
			get => itemsField;
			set => itemsField = value;
		}

		[XmlAttribute(DataType = "normalizedString")]
		public string Name
		{
			get => nameField;
			set => nameField = value;
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
	public enum DataSetTypeAccentSensitivity
	{
		True,
		False,
		Auto,
	}

	[Serializable()]
	[XmlType(AnonymousType = true)]
	public enum DataSetTypeCaseSensitivity
	{
		True,
		False,
		Auto,
	}

	[Serializable()]
	[XmlType(AnonymousType = true)]
	public enum DataSetTypeKanatypeSensitivity
	{
		True,
		False,
		Auto,
	}

	[Serializable()]
	[XmlType(AnonymousType = true)]
	public enum DataSetTypeWidthSensitivity
	{
		True,
		False,
		Auto,
	}
}