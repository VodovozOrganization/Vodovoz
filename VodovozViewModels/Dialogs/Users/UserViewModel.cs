using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Employees;
using QS.Commands;

namespace Vodovoz.ViewModels
{
    public class UserViewModel : EntityTabViewModelBase<User>
    {
        public UserViewModel(IEntityUoWBuilder uowBuilder, IUnitOfWorkFactory unitOfWorkFactory, ICommonServices commonServices, INavigationManager navigation = null) 
            : base(uowBuilder, unitOfWorkFactory, commonServices, navigation)
        {
		}

		private DelegateCommand _saveCommand;
		public DelegateCommand SaveCommand
		{
			get
			{
				if(_saveCommand == null)
				{
					_saveCommand = new DelegateCommand(() => {
						UoW.Save();
					});
				}
				return _saveCommand;
			}
		}

		private DelegateCommand _cancelCommand;
		public DelegateCommand CancelCommand
		{
			get
			{
				if(_cancelCommand == null)
				{
					_cancelCommand = new DelegateCommand(() => {
						Close(true, CloseSource.Cancel);
					});
				}
				return _cancelCommand;
			}
		}
	}
}
