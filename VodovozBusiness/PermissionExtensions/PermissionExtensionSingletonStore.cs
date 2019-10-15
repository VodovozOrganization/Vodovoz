using System;
using System.Collections.Generic;
using System.Linq;

namespace Vodovoz.PermissionExtensions
{
	public class PermissionExtensionSingletonStore
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		private static PermissionExtensionSingletonStore instance;

		public static PermissionExtensionSingletonStore GetInstance()
		{
			if(instance == null)
				instance = new PermissionExtensionSingletonStore();
			return instance;
		}

		protected PermissionExtensionSingletonStore() { }

		private IList<IPermissionExtension> permissionExtensions;
		public IList<IPermissionExtension> PermissionExtensions {
			get 
			{
				if(permissionExtensions == null)
					permissionExtensions = GetExtensions();

				return permissionExtensions;
			}
		}

		protected IList<IPermissionExtension> GetExtensions()
		{
			IList<IPermissionExtension> extensions = new List<IPermissionExtension>();
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
						extensions.Add(instance);
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
