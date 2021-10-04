using System;
using NHibernate;
using QS.DomainModel.UoW;
using QS.Project.Journal;
using QS.Services;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.ViewModels.Journals.FilterViewModels.Logistic;
using Vodovoz.ViewModels.Journals.JournalNodes.Logistic;
using Vodovoz.ViewModels.ViewModels.Logistic;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Logistic
{
	public class CarVersionJournalViewModel: SingleEntityJournalViewModelBase<CarVersion, CarVersionViewModel, CarVersionJournalNode>
	{
		private readonly ICommonServices _commonServices;

		public CarVersionJournalViewModel(
			CarVersionFilterViewModel filter,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices)
			: base(unitOfWorkFactory, commonServices)
		{
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));

			TabName = "Журнал версий автомобилей";
		}

		protected override Func<IUnitOfWork, IQueryOver<CarVersion>> ItemsSourceQueryFunction { get; }
		protected override Func<CarVersionViewModel> CreateDialogFunction { get; }
		protected override Func<CarVersionJournalNode, CarVersionViewModel> OpenDialogFunction { get; }
	}
}
