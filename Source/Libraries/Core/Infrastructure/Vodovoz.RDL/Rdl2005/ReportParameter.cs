using System;
using System.Xml.Serialization;
using System.Xml;

namespace Vodovoz.RDL.Rdl2005
{
	[Serializable()]
	[XmlType()]
	public partial class ReportParameter
	{
		private object[] itemsField;
		private ItemsChoiceType33[] itemsElementNameField;
		private string nameField;
		private XmlAttribute[] anyAttrField;

		[XmlAnyElement()]
		[XmlElement("AllowBlank", typeof(bool))]
		[XmlElement("DataType", typeof(ReportParameterTypeDataType))]
		[XmlElement("DefaultValue", typeof(DefaultValue))]
		[XmlElement("Hidden", typeof(bool))]
		[XmlElement("MultiValue", typeof(bool))]
		[XmlElement("Nullable", typeof(bool))]
		[XmlElement("Prompt", typeof(string))]
		[XmlElement("UsedInQuery", typeof(ReportParameterTypeUsedInQuery))]
		[XmlElement("ValidValues", typeof(ValidValues))]
		[XmlChoiceIdentifier("ItemsElementName")]
		public object[] Items
		{
			get => itemsField;
			set => itemsField = value;
		}

		[XmlElement("ItemsElementName")]
		[XmlIgnore()]
		public ItemsChoiceType33[] ItemsElementName
		{
			get => itemsElementNameField;
			set => itemsElementNameField = value;
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
	[XmlType(IncludeInSchema = false)]
	public enum ItemsChoiceType33
	{
		[XmlEnum("##any:")]
		Item,
		AllowBlank,
		DataType,
		DefaultValue,
		Hidden,
		MultiValue,
		Nullable,
		Prompt,
		UsedInQuery,
		ValidValues,
	}

	[Serializable()]
	[XmlType(AnonymousType = true)]
	public enum ReportParameterTypeDataType
	{
		Boolean,
		DateTime,
		Integer,
		Float,
		String,
	}

	[Serializable()]
	[XmlType(AnonymousType = true)]
	public enum ReportParameterTypeUsedInQuery
	{
		False,
		True,
		Auto,
	}
}