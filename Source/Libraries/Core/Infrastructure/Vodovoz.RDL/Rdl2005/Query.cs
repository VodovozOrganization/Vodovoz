using System;
using System.Xml.Serialization;
using System.Xml;

namespace Vodovoz.RDL.Rdl2005
{
	[Serializable()]
	[XmlType()]
	public partial class Query
	{
		private object[] itemsField;
		private ItemsChoiceType2[] itemsElementNameField;
		private XmlAttribute[] anyAttrField;

		[XmlAnyElement()]
		[XmlElement("CommandText", typeof(string))]
		[XmlElement("CommandType", typeof(QueryTypeCommandType))]
		[XmlElement("DataSourceName", typeof(string))]
		[XmlElement("QueryParameters", typeof(QueryParameters))]
		[XmlElement("Timeout", typeof(uint))]
		[XmlChoiceIdentifier("ItemsElementName")]
		public object[] Items
		{
			get => itemsField;
			set => itemsField = value;
		}

		[XmlElement("ItemsElementName")]
		[XmlIgnore()]
		public ItemsChoiceType2[] ItemsElementName
		{
			get => itemsElementNameField;
			set => itemsElementNameField = value;
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
	public enum QueryTypeCommandType
	{
		Text,
		StoredProcedure,
		TableDirect,
	}

	[Serializable()]
	[XmlType(IncludeInSchema = false)]
	public enum ItemsChoiceType2
	{
		[XmlEnum("##any:")]
		Item,
		CommandText,
		CommandType,
		DataSourceName,
		QueryParameters,
		Timeout,
	}
}