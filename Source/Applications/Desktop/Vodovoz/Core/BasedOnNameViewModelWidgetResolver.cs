using System;
using Gtk;
using QS.Tdi;
using System.Reflection;
using QS.HistoryLog;
using QS.Banks.Domain;

namespace Vodovoz.Core
{
	public class BasedOnNameViewModelWidgetResolver : ViewModelWidgetResolver
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
		private Assembly[] usedAssemblies;

		public BasedOnNameViewModelWidgetResolver()
		{
			usedAssemblies = new Assembly[] {
				Assembly.GetAssembly(typeof(QS.Project.HibernateMapping.UserBaseMap)),
				Assembly.GetAssembly(typeof(HibernateMapping.Organizations.OrganizationMap)),
				Assembly.GetAssembly(typeof(Bank)),
				Assembly.GetAssembly(typeof(HistoryMain)),
				Assembly.GetAssembly(typeof(MainWindow)),
				Assembly.GetAssembly(typeof(VodovozViewModelAssemblyFinder))
			};
		}

		public override Widget Resolve(ITdiTab tab)
		{
			try {
				return base.Resolve(tab);
			} catch(WidgetResolveException ex) {
				try {
					var baseOnNameResolver = new BasedOnNameTDIResolver(usedAssemblies);
					return baseOnNameResolver.Resolve(tab);
				} catch(Exception e) {
					throw new InvalidProgramException($"Невозможно найти View для ViewModel вкладки: {tab.TabName}. Имя класса ViewModel: {tab.GetType()} не соответствует шаблону именования или не настроено правильное сопоставление");
				}
			}
		}

	}
}
