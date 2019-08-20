using QS.Commands;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Suppliers;

namespace Vodovoz.ViewModels.Suppliers
{
	public class RequestToSupplierViewModel : EntityTabViewModelBase<RequestToSupplier>
	{
		public RequestToSupplierViewModel(IEntityConstructorParam ctorParam, ICommonServices commonServices) : base(ctorParam, commonServices)
		{
			CreateCommands();
		}

		public bool CanEdit = false;

		#region Commands

		void CreateCommands()
		{
			CreateRefreshCommand();
			CreateAddRequestingNomenclatureCommand();
			CreateRemoveRequestingNomenclatureCommand();
			CreateTransferRequestingNomenclatureCommand();
		}

		#region RefreshCommand

		public DelegateCommand RefreshCommand { get; private set; }

		void CreateRefreshCommand()
		{
			RefreshCommand = new DelegateCommand(
				() => { },
				() => true
			);
		}

		#endregion RefreshCommand

		#region AddRequestingNomenclatureCommand

		public DelegateCommand AddRequestingNomenclatureCommand { get; private set; }

		void CreateAddRequestingNomenclatureCommand()
		{
			AddRequestingNomenclatureCommand = new DelegateCommand(
				() => { },
				() => true
			);
		}

		#endregion AddRequestingNomenclatureCommand

		#region RemoveRequestingNomenclatureCommand

		public DelegateCommand RemoveRequestingNomenclatureCommand { get; private set; }

		void CreateRemoveRequestingNomenclatureCommand()
		{
			RemoveRequestingNomenclatureCommand = new DelegateCommand(
				() => { },
				() => true
			);
		}

		#endregion RemoveRequestingNomenclatureCommand

		#region TransferRequestingNomenclatureCommand

		public DelegateCommand TransferRequestingNomenclatureCommand { get; private set; }

		void CreateTransferRequestingNomenclatureCommand()
		{
			TransferRequestingNomenclatureCommand = new DelegateCommand(
				() => { },
				() => true
			);
		}

		#endregion TransferRequestingNomenclatureCommand

		#endregion Commands
	}
}
