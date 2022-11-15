using System;
using System.Xml.Serialization;
using System.Xml;
using System.Linq;
using System.Collections.Generic;

namespace Vodovoz.RDL.Elements
{
	[Serializable()]
	[XmlType()]
	public partial class ReportParameter
	{
		private List<object> itemsField = new List<object>();
		private List<ItemsChoiceType33> itemsElementNameField = new List<ItemsChoiceType33>();
		private string nameField;
		private List<XmlAttribute> anyAttrField = new List<XmlAttribute>();

		[XmlIgnore()]
		public List<object> ItemsList
		{
			get => itemsField;
			set => itemsField = value;
		}

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
			get => ItemsList.ToArray();
			set => ItemsList = value == null ? new List<object>() : value.ToList();
		}

		[XmlIgnore()]
		public List<ItemsChoiceType33> ItemsElementNameList
		{
			get => itemsElementNameField;
			set => itemsElementNameField = value;
		}

		[XmlElement("ItemsElementName")]
		[XmlIgnore()]
		public ItemsChoiceType33[] ItemsElementName
		{
			get => ItemsElementNameList.ToArray();
			set => ItemsElementNameList = value == null ? new List<ItemsChoiceType33>() : value.ToList();
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
