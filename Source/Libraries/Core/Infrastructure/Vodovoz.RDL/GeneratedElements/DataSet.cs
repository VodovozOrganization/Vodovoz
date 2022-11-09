using System;
using System.Xml.Serialization;
using System.Xml;
using System.Linq;
using System.Collections.Generic;

namespace Vodovoz.RDL.Elements
{
	[Serializable()]
	[XmlType()]
	public partial class DataSet
	{
		private List<object> itemsField = new List<object>();
		private string nameField;
		private List<XmlAttribute> anyAttrField = new List<XmlAttribute>();

		[XmlIgnore()]
		public List<object> ItemsList
		{
			get => itemsField;
			set => itemsField = value;
		}

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
			get => ItemsList.ToArray();
			set => ItemsList = value == null ? new List<object>() : value.ToList();
		}

		[XmlAttribute(DataType = "normalizedString")]
		public string Name
		{
			get => nameField;
			set => nameField = value;
		}

		[XmlIgnore()]
		public List<XmlAttribute> AnyAttrList
		{
			get => anyAttrField;
			set => anyAttrField = value;
		}

		[XmlAnyAttribute()]
		public XmlAttribute[] AnyAttr
		{
			get => AnyAttrList.ToArray();
			set => AnyAttrList = value == null ? new List<XmlAttribute>() : value.ToList();
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