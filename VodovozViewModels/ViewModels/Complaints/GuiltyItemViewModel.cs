using System;
using System.Collections.Generic;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Complaints;
using Vodovoz.EntityRepositories.Subdivisions;

namespace Vodovoz.ViewModels.Complaints
{
	public class GuiltyItemViewModel : EntityWidgetViewModelBase<ComplaintGuiltyItem>
	{
		readonly ISubdivisionRepository subdivisionRepository;

		public GuiltyItemViewModel(ComplaintGuiltyItem entity, ICommonServices commonServices, ISubdivisionRepository subdivisionRepository) : base(entity, commonServices)
		{
			this.subdivisionRepository = subdivisionRepository ?? throw new ArgumentNullException(nameof(subdivisionRepository));
			//UpdateAcessibility();
			ConfigureEntityPropertyChanges();
		}

		public IList<Subdivision> AllDepartments => subdivisionRepository.GetAllDepartments(UoW);

		public bool CanChooseEmployee => Entity.GuiltyType == ComplaintGuiltyTypes.Employee;

		public bool CanChooseSubdivision => Entity.GuiltyType == ComplaintGuiltyTypes.Subdivision;

		public bool IsGuiltyCorrect => Entity.GuiltyType != null
											&& (Entity.GuiltyType == ComplaintGuiltyTypes.Employee && Entity.Employee != null
												|| Entity.GuiltyType == ComplaintGuiltyTypes.Subdivision && Entity.Subdivision != null
												|| Entity.GuiltyType == ComplaintGuiltyTypes.Client && Entity.Employee == null && Entity.Subdivision == null
												|| Entity.GuiltyType == ComplaintGuiltyTypes.None && Entity.Employee == null && Entity.Subdivision == null);

		void ConfigureEntityPropertyChanges()
		{
			SetPropertyChangeRelation(
				e => e.GuiltyType,
				() => CanChooseEmployee,
				() => CanChooseSubdivision
			);

			SetPropertyChangeRelation(
				e => e.GuiltyType,
				() => IsGuiltyCorrect
			);

			SetPropertyChangeRelation(
				e => e.Employee,
				() => IsGuiltyCorrect
			);

			SetPropertyChangeRelation(
				e => e.Subdivision,
				() => IsGuiltyCorrect
			);
		}
	}
}