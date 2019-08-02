using System;
using System.Collections.Generic;
using System.Linq;
using QS.Commands;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Complaints;
using Vodovoz.EntityRepositories.Subdivisions;

namespace Vodovoz.ViewModels.Complaints
{
	public class GuiltyItemsViewModel : EntityWidgetViewModelBase<Complaint>
	{
		readonly ISubdivisionRepository subdivisionRepository;

		public GuiltyItemsViewModel(Complaint entity, ICommonServices commonServices, ISubdivisionRepository subdivisionRepository) : base(entity, commonServices)
		{
			this.subdivisionRepository = subdivisionRepository ?? throw new ArgumentNullException(nameof(subdivisionRepository));
		}

		public IList<Subdivision> AllDepartments => subdivisionRepository.GetAllDepartments(UoW);

		public bool CanAddSubdivision(ComplaintGuiltyItem guilty) => guilty.GuiltyType == ComplaintGuiltyTypes.Subdivision && AllDepartments.Any();

		public bool CanRemoveGuilty(ComplaintGuiltyItem guilty) => guilty != null;

		#region Commands

		private void CreateCommands()
		{
			CreateAddGuiltyCommand();
			CreateRemoveGuiltyCommand();
		}

		#region AddGuiltyCommand

		public DelegateCommand<ComplaintGuiltyTypes> AddGuiltyCommand { get; private set; }
		private void CreateAddGuiltyCommand()
		{
			AddGuiltyCommand = new DelegateCommand<ComplaintGuiltyTypes>(
				type => {
					var guilty = new ComplaintGuiltyItem {
						GuiltyType = type,
						Complaint = Entity,
					};
					Entity.ObservableGuilties.Add(guilty);
				},
				type => true
			);
		}

		#endregion AddGuiltyCommand

		#region RemoveGuiltyCommand

		public DelegateCommand<ComplaintGuiltyItem> RemoveGuiltyCommand { get; private set; }
		private void CreateRemoveGuiltyCommand()
		{
			RemoveGuiltyCommand = new DelegateCommand<ComplaintGuiltyItem>(
				g => Entity.ObservableGuilties.Remove(g),
				CanRemoveGuilty
			);
		}

		#endregion RemoveGuiltyCommand

		#endregion Commands
	}
}
