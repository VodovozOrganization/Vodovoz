using System;
using System.Collections.Generic;
using Gamma.ColumnConfig;
using NHibernate.Criterion;
using NHibernate.Transform;
using QSOrmProject;
using QSOrmProject.RepresentationModel;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Goods;

namespace Vodovoz.ViewModel
{
	public class ClientEquipmentBalanceVM : RepresentationModelWithoutEntityBase<ClientEquipmentBalanceVMNode>
	{
		public ClientBalanceFilter Filter {
			get {
				return RepresentationFilter as ClientBalanceFilter;
			}
			set {
				RepresentationFilter = value as IRepresentationFilter;
			}
		}

		#region IRepresentationModel implementation

		public override void UpdateNodes ()
		{
			Nomenclature nomenclatureAlias = null;
			ClientEquipmentBalanceVMNode resultAlias = null;
			Counterparty counterpartyAlias = null;
			DeliveryPoint deliveryPointAlias = null;
			Equipment equipmentAlias = null;
			CounterpartyMovementOperation operationAlias = null;
			CounterpartyMovementOperation subsequentOperationAlias = null;

			var lastCouterpartyOp = UoW.Session.QueryOver<CounterpartyMovementOperation>(() => operationAlias)
				.JoinAlias(o => o.Equipment, () => equipmentAlias)
				.Where(o => o.Equipment != null);

			if (Filter.RestrictIncludeSold == false)
				lastCouterpartyOp.Where(x => x.ForRent == true);

			if(Filter.RestrictDeliveryPoint == null)
			{
				if (Filter.RestrictCounterparty != null)
					lastCouterpartyOp.Where(x => x.IncomingCounterparty == Filter.RestrictCounterparty);
				else
					lastCouterpartyOp.Where(x => x.IncomingCounterparty != null);
			}
			else
			{
				lastCouterpartyOp.Where(x => x.IncomingDeliveryPoint == Filter.RestrictDeliveryPoint);
			}

			if(Filter.RestrictNomenclature != null)
			{
				lastCouterpartyOp.Where(() => equipmentAlias.Nomenclature.Id == Filter.RestrictNomenclature.Id);
			}

			var subsequentOperationsSubquery = QueryOver.Of<CounterpartyMovementOperation> (() => subsequentOperationAlias)
				.Where (() => operationAlias.OperationTime < subsequentOperationAlias.OperationTime && operationAlias.Equipment == subsequentOperationAlias.Equipment)
				.Select (op=>op.Id);

			lastCouterpartyOp.WithSubquery.WhereNotExists(subsequentOperationsSubquery);
			
			var resultList = lastCouterpartyOp
				.JoinAlias(o => o.IncomingCounterparty, () => counterpartyAlias)
				.JoinAlias(o => o.IncomingDeliveryPoint, () => deliveryPointAlias)
				.JoinAlias(() => equipmentAlias.Nomenclature, () => nomenclatureAlias)
				.SelectList (list => list
					.Select (() => equipmentAlias.Id).WithAlias (() => resultAlias.Id)
					.Select (() => nomenclatureAlias.Name).WithAlias (() => resultAlias.NomenclatureName)
					.Select (() => counterpartyAlias.FullName).WithAlias (() => resultAlias.Client)
					.Select (() => deliveryPointAlias.CompiledAddress).WithAlias (() => resultAlias.Address)
					.Select (() => operationAlias.ForRent).WithAlias (() => resultAlias.IsOur)
					.Select (() => equipmentAlias.Id).WithAlias (() => resultAlias.SerialNumberInt)
			                )
				.TransformUsing (Transformers.AliasToBean<ClientEquipmentBalanceVMNode> ())
				.List<ClientEquipmentBalanceVMNode> ();

			SetItemsSource (resultList);
		}

		IColumnsConfig columnsConfig = FluentColumnsConfig<ClientEquipmentBalanceVMNode>.Create ()
			.AddColumn ("Номенклатура").SetDataProperty (node => node.NomenclatureName)
			.AddColumn("Серийный номер").AddTextRenderer(node => node.SerialNumber)
			.AddColumn ("Наше").AddToggleRenderer(node => node.IsOur)
			.AddColumn("Клиент").AddTextRenderer(node => node.Client)
			.AddColumn("Адрес").AddTextRenderer(node => node.Address)
			.Finish ();

		public override IColumnsConfig ColumnsConfig {
			get { return columnsConfig; }
		}

		#endregion

		#region implemented abstract members of RepresentationModelBase

		protected override bool NeedUpdateFunc (object updatedSubject)
		{
			//FIXME Пока простая проверка.
			return true; //(updatedSubject is Nomenclature || updatedSubject is GoodsMovementOperation);
		}

		#endregion

		public ClientEquipmentBalanceVM (ClientBalanceFilter filter) : this (filter.UoW)
		{
			Filter = filter;
		}

		public ClientEquipmentBalanceVM ()
			: this (UnitOfWorkFactory.CreateWithoutRoot ())
		{
			CreateRepresentationFilter = () => new ClientBalanceFilter (UoW);
		}

		public ClientEquipmentBalanceVM (IUnitOfWork uow) : base (typeof(CounterpartyMovementOperation))
		{
			this.UoW = uow;
		}
	}

	public class ClientEquipmentBalanceVMNode
	{

		public int Id{ get; set; }

		[UseForSearch]
		public string SerialNumber
		{
			get
			{
				return SerialNumberInt.ToString();
			}
		}

		public int SerialNumberInt { get; set; }

		public bool IsOur { get; set; }

		public string Client { get; set; }

		[UseForSearch]
		public string Address { get; set; }

		[UseForSearch]
		public string NomenclatureName { get; set; }
	}
}

