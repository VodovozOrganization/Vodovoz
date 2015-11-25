using System;
using System.Collections.Generic;
using Gtk;
using NHibernate.Transform;
using QSOrmProject;
using QSOrmProject.RepresentationModel;
using Vodovoz.Domain;
using Gamma.ColumnConfig;

namespace Vodovoz.ViewModel
{
	public class DeliveryPointsVM : RepresentationModelEntityBase<DeliveryPoint, DeliveryPointVMNode>, IRepresentationModelWithParent
	{
		public IUnitOfWorkGeneric<Counterparty> CounterpartyUoW {
			get {
				return UoW as IUnitOfWorkGeneric<Counterparty>;
			}
		}

		Counterparty counterparty;

		public Counterparty Counterparty {
			get {
				if (CounterpartyUoW != null)
					return CounterpartyUoW.Root;
				else
					return counterparty;
			}
			private set {
				counterparty = value;
			}
		}

		#region IRepresentationModelWithParent implementation

		public object GetParent {
			get {
				return Counterparty;
			}
		}

		#endregion

		#region IRepresentationModel implementation

		public override void UpdateNodes ()
		{
			DeliveryPoint deliveryPointAlias = null;
			Counterparty counterpartyAlias = null;
			DeliveryPointVMNode resultAlias = null;

			var deliveryPointslist = UoW.Session.QueryOver<DeliveryPoint> (() => deliveryPointAlias)
				.JoinAlias (c => c.Counterparty, () => counterpartyAlias)
				.Where (() => counterpartyAlias.Id == Counterparty.Id)
				.SelectList (list => list
					.Select (() => deliveryPointAlias.Id).WithAlias (() => resultAlias.Id)
					.Select (() => deliveryPointAlias.CompiledAddress).WithAlias (() => resultAlias.CompiledAddress)
					.Select (() => deliveryPointAlias.IsActive).WithAlias (() => resultAlias.IsActive)
			                         )
				.TransformUsing (Transformers.AliasToBean<DeliveryPointVMNode> ())
				.List<DeliveryPointVMNode> ();

			SetItemsSource (deliveryPointslist);
		}

		IColumnsConfig columnsConfig = FluentColumnsConfig<DeliveryPointVMNode>.Create ()
			.AddColumn ("Название").SetDataProperty (node => node.CompiledAddress)
			.RowCells ().AddSetter<CellRendererText> ((c, n) => c.Foreground = n.RowColor)
			.Finish ();

		public override IColumnsConfig ColumnsConfig {
			get { return columnsConfig; }
		}

		#endregion

		#region implemented abstract members of RepresentationModelBase

		protected override bool NeedUpdateFunc (DeliveryPoint updatedSubject)
		{
			return Counterparty.Id == updatedSubject.Counterparty.Id;
		}

		#endregion

		public DeliveryPointsVM (IUnitOfWorkGeneric<Counterparty> uow)
		{
			this.UoW = uow;
		}

		public DeliveryPointsVM (IUnitOfWork uow, Counterparty counterparty)
		{
			this.UoW = uow;
			Counterparty = counterparty;
		}
	}

	public class DeliveryPointVMNode
	{

		public int Id { get; set; }

		public string CompiledAddress { get; set; }

		public bool IsActive { get; set; }

		public string RowColor { get { return IsActive ? "black" : "grey"; } }
	}
}

