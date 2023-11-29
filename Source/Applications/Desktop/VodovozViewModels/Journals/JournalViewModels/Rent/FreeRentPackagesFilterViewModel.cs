using QS.Project.Filter;
using System;
using System.Linq.Expressions;
using Vodovoz.Domain;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Rent
{
	public class FreeRentPackagesFilterViewModel
		: FilterViewModelBase<FreeRentPackagesFilterViewModel>
	{
		private bool _showArchieved;

		public bool ShowArchieved
		{
			get => _showArchieved;
			set => UpdateFilterField(ref _showArchieved, value);
		}

		public Expression<Func<FreeRentPackage, bool>> Specification => (freeRentPackage) => ShowArchieved ? true : !freeRentPackage.IsArchieve;
	}
}
