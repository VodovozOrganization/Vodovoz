using QS.DomainModel.Entity;
using System;
using System.Reflection;

namespace Vodovoz.Tools
{
	public static class TypeUtils
	{
		public static AppellativeAttribute GetClassUserFriendlyName(this Type t)
			=> t.GetCustomAttribute<AppellativeAttribute>(true);
	}
}
