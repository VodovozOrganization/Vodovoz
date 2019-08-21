using System;
using QS.Commands;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Suppliers;
using Vodovoz.EntityRepositories.Suppliers;

namespace Vodovoz.ViewModels.Suppliers
{
	public class RequestToSupplierViewModel : EntityTabViewModelBase<RequestToSupplier>
	{
		readonly ISupplierPriceItemsRepository supplierPriceItemsRepository;

		public RequestToSupplierViewModel(IEntityConstructorParam ctorParam, ICommonServices commonServices, ISupplierPriceItemsRepository supplierPriceItemsRepository) : base(ctorParam, commonServices)
		{
			this.supplierPriceItemsRepository = supplierPriceItemsRepository ?? throw new ArgumentNullException(nameof(supplierPriceItemsRepository));
			CreateCommands();
			RefreshSuppliers();
			Entity.ObservableRequestingNomenclatureItems.ElementAdded += (aList, aIdx) => RefreshSuppliers();
			Entity.ObservableRequestingNomenclatureItems.ElementRemoved += (aList, aIdx, aObject) => RefreshSuppliers();
		}
		public event EventHandler ListContentChanged;


		bool canEdit = true;
		public bool CanEdit {
			get => canEdit;
			set => SetField(ref canEdit, value);
		}

		bool canRemove;
		public bool CanRemove {
			get => canRemove;
			set => SetField(ref canRemove, value);
		}

		#region Commands

		void CreateCommands()
		{
			CreateRefreshCommand();
			CreateAddRequestingNomenclatureCommand();
			CreateRemoveRequestingNomenclatureCommand();
			CreateTransferRequestingNomenclatureCommand();
		}

		void RefreshSuppliers()
		{
			Entity.RequestingNomenclaturesListRefresh(UoW, supplierPriceItemsRepository, Entity.SuppliersOrdering);
			ListContentChanged?.Invoke(this, new EventArgs());
		}

		#region RefreshCommand

		public DelegateCommand RefreshCommand { get; private set; }

		void CreateRefreshCommand()
		{
			RefreshCommand = new DelegateCommand(
				RefreshSuppliers,
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
