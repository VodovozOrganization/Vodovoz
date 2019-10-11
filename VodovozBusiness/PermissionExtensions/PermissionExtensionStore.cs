using System;
using System.Collections.Generic;
using System.Linq;

namespace Vodovoz.PermissionExtensions
{
	public class PermissionExtensionStore
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		private SortedList<string, IPermissionExtension> permissionExtensions;
		public SortedList<string, IPermissionExtension> PermissionExtensions {
			get 
			{
				if(permissionExtensions == null)
					permissionExtensions = GetExtensions();

				return permissionExtensions;
			}
		}

		protected SortedList<string,IPermissionExtension> GetExtensions()
		{
			SortedList<string, IPermissionExtension> extensions = new SortedList<string, IPermissionExtension>(StringComparer.Ordinal);
			Type parent = typeof(IPermissionExtension);
			IEnumerable<Type> types = new List<Type>();

			foreach(var assembly in AppDomain.CurrentDomain.GetAssemblies()) {
				var list = assembly.GetTypes().Where(x => parent.IsAssignableFrom(x) && !x.IsAbstract);
				if(list?.FirstOrDefault() != null)
					types = types.Concat(list);
			}
			foreach(var item in types) {
				try 
				{
					if(Activator.CreateInstance(item) is IPermissionExtension instance)
						extensions.Add(instance.PermissionId ,instance);
				}
				catch(MissingMethodException ex) {
					logger.Error(ex, $"Ошибка при создании экземпляра класса {item.Name}, у класса отсутствует пустой конструктор");
					continue;
				}
			}

			return extensions;
		}


	}
}
