using System;
using System.Reflection;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.ViewModels;
using Vodovoz.Domain.Goods;
using QS.DomainModel.Entity;

namespace Vodovoz.ViewModels.ViewModels.Goods
{
	public class NomenclatureOnlineCatalogViewModel : DialogTabViewModelBase
	{
		private readonly IEntityUoWBuilder _uowBuilder;

		public NomenclatureOnlineCatalogViewModel(
			IEntityUoWBuilder uowBuilder,
			Type entityType,
			IUnitOfWorkFactory uowFactory,
			IInteractiveService interactiveService,
			INavigationManager navigationManager) : base(uowFactory, interactiveService, navigationManager)
		{
			_uowBuilder = uowBuilder ?? throw new ArgumentNullException(nameof(uowBuilder));

			TabName = entityType.GetCustomAttribute<AppellativeAttribute>(true)?.Nominative;
			InitializeUow(entityType);
		}

		public bool CanShowId => !UoW.IsNew;
		public string IdString => Entity.Id.ToString();
		public NomenclatureOnlineCatalog Entity { get; private set; }

		private void InitializeUow(Type entityType)
		{
			if(entityType == typeof(VodovozWebSiteNomenclatureOnlineCatalog))
			{
				var uow = _uowBuilder.CreateUoW<VodovozWebSiteNomenclatureOnlineCatalog>(UnitOfWorkFactory);
				UoW = uow;
				Entity = uow.Root;
			}
			else if(entityType == typeof(MobileAppNomenclatureOnlineCatalog))
			{
				var uow = _uowBuilder.CreateUoW<MobileAppNomenclatureOnlineCatalog>(UnitOfWorkFactory);
				UoW = uow;
				Entity = uow.Root;
			}
			else if(entityType == typeof(KulerSaleWebSiteNomenclatureOnlineCatalog))
			{
				var uow = _uowBuilder.CreateUoW<KulerSaleWebSiteNomenclatureOnlineCatalog>(UnitOfWorkFactory);
				UoW = uow;
				Entity = uow.Root;
			}
			else
			{
				throw new InvalidOperationException();
			}
		}
	}
}
