﻿using System;
using System.Xml.Serialization;
using System.Xml;
using System.Linq;
using System.Collections.Generic;

namespace Vodovoz.RDL.Elements
{
	[Serializable()]
	[XmlType()]
	public partial class DataRows
	{
		private List<DataRow> dataRowField = new List<DataRow>();
		private List<XmlAttribute> anyAttrField = new List<XmlAttribute>();

		[XmlElement("DataRow")]
		public List<DataRow> DataRow
		{
			get => dataRowField;
			set => dataRowField = value;
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
