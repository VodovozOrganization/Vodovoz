using System;
using System.Xml.Serialization;
using System.Xml;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;

namespace Vodovoz.RDL.Elements
{
	[Serializable()]
	[XmlType()]
	public partial class CustomReportItem : IReportItemElement
	{
		private List<object> itemsField = new List<object>();
		private List<ItemsChoiceType29> itemsElementNameField = new List<ItemsChoiceType29>();
		private string nameField;
		private List<XmlAttribute> anyAttrField = new List<XmlAttribute>();

		[XmlIgnore()]
		public List<object> ItemsList
		{
			get => itemsField;
			set => itemsField = value;
		}

		[XmlAnyElement()]
		[XmlElement("AltReportItem", typeof(ReportItems))]
		[XmlElement("Bookmark", typeof(string))]
		[XmlElement("CustomData", typeof(CustomData))]
		[XmlElement("CustomProperties", typeof(CustomProperties))]
		[XmlElement("DataElementName", typeof(string))]
		[XmlElement("DataElementOutput", typeof(CustomReportItemTypeDataElementOutput))]
		[XmlElement("Label", typeof(string))]
		[XmlElement("RepeatWith", typeof(string))]
		[XmlElement("Style", typeof(Style))]
		[XmlElement("Type", typeof(string))]
		[XmlElement("Visibility", typeof(Visibility))]
		[XmlElement("ZIndex", typeof(uint))]
		[XmlChoiceIdentifier("ItemsElementName")]
		public object[] Items
		{
			get => ItemsList.ToArray();
			set => ItemsList = value == null ? new List<object>() : value.ToList();
		}

		[XmlIgnore()]
		public List<ItemsChoiceType29> ItemsElementNameList
		{
			get => itemsElementNameField;
			set => itemsElementNameField = value;
		}

		[XmlElement("ItemsElementName")]
		[XmlIgnore()]
		public ItemsChoiceType29[] ItemsElementName
		{
			get => ItemsElementNameList.ToArray();
			set => ItemsElementNameList = value == null ? new List<ItemsChoiceType29>() : value.ToList();
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

		private string _top;

		public string Top
		{
			get => _top;
			set
			{
				_top = value;
				ParseTopValue(_top);
			}
		}
		
		[XmlIgnore]
		public decimal TopSize { get; set; }
		[XmlIgnore]
		public string TopDimension { get; set; }
		
		private string _left;

		public string Left
		{
			get => _left;
			set
			{
				_left = value;
				ParseLeftValue(_left);
			}
		}

		[XmlIgnore]
		public decimal LeftSize { get; set; }
		[XmlIgnore]
		public string LeftDimension { get; set; }

		public void ParseTopValue(string top)
		{
			TopDimension = top.Substring(top.Length - 2, 2);
			var size = top.Substring(0, top.Length - 2);
			TopSize = decimal.Parse(size, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture);
		}
		
		public void ParseLeftValue(string left)
		{
			LeftDimension = left.Substring(left.Length - 2, 2);
			var size = left.Substring(0, left.Length - 2);
			LeftSize = decimal.Parse(size, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture);
		}
		
		private string _height;

		public string Height
		{
			get => _height;
			set
			{
				_height = value;
				ParseHeightValue(_height);
			}
		}
		
		[XmlIgnore]
		public decimal HeightSize { get; set; }
		[XmlIgnore]
		public string HeightDimension { get; set; }

		private string _width;

		public string Width
		{
			get => _width;
			set
			{
				_width = value;
				ParseWidthValue(_width);
			}
		}
		
		[XmlIgnore]
		public decimal WidthSize { get; set; }
		[XmlIgnore]
		public string WidthDimension { get; set; }

		public void ParseHeightValue(string height)
		{
			HeightDimension = height.Substring(height.Length - 2, 2);
			var size = height.Substring(0, height.Length - 2);
			HeightSize = decimal.Parse(size, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture);
		}
		
		public void ParseWidthValue(string width)
		{
			WidthDimension = width.Substring(width.Length - 2, 2);
			var size = width.Substring(0, width.Length - 2);
			WidthSize = decimal.Parse(size, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture);
		}
	}

	[Serializable()]
	[XmlType(IncludeInSchema = false)]
	public enum ItemsChoiceType29
	{
		[XmlEnum("##any:")]
		Item,
		AltReportItem,
		Bookmark,
		CustomData,
		CustomProperties,
		DataElementName,
		DataElementOutput,
		Label,
		RepeatWith,
		Style,
		Type,
		Visibility,
		ZIndex,
	}

	[Serializable()]
	[XmlType(AnonymousType = true)]
	public enum CustomReportItemTypeDataElementOutput
	{
		Output,
		NoOutput,
		ContentsOnly,
		Auto,
	}
}
