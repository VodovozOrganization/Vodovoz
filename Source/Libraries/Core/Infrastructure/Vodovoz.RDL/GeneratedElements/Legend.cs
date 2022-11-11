using System;
using System.Xml.Serialization;
using System.Xml;
using System.Linq;
using System.Collections.Generic;

namespace Vodovoz.RDL.Elements
{
	[Serializable()]
	[XmlType()]
	public partial class Legend
	{
		private List<object> itemsField = new List<object>();
		private List<ItemsChoiceType24> itemsElementNameField = new List<ItemsChoiceType24>();
		private List<XmlAttribute> anyAttrField = new List<XmlAttribute>();

		[XmlIgnore()]
		public List<object> ItemsList
		{
			get => itemsField;
			set => itemsField = value;
		}

		[XmlAnyElement()]
		[XmlElement("InsidePlotArea", typeof(bool))]
		[XmlElement("Layout", typeof(LegendTypeLayout))]
		[XmlElement("Position", typeof(LegendTypePosition))]
		[XmlElement("Style", typeof(Style))]
		[XmlElement("Visible", typeof(bool))]
		[XmlChoiceIdentifier("ItemsElementName")]
		public object[] Items
		{
			get => ItemsList.ToArray();
			set => ItemsList = value == null ? new List<object>() : value.ToList();
		}

		[XmlIgnore()]
		public List<ItemsChoiceType24> ItemsElementNameList
		{
			get => itemsElementNameField;
			set => itemsElementNameField = value;
		}

		[XmlElement("ItemsElementName")]
		[XmlIgnore()]
		public ItemsChoiceType24[] ItemsElementName
		{
			get => ItemsElementNameList.ToArray();
			set => ItemsElementNameList = value == null ? new List<ItemsChoiceType24>() : value.ToList();
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
	public enum LegendTypeLayout
	{
		Column,
		Row,
		Table,
	}

	[Serializable()]
	[XmlType(AnonymousType = true)]
	public enum LegendTypePosition
	{
		TopLeft,
		TopCenter,
		TopRight,
		LeftTop,
		LeftCenter,
		LeftBottom,
		RightTop,
		RightCenter,
		RightBottom,
		BottomLeft,
		BottomCenter,
		BottomRight,
	}

	[Serializable()]
	[XmlType(IncludeInSchema = false)]
	public enum ItemsChoiceType24
	{
		[XmlEnum("##any:")]
		Item,
		InsidePlotArea,
		Layout,
		Position,
		Style,
		Visible,
	}
}
