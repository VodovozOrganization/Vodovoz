using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using Vodovoz.RDL.Elements.Base;

namespace Vodovoz.RDL.Elements
{
	public partial class ReportItems : BaseElementWithItems
	{
		[XmlIgnore]
		public Textbox Textbox
		{
			get => GetItemsValue<Textbox>();
			set => SetItemsValue(value);
		}
	}
}
