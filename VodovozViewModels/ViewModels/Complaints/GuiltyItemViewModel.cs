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
			UpdateAcessibility();
		}

		public IList<Subdivision> AllDepartments => subdivisionRepository.GetAllDepartments(UoW);

		bool canChooseEmployee;
		public virtual bool CanChooseEmployee {
			get => canChooseEmployee;
			set => SetField(ref canChooseEmployee, value, () => CanChooseEmployee);
		}

		bool canChooseSubdivision;
		public virtual bool CanChooseSubdivision {
			get => canChooseSubdivision;
			set => SetField(ref canChooseSubdivision, value, () => CanChooseSubdivision);
		}

		bool isGuiltyCorrect;
		public virtual bool IsGuiltyCorrect {
			get => isGuiltyCorrect;
			set => SetField(ref isGuiltyCorrect, value, () => IsGuiltyCorrect);
		}

		void UpdateAcessibility()
		{
			CanChooseEmployee = Entity.GuiltyType == ComplaintGuiltyTypes.Employee;
			CanChooseSubdivision = Entity.GuiltyType == ComplaintGuiltyTypes.Subdivision;
			IsGuiltyCorrect = Entity.GuiltyType != null
								&& (Entity.GuiltyType == ComplaintGuiltyTypes.Employee && Entity.Employee != null
									|| Entity.GuiltyType == ComplaintGuiltyTypes.Subdivision && Entity.Subdivision != null
									|| Entity.GuiltyType == ComplaintGuiltyTypes.Client && Entity.Employee == null && Entity.Subdivision == null
									|| Entity.GuiltyType == ComplaintGuiltyTypes.None && Entity.Employee == null && Entity.Subdivision == null);
		}

		void ConfigureEntityPropertyChanges()
		{
			OnEntityPropertyChanged(
				UpdateAcessibility,
				e => e.Employee,
				e => e.Subdivision
			);

			OnEntityPropertyChanged(
				() => {
					Entity.Employee = null;
					Entity.Subdivision = null;
					UpdateAcessibility();
				},
				e => e.GuiltyType
			);
		}
	}
}