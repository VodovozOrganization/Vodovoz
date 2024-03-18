using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Vodovoz.Extensions
{
	public static class EnumExtensions
	{
		public static string GetEnumDisplayName<TEnum>(this TEnum currentEnum, bool useShortName = false)
			where TEnum : Enum
		{
			var enumType = typeof(TEnum);
			var memberInfos = enumType.GetMember(currentEnum.ToString());
			var enumValueMemberInfo = memberInfos.FirstOrDefault(m => m.DeclaringType == enumType);
			var valueAttributes = enumValueMemberInfo.GetCustomAttributes(typeof(DisplayAttribute), false);
			var displayAttrib = (DisplayAttribute)valueAttributes[0];
			return useShortName ? displayAttrib.ShortName : displayAttrib.Name;
		}
	}
}
