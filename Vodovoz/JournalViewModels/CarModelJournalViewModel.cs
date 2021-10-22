using System;
using Gamma.Utilities;
using NHibernate;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Services;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Filters.ViewModels;
using Vodovoz.JournalNodes;
using Vodovoz.ViewModels.ViewModels.Logistic;
namespace Vodovoz.JournalViewModels
{
	public class CarModelJournalViewModel : SingleEntityJournalViewModelBase<CarModel, ModelCarViewModel, CarModelJournalNode>
	{
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;

		public CarModelJournalViewModel(CarModelJournalFilterViewModel filterViewModel, IUnitOfWorkFactory unitOfWorkFactory, ICommonServices commonServices, bool hideJournalForOpenDialog = false, bool hideJournalForCreateDialog = false) : base(unitOfWorkFactory, commonServices, hideJournalForOpenDialog, hideJournalForCreateDialog)
		{
			TabName = "Журнал моделей автомобилей";
			_unitOfWorkFactory = unitOfWorkFactory;
		}

		protected override Func<IUnitOfWork, IQueryOver<CarModel>> ItemsSourceQueryFunction => (uow) =>
		{
			CarModel carModelAlias = null;
			CarModelJournalNode resultNode = null;
			ManufacturerCars manufacturerCarsAlias = null;
			
			var query = uow.Session.QueryOver(() => carModelAlias).WhereNot(x => !x.IsArchive)
				.Left.JoinAlias(() => carModelAlias.ManufacturerCars, () => manufacturerCarsAlias);
			var result = query.SelectList(list => list
					.Select(c => c.Name).WithAlias(() => resultNode.Name)
					.Select(c => manufacturerCarsAlias.Name).WithAlias(() => resultNode.ManufacturedCars)
					.Select(c => c.TypeOfUse.GetEnumTitle()).WithAlias(() => resultNode.Type)
				).TransformUsing(Transformers.AliasToBean<CarModelJournalNode>());
			return result;
		};

		protected override Func<ModelCarViewModel> CreateDialogFunction => () => new ModelCarViewModel(
			EntityUoWBuilder.ForCreate(),
			_unitOfWorkFactory,
			commonServices);

		protected override Func<CarModelJournalNode, ModelCarViewModel> OpenDialogFunction => node => new ModelCarViewModel(
			EntityUoWBuilder.ForOpen(node.Id),
			_unitOfWorkFactory,
			commonServices);
	}
}
