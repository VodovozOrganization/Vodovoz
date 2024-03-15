using System;

namespace Vodovoz.Settings.Database
{
	[AttributeUsage(AttributeTargets.Property)]
	public class CustomSettingPropertyNameAttribute : Attribute
	{
		public CustomSettingPropertyNameAttribute(string customPropertyName)
		{
			CustomPropertyName = customPropertyName;
		}

		public string CustomPropertyName { get; }
	}
}
