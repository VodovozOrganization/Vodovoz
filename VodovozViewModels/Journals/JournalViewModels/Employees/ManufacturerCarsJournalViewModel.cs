using System;
using NHibernate;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Services;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.ViewModels.Journals.JournalNodes;
using Vodovoz.ViewModels.Journals.JournalNodes.Logistic;
using Vodovoz.ViewModels.ViewModels.Logistic;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Employees
{
	public class ManufacturerCarsJournalViewModel: SingleEntityJournalViewModelBase<ManufacturerCars, ManufacturerCarsViewModel, ManufacturerCarsJournalNode>
	{
		private readonly ICommonServices _commonServices;

		public ManufacturerCarsJournalViewModel(IUnitOfWorkFactory unitOfWorkFactory, ICommonServices commonServices, bool hideJournalForOpenDialog = false, bool hideJournalForCreateDialog = false) : base(unitOfWorkFactory, commonServices, hideJournalForOpenDialog, hideJournalForCreateDialog)
		{
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));

			TabName = "Журнал производителей автомобилей";
		}

		protected override Func<IUnitOfWork, IQueryOver<ManufacturerCars>> ItemsSourceQueryFunction => uow => {
			ManufacturerCarsJournalNode resultAlias = null;
			ManufacturerCars manufacturerCarsAlias = null;

			var query = uow.Session.QueryOver(() => manufacturerCarsAlias);

			query.Where(GetSearchCriterion(
				() => manufacturerCarsAlias.Id,
				() => manufacturerCarsAlias.Name));

			return query
				.SelectList(list => list
					.Select(x => manufacturerCarsAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(x => manufacturerCarsAlias.Name).WithAlias(() => resultAlias.ManufacturerName))
				.TransformUsing(Transformers.AliasToBean<ManufacturerCarsJournalNode>());
		};
		protected override Func<ManufacturerCarsViewModel> CreateDialogFunction => () => new ManufacturerCarsViewModel(
			EntityUoWBuilder.ForCreate(),
			QS.DomainModel.UoW.UnitOfWorkFactory.GetDefaultFactory,
			_commonServices);
		protected override Func<ManufacturerCarsJournalNode, ManufacturerCarsViewModel> OpenDialogFunction => (node) => new ManufacturerCarsViewModel(
			EntityUoWBuilder.ForOpen(node.Id),
			QS.DomainModel.UoW.UnitOfWorkFactory.GetDefaultFactory,
			_commonServices
		);
		
		protected override void CreateNodeActions()
		{
			NodeActionsList.Clear();
			CreateDefaultSelectAction();
			CreateDefaultAddActions();
			CreateDefaultEditAction();
		}
	}
}
