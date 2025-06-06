using NHibernate.Transform;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Warehouses;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Infrastructure.Report.SelectableParametersFilter;
using Vodovoz.ViewModels.Widgets;

namespace Vodovoz.ViewModels.Factories
{
	public class SelectableParametersFilterViewModelFactory : ISelectableParametersFilterViewModelFactory
	{
		public SelectableParametersFilterViewModel CreateProductGroupsSelectableParametersFilterViewModel(IUnitOfWork uow, string title)
		{
			var parametersFactory = new RecursiveParametersFactory<ProductGroup>(
				uow,
				filter =>
				{
					var query = uow.Session.QueryOver<ProductGroup>()
						.Where(p => p.Parent == null);

					if(uow.IsNew)
					{
						query.And(p => !p.IsArchive);
					}
					return	query.List();
				},
				x => x.Name,
				x => x.Childs);
			
			return new SelectableParametersFilterViewModel(parametersFactory, title);
		}
		
		public SelectableParametersFilterViewModel CreateWarehousesSelectableParametersFilterViewModel(IUnitOfWork uow, string title)
		{
			var parametersFactory = new ParametersFactory(
				uow,
				filter =>
				{
					SelectableEntityParameter<Warehouse> resultAlias = null;
					
					return uow.Session.QueryOver<Warehouse>()
						.SelectList(list => list
							.Select(x => x.Id).WithAlias(() => resultAlias.EntityId)
							.Select(x => x.Name).WithAlias(() => resultAlias.EntityTitle)
						).TransformUsing(Transformers.AliasToBean<SelectableEntityParameter<Warehouse>>())
						.List<SelectableParameter>();
				});
			
			return new SelectableParametersFilterViewModel(parametersFactory, title);
		}
		
		public SelectableParametersFilterViewModel CreateCarEventTypesSelectableParametersFilterViewModel(IUnitOfWork uow, string title)
		{
			var parametersFactory = new ParametersFactory(
				uow,
				filter =>
				{
					SelectableEntityParameter<CarEventType> resultAlias = null;
					
					return uow.Session.QueryOver<CarEventType>()
						.SelectList(list => list
							.Select(x => x.Id).WithAlias(() => resultAlias.EntityId)
							.Select(x => x.Name).WithAlias(() => resultAlias.EntityTitle)
						).TransformUsing(Transformers.AliasToBean<SelectableEntityParameter<CarEventType>>())
						.List<SelectableParameter>();
				});
			
			return new SelectableParametersFilterViewModel(parametersFactory, title);
		}
	}
}
