using System;
using QS.Commands;
using QS.DomainModel.UoW;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Client;

namespace Vodovoz.ViewModels.Client
{
	public class SupplierPricesWidgetViewModel : EntityWidgetViewModelBase<Counterparty>
	{
		public SupplierPricesWidgetViewModel(Counterparty entity, IUnitOfWork uow, ICommonServices commonServices) : base(entity, commonServices)
		{
			UoW = uow ?? throw new ArgumentNullException(nameof(uow));
			CreateCommands();
			//UpdateAcessibility();

		}

		void CreateCommands()
		{
			CreateAddItemCommand();
			CreateRemoveItemCommand();
			CreateEditItemCommand();
		}

		public bool CanAdd { get; set; }
		public bool CanEdit { get; set; } = false;//задача редактирования пока не актуальна
		public bool CanRemove { get; set; }

		#region Commands

		#region AddItemCommand

		public DelegateCommand AddItemCommand { get; private set; }

		private void CreateAddItemCommand()
		{
			AddItemCommand = new DelegateCommand(
				() => {

				},
				() => true
			);
		}

		#endregion AddItemCommand

		#region RemoveItemCommand

		public DelegateCommand RemoveItemCommand { get; private set; }

		private void CreateRemoveItemCommand()
		{
			RemoveItemCommand = new DelegateCommand(
				() => { },
				() => true
			);
		}

		#endregion RemoveItemCommand

		#region EditItemCommand

		public DelegateCommand EditItemCommand { get; private set; }

		private void CreateEditItemCommand()
		{
			EditItemCommand = new DelegateCommand(
				() => { },
				() => true
			);
		}

		#endregion EditItemCommand

		#endregion Commands

	}
}
