using QS.DomainModel.UoW;
using Vodovoz.Tools.CommerceML;

namespace VodovozMobileService
{
	public static class BackgroundTask
	{
		static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();

		public static void OnlineStoreCatalogSync()
		{
			using(var uow = UnitOfWorkFactory.CreateWithoutRoot($"[BT]Синхронизация каталога товаров с интернет магазином")) {
				var export = new Export(uow);
				export.RunToSite();
			}
		}
	}
}

