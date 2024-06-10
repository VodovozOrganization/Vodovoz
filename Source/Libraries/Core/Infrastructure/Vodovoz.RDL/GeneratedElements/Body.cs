using System;
using System.Xml.Serialization;
using System.Xml;
using System.Linq;
using System.Collections.Generic;
using Vodovoz.RDL.Elements.Base;

namespace Vodovoz.RDL.Elements
{
	[Serializable]
	[XmlType]
	public partial class Body : BaseElementWithEnumedItems<ItemsChoiceType30>
	{
		private List<object> _itemsField = new List<object>();
		private List<ItemsChoiceType30> _itemsElementNameField = new List<ItemsChoiceType30>();
		private List<XmlAttribute> _anyAttrField = new List<XmlAttribute>();

		[XmlIgnore]
		public override List<object> ItemsList
		{
			get => _itemsField;
			set => _itemsField = value;
		}

		[XmlAnyElement]
		[XmlElement("ColumnSpacing", typeof(string), DataType = "normalizedString")]
		[XmlElement("Columns", typeof(uint))]
		[XmlElement("Height", typeof(string), DataType = "normalizedString")]
		[XmlElement("ReportItems", typeof(ReportItems))]
		[XmlElement("Style", typeof(Style))]
		[XmlChoiceIdentifier("ItemsElementName")]
		public object[] Items
		{
			get => ItemsList.ToArray();
			set => ItemsList = value == null ? new List<object>() : value.ToList();
		}

		[XmlIgnore]
		public override List<ItemsChoiceType30> ItemsElementNameList
		{
			get => _itemsElementNameField;
			set => _itemsElementNameField = value;
		}

		[XmlElement("ItemsElementName")]
		[XmlIgnore]
		public ItemsChoiceType30[] ItemsElementName
		{
			get => ItemsElementNameList.ToArray();
			set => ItemsElementNameList = value == null ? new List<ItemsChoiceType30>() : value.ToList();
		}

		[XmlIgnore]
		public List<XmlAttribute> AnyAttrList
		{
			get => _anyAttrField;
			set => _anyAttrField = value;
		}

		[XmlAnyAttribute]
		public XmlAttribute[] AnyAttr
		{
			get => AnyAttrList.ToArray();
			set => AnyAttrList = value == null ? new List<XmlAttribute>() : value.ToList();
		}
	}

	[Serializable]
	[XmlType(IncludeInSchema = false)]
	public enum ItemsChoiceType30
	{
		[XmlEnum("##any:")]
		Item,
		ColumnSpacing,
		Columns,
		Height,
		ReportItems,
		Style,
	}
}
