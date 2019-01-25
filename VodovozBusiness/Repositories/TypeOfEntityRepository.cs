using System;
using System.Linq;
using System.Reflection;
using QS.DomainModel.Entity;

namespace Vodovoz.Repositories
{
	public static class TypeOfEntityRepository
	{
		public static string GetRealName(Type type)
		{
			var result = type?.GetCustomAttributes(typeof(AppellativeAttribute), false)
				.Cast<AppellativeAttribute>()
				.FirstOrDefault()
				.Nominative;

			return result;
		}
		
		public static Type GetEntityType(string strType)
		{
			var items = Assembly.GetAssembly(typeof(TypeOfEntity)).GetTypes();
			return items.FirstOrDefault(t => t.Name == strType);
		}
	}
}
