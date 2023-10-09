using QS.DomainModel.UoW;
using QS.Project.Journal.EntitySelector;
using QS.Services;
using QS.ViewModels;
using QS.ViewModels.Control.EEVM;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Complaints;
using Vodovoz.EntityRepositories.Subdivisions;
using Vodovoz.Parameters;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.JournalFactories;

namespace Vodovoz.ViewModels.Complaints
{
	public class GuiltyItemViewModel : EntityWidgetViewModelBase<ComplaintGuiltyItem>
	{
		private readonly IEmployeeJournalFactory _employeeJournalFactory;
		private readonly ISubdivisionParametersProvider _subdivisionParametersProvider;
		private bool _isForSalesDepartment;

		public GuiltyItemViewModel(
			ComplaintGuiltyItem entity,
			ICommonServices commonServices,
			ISubdivisionRepository subdivisionRepository,
			IEmployeeJournalFactory employeeJournalFactory,
			ISubdivisionParametersProvider subdivisionParametersProvider,
			IUnitOfWork uow,
			bool fromComplaintsJournalFilter = false
		) : base(entity, commonServices)
		{
			UoW = uow ?? throw new ArgumentNullException(nameof(uow));

			_employeeJournalFactory = employeeJournalFactory ?? throw new ArgumentNullException(nameof(employeeJournalFactory));
			EmployeeSelectorFactory = _employeeJournalFactory.CreateEmployeeAutocompleteSelectorFactory();

			if(subdivisionRepository == null) {
				throw new ArgumentNullException(nameof(subdivisionRepository));
			}

			_subdivisionParametersProvider = subdivisionParametersProvider ?? throw new ArgumentNullException(nameof(subdivisionParametersProvider));
			ConfigureEntityPropertyChanges();
			HideClientFromGuilty = !fromComplaintsJournalFilter;
			ResponsibleList = uow.GetAll<Responsible>().Where(r => !r.IsArchived).ToList();
		}

		public event EventHandler OnGuiltyItemReady;

		public bool CanChooseEmployee => Entity.Responsible != null && Entity.Responsible.IsEmployeeResponsible;

		public bool CanChooseSubdivision => Entity.Responsible != null && Entity.Responsible.IsSubdivisionResponsible;
		public bool HideClientFromGuilty { get; }
		public IList<Responsible> ResponsibleList { get; }

		public bool IsForSalesDepartment
		{
			get => _isForSalesDepartment;
			set
			{
				_isForSalesDepartment = value;

				if(value)
				{
					Entity.Responsible = ResponsibleList.FirstOrDefault(r => r.IsSubdivisionResponsible);
					var salesSubDivisionId = _subdivisionParametersProvider.GetSalesSubdivisionId();
					Entity.Subdivision = UoW.GetById<Subdivision>(salesSubDivisionId);
				}
			}
		}

		public IEntityAutocompleteSelectorFactory EmployeeSelectorFactory { get; }

		public IEntityEntryViewModel SubdivisionViewModel { get; private set; }

		void ConfigureEntityPropertyChanges()
		{
			SetPropertyChangeRelation(
				e => e.Responsible,
				() => CanChooseEmployee,
				() => CanChooseSubdivision
			);

			OnEntityPropertyChanged(
				() => OnGuiltyItemReady?.Invoke(this, EventArgs.Empty),
				e => e.Responsible,
				e => e.Employee,
				e => e.Subdivision
			);
		}
	}
}
