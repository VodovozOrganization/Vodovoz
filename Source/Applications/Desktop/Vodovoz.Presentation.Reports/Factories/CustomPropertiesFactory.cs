using System.Collections.Generic;
using Vodovoz.RDL.Elements;

namespace Vodovoz.Presentation.Reports.Factories
{
	public class CustomPropertiesFactory : ICustomPropertiesFactory
	{
		public CustomProperties CreateDefaultQrCustomProperties(string qrString)
		{
			return new CustomProperties
			{
				CustomProperty = new List<CustomProperty>
				{
					new CustomProperty
					{
						ItemsElementNameList = new List<ItemsChoiceType10>
						{
							ItemsChoiceType10.Name,
							ItemsChoiceType10.Value
						},
						ItemsList = new List<object>
						{
							"QrCode",
							qrString
						}
					}
				}
			};
		}
	}
}
