﻿using System;
using System.Xml.Serialization;
using System.Xml;
using System.Linq;
using System.Collections.Generic;

namespace Vodovoz.RDL.Elements
{
	[Serializable()]
	[XmlType()]
	public partial class TableCell
	{
		private List<object> itemsField = new List<object>();
		private List<XmlAttribute> anyAttrField = new List<XmlAttribute>();

		[XmlIgnore()]
		public override List<object> ItemsList
		{
			get => itemsField;
			set => itemsField = value;
		}

		[XmlAnyElement()]
		[XmlElement("ColSpan", typeof(uint))]
		[XmlElement("ReportItems", typeof(ReportItems))]
		public object[] Items
		{
			get => ItemsList.ToArray();
			set => ItemsList = value == null ? new List<object>() : value.ToList();
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
}
