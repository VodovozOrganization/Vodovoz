using System.Xml.Serialization;
using Vodovoz.RDL.Elements.Base;

namespace Vodovoz.RDL.Elements
{
	public partial class Report : BaseElementWithEnumedItems<ItemsChoiceType37>
	{
		[XmlIgnore]
		public Body Body
		{
			get => GetEnamedItemsValue<Body>();
			set => SetEnamedItemsValue(value);
		}
	}
}
