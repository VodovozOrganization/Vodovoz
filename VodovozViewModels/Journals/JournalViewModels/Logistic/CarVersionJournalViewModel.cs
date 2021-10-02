using System;
using NHibernate;
using QS.DomainModel.UoW;
using QS.Project.Journal;
using QS.Services;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.ViewModels.Journals.JournalNodes;
using Vodovoz.ViewModels.Journals.JournalNodes.Logistic;
using Vodovoz.ViewModels.ViewModels.Employees;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Logistic
{
	public class CarVersionJournalViewModel: SingleEntityJournalViewModelBase<CarVersion, EmployeePostViewModel, CarVersionJournalNode>
	{
		private readonly ICommonServices _commonServices;

		public CarVersionJournalViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices)
			: base(unitOfWorkFactory, commonServices)
		{
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));

			TabName = "Журнал версий автомобилей";
		}

		protected override Func<IUnitOfWork, IQueryOver<CarVersion>> ItemsSourceQueryFunction { get; }
		protected override Func<EmployeePostViewModel> CreateDialogFunction { get; }
		protected override Func<CarVersionJournalNode, EmployeePostViewModel> OpenDialogFunction { get; }
	}
}
