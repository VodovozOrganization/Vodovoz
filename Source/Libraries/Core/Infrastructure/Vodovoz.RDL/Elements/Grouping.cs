using System.Collections.Generic;
using System.Xml.Serialization;
using Vodovoz.RDL.Elements.Base;

namespace Vodovoz.RDL.Elements
{
	public partial class Grouping : BaseElementWithEnumedItems<ItemsChoiceType17>
	{
		[XmlIgnore]
		public IList<CustomProperty> CustomProperties =>
			GetEnumedItemsList<CustomProperty, CustomProperties>(x => x.CustomProperty);

		[XmlIgnore]
		public string DataCollectionName
		{
			get => GetEnamedItemsValue<string>();
			set => SetEnamedItemsValue(value);
		}

		[XmlIgnore]
		public string DataElementName
		{
			get => GetEnamedItemsValue<string>();
			set => SetEnamedItemsValue(value);
		}

		[XmlIgnore]
		public GroupingTypeDataElementOutput DataElementOutput
		{
			get => GetEnamedItemsValue<GroupingTypeDataElementOutput>();
			set => SetEnamedItemsValue(value);
		}

		[XmlIgnore]
		public IList<Filter> Filters =>
			GetEnumedItemsList<Filter, Filters>(x => x.Filter);

		[XmlIgnore]
		public IList<string> GroupExpressions => 
			GetEnumedItemsList<string, GroupExpressions>(x => x.GroupExpression);

		[XmlIgnore]
		public string Label
		{
			get => GetEnamedItemsValue<string>();
			set => SetEnamedItemsValue(value);
		}

		[XmlIgnore]
		public bool PageBreakAtEnd
		{
			get => GetEnamedItemsValue<bool>();
			set => SetEnamedItemsValue(value);
		}

		[XmlIgnore]
		public bool PageBreakAtStart
		{
			get => GetEnamedItemsValue<bool>();
			set => SetEnamedItemsValue(value);
		}

		[XmlIgnore]
		public string Parent
		{
			get => GetEnamedItemsValue<string>();
			set => SetEnamedItemsValue(value);
		}

	}
}
