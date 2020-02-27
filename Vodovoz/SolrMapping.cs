using System;
using System.Reflection;
using QS.Project.DB;
using SolrSearch.Mapping;
namespace Vodovoz
{
	public static class SolrMapping
	{
		private static SolrOrmSourceMapping ormMapping;
		public static SolrOrmSourceMapping OrmMapping {
			get {
				if(ormMapping == null) {
					var assemblies = new Assembly[] {
						Assembly.GetAssembly(typeof(VodovozViewModelAssemblyFinder))
					};
					ormMapping = new SolrOrmSourceMapping(new NhibernateMappingInfoProvider(OrmConfig.NhConfig), assemblies: assemblies);
				}
				return ormMapping;
			}
		}
	}
}
