using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.ViewModels;
using Vodovoz.Domain.Goods;
using QS.DomainModel.Entity;
using QS.Services;

namespace Vodovoz.ViewModels.ViewModels.Goods
{
	public class NomenclatureOnlineCatalogViewModel : DialogTabViewModelBase
	{
		private readonly IEntityUoWBuilder _uowBuilder;
		private readonly ICommonServices _commonServices;
		private readonly ValidationContext _validationContext;

		public NomenclatureOnlineCatalogViewModel(
			IEntityUoWBuilder uowBuilder,
			Type entityType,
			IUnitOfWorkFactory uowFactory,
			ICommonServices commonServices,
			INavigationManager navigationManager) : base(uowFactory, commonServices?.InteractiveService, navigationManager)
		{
			_uowBuilder = uowBuilder ?? throw new ArgumentNullException(nameof(uowBuilder));
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));

			TabName = entityType.GetCustomAttribute<AppellativeAttribute>(true)?.Nominative;
			InitializeUow(entityType);
			_validationContext = new ValidationContext(Entity);
		}

		public bool CanShowId => !UoW.IsNew;
		public string IdString => Entity.Id.ToString();
		public NomenclatureOnlineCatalog Entity { get; private set; }

		public override bool Save(bool needClose)
		{
			return Validate() && base.Save(needClose);
		}

		private bool Validate()
		{
			return _commonServices.ValidationService.Validate(Entity, _validationContext);
		}

		private void InitializeUow(Type entityType)
		{
			if(entityType == typeof(VodovozWebSiteNomenclatureOnlineCatalog))
			{
				var unitOfWorkGeneric = _uowBuilder.CreateUoW<VodovozWebSiteNomenclatureOnlineCatalog>(UnitOfWorkFactory);
				UoW = unitOfWorkGeneric;
				Entity = unitOfWorkGeneric.Root;
			}
			else if(entityType == typeof(MobileAppNomenclatureOnlineCatalog))
			{
				var unitOfWorkGeneric = _uowBuilder.CreateUoW<MobileAppNomenclatureOnlineCatalog>(UnitOfWorkFactory);
				UoW = unitOfWorkGeneric;
				Entity = unitOfWorkGeneric.Root;
			}
			else if(entityType == typeof(KulerSaleWebSiteNomenclatureOnlineCatalog))
			{
				var unitOfWorkGeneric = _uowBuilder.CreateUoW<KulerSaleWebSiteNomenclatureOnlineCatalog>(UnitOfWorkFactory);
				UoW = unitOfWorkGeneric;
				Entity = unitOfWorkGeneric.Root;
			}
			else
			{
				throw new InvalidOperationException();
			}
		}
	}
}
