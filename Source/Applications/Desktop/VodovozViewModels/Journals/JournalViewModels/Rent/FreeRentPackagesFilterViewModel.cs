using QS.DomainModel.Entity;
using QS.Project.Filter;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Rent
{
	public class FreeRentPackagesFilterViewModel
		: FilterViewModelBase<FreeRentPackagesFilterViewModel>
	{
		private const bool _showArchievedDefault = false;

		private bool _showArchieved;
		private bool? _restrictArchieved;

		public bool ShowArchieved
		{
			get => _showArchieved;
			set
			{
				if(!CanChangeArchieved)
				{
					return;
				}

				UpdateFilterField(ref _showArchieved, value);
			}
		}

		/// <summary>
		/// Установка не изменяемого значения для ShowArchieved
		/// Или null - для разблокировки
		/// </summary>
		[PropertyChangedAlso(nameof(CanChangeArchieved))]		
		public bool? RestrictArchieved
		{
			get => _restrictArchieved;
			set
			{
				if(UpdateFilterField(ref _restrictArchieved, value))
				{
					if(value.HasValue)
					{
						ShowArchieved = value.Value;
					}
					else
					{
						ShowArchieved = _showArchievedDefault;
					}
				}
			}
		}

		public bool CanChangeArchieved => RestrictArchieved == null;
	}
}
