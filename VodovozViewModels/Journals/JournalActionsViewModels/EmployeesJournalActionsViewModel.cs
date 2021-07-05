using System;
using System.Linq;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Employees;
using Vodovoz.ViewModels.Journals.JournalNodes.Employees;

namespace Vodovoz.Journals.JournalActionsViewModels
{
	public class EmployeesJournalActionsViewModel : EntitiesJournalActionsViewModel
	{
		private Action<Employee> _resetPasswordForEmployeeAction;
		private DelegateCommand _resetPasswordCommand;
		
		public EmployeesJournalActionsViewModel(
			IInteractiveService interactiveService,
			IUnitOfWorkFactory unitOfWorkFactory) : base(interactiveService)
		{
			if(unitOfWorkFactory == null)
			{
				throw new ArgumentNullException(nameof(unitOfWorkFactory));
			}
			
			UoW = unitOfWorkFactory.CreateWithoutRoot();
		}

		public override object[] SelectedItems 
		{ 
			get => selectedItems;
			set
			{
				if(SetField(ref selectedItems, value))
				{
					OnPropertyChanged(nameof(CanSelect));
					OnPropertyChanged(nameof(CanAdd));
					OnPropertyChanged(nameof(CanEdit));
					OnPropertyChanged(nameof(CanDelete));
					OnPropertyChanged(nameof(CanResetPassword));
				}
			}
		}

		public bool CanResetPassword => SelectedItems.FirstOrDefault() != null;

		public DelegateCommand ResetPasswordCommand => _resetPasswordCommand ?? (_resetPasswordCommand = new DelegateCommand(
				() =>
				{
					var selectedNodes = selectedItems.OfType<EmployeeJournalNode>();
					var selectedNode = selectedNodes.FirstOrDefault();
					
					if(selectedNode != null)
					{
						var employee = UoW.GetById<Employee>(selectedNode.Id);

						if(employee.User == null)
						{
							interactiveService.ShowMessage(ImportanceLevel.Error, "К сотруднику не привязан пользователь!");

							return;
						}

						if(string.IsNullOrEmpty(employee.User.Login))
						{
							interactiveService.ShowMessage(ImportanceLevel.Error, "У пользователя не заполнен логин!");

							return;
						}

						if(interactiveService.Question("Вы уверены?"))
						{
							_resetPasswordForEmployeeAction?.Invoke(employee);
						}
					}
				},
				() => CanResetPassword
			)
		);

		public void SetResetPasswordForEmployeeAction(Action<Employee> resetPasswordForEmployeeAction)
		{
			_resetPasswordForEmployeeAction = resetPasswordForEmployeeAction;
		}
	}
}