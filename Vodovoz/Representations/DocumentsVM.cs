using System;
using QSOrmProject.RepresentationModel;
using Vodovoz.Domain.Documents;
using QSOrmProject;
using Gtk.DataBindings;
using NHibernate.Transform;
using NHibernate.Criterion;
using Vodovoz.Domain;
using NHibernate;
using System.Collections.Generic;

namespace Vodovoz.ViewModel
{
	public class DocumentsVM : RepresentationModelBase<Document, DocumentVMNode>
	{
		#region IRepresentationModel implementation

		public override void UpdateNodes ()
		{
			IncomingInvoice invoiceAlias = null;
			IncomingWater waterAlias = null;
			MovementDocument movementAlias = null;
			WriteoffDocument writeoffAlias = null;
			DocumentVMNode resultAlias = null;
			Counterparty counterpartyAlias = null;
			Counterparty secondCounterpartyAlias = null;
			Warehouse warehouseAlias = null;
			Warehouse secondWarehouseAlias = null;

			var invoiceList = UoW.Session.QueryOver<IncomingInvoice> (() => invoiceAlias)
				.JoinQueryOver(() => invoiceAlias.Contractor, () => counterpartyAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.JoinQueryOver(() => invoiceAlias.Warehouse, () => warehouseAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.SelectList (list => list
					.Select (() => invoiceAlias.Id).WithAlias (() => resultAlias.Id)
					.Select (() => invoiceAlias.TimeStamp).WithAlias (() => resultAlias.Date)
					.Select (() => "Входящая накладная").WithAlias (() => resultAlias.DocType)
					.Select(Projections.Conditional(
						Restrictions.Where(() => counterpartyAlias.Name == null),
						Projections.Constant("Не указан", NHibernateUtil.String),
						Projections.Property(() => counterpartyAlias.Name)))
					.WithAlias (() => resultAlias.Counterparty)
					.Select(Projections.Conditional(
						Restrictions.Where(() => warehouseAlias.Name == null),
						Projections.Constant("Не указан", NHibernateUtil.String),
						Projections.Property(() => warehouseAlias.Name)))
					.WithAlias (() => resultAlias.Warehouse))
				.TransformUsing (Transformers.AliasToBean<DocumentVMNode> ())
				.List<DocumentVMNode> ();
		
			var waterList = UoW.Session.QueryOver<IncomingWater> (() => waterAlias)
				.JoinQueryOver(() => waterAlias.WriteOffWarehouse, () => warehouseAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.SelectList (list => list
					.Select (() => waterAlias.Id).WithAlias (() => resultAlias.Id)
					.Select (() => waterAlias.TimeStamp).WithAlias (() => resultAlias.Date)
					.Select (() => "Документ производства").WithAlias (() => resultAlias.DocType)
					.Select(Projections.Conditional(
						Restrictions.Where(() => warehouseAlias.Name == null),
						Projections.Constant("Не указан", NHibernateUtil.String),
						Projections.Property(() => warehouseAlias.Name)))
					.WithAlias (() => resultAlias.Warehouse)
					.Select (() => waterAlias.Amount).WithAlias (() => resultAlias.Amount))
				.TransformUsing (Transformers.AliasToBean<DocumentVMNode> ())
				.List<DocumentVMNode> ();

			var movementList = UoW.Session.QueryOver<MovementDocument> (() => movementAlias)
				.JoinQueryOver(() => movementAlias.FromClient, () => counterpartyAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.JoinQueryOver(() => movementAlias.FromWarehouse, () => warehouseAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.JoinQueryOver(() => movementAlias.ToClient, () => secondCounterpartyAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.JoinQueryOver(() => movementAlias.ToWarehouse, () => secondWarehouseAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.SelectList (list => list
					.Select (() => movementAlias.Id).WithAlias (() => resultAlias.Id)
					.Select (() => movementAlias.TimeStamp).WithAlias (() => resultAlias.Date)
					.Select (() => "Документ перемещения").WithAlias (() => resultAlias.DocType)
					.Select (() => movementAlias.Category).WithAlias (() => resultAlias.MDCategory)
					.Select(Projections.Conditional(
						Restrictions.Where(() => counterpartyAlias.Name == null),
						Projections.Constant("Не указан", NHibernateUtil.String),
						Projections.Property(() => counterpartyAlias.Name)))
					.WithAlias (() => resultAlias.Counterparty)
					.Select(Projections.Conditional(
						Restrictions.Where(() => warehouseAlias.Name == null),
						Projections.Constant("Не указан", NHibernateUtil.String),
						Projections.Property(() => warehouseAlias.Name)))
					.WithAlias (() => resultAlias.Warehouse)
					.Select(Projections.Conditional(
						Restrictions.Where(() => secondCounterpartyAlias.Name == null),
						Projections.Constant("Не указан", NHibernateUtil.String),
						Projections.Property(() => secondCounterpartyAlias.Name)))
					.WithAlias (() => resultAlias.SecondCounterparty)
					.Select(Projections.Conditional(
						Restrictions.Where(() => secondWarehouseAlias.Name == null),
						Projections.Constant("Не указан", NHibernateUtil.String),
						Projections.Property(() => secondWarehouseAlias.Name)))
					.WithAlias (() => resultAlias.SecondWarehouse))
				.TransformUsing (Transformers.AliasToBean<DocumentVMNode> ())
				.List<DocumentVMNode> ();
			
			var writeoffList = UoW.Session.QueryOver<WriteoffDocument> (() => writeoffAlias)
				.JoinQueryOver(() => writeoffAlias.Client, () => counterpartyAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.JoinQueryOver(() => writeoffAlias.WriteoffWarehouse, () => warehouseAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.SelectList (list => list
					.Select (() => writeoffAlias.Id).WithAlias (() => resultAlias.Id)
					.Select (() => writeoffAlias.TimeStamp).WithAlias (() => resultAlias.Date)
					.Select (() => "Акт списания").WithAlias (() => resultAlias.DocType)
					.Select(Projections.Conditional(
						Restrictions.Where(() => counterpartyAlias.Name == null),
						Projections.Constant(String.Empty, NHibernateUtil.String),
						Projections.Property(() => counterpartyAlias.Name)))
					.WithAlias (() => resultAlias.Counterparty)
					.Select(Projections.Conditional(
						Restrictions.Where(() => warehouseAlias.Name == null),
						Projections.Constant(String.Empty, NHibernateUtil.String),
						Projections.Property(() => warehouseAlias.Name)))
					.WithAlias (() => resultAlias.Warehouse))
				.TransformUsing (Transformers.AliasToBean<DocumentVMNode> ())
				.List<DocumentVMNode> ();

			List<DocumentVMNode> result = new List<DocumentVMNode> ();
			result.AddRange (invoiceList);
			result.AddRange (waterList);
			result.AddRange (movementList);
			result.AddRange (writeoffList);

			result.Sort ((x, y) => { 
				if (x.Date > y.Date)
					return 1;
				if (x.Date == y.Date)
					return 0;
				return -1;
			});

			SetItemsSource (result);
		}

		IMappingConfig treeViewConfig = FluentMappingConfig<DocumentVMNode>.Create ()
			.AddColumn ("Номер").SetDataProperty (node => node.Id.ToString())
			.AddColumn ("Тип документа").SetDataProperty (node => node.DocType)
			.AddColumn ("Дата").SetDataProperty (node => node.DateString)
			.AddColumn ("Детали").SetDataProperty (node => node.Description)
			.Finish ();

		public override IMappingConfig TreeViewConfig {
			get { return treeViewConfig; }
		}

		#endregion

		#region implemented abstract members of RepresentationModelBase

		protected override bool NeedUpdateFunc (Document updatedSubject)
		{
			return true;
		}

		protected override bool NeedUpdateFunc (object updatedSubject)
		{
			return true;
		}

		#endregion

		public DocumentsVM () : this(UnitOfWorkFactory.CreateWithoutRoot ())
		{
			
		}

		public DocumentsVM (IUnitOfWork uow) : base (
			typeof(IncomingInvoiceDlg),
			typeof(IncomingWaterDlg),
			typeof(MovementDocumentDlg),
			typeof(WriteoffDocumentDlg))
		{
			this.UoW = uow;
		}
	}

	public class DocumentVMNode
	{

		public int Id { get; set; }

		public string DocType { get; set; }

		public DateTime Date { get; set; }

		public string DateString { get { return Date.ToShortDateString () + " " + Date.ToShortTimeString (); } }

		public string Description { 
			get {
				switch (DocType) {
				case "Входящая накладная":
					return String.Format ("Поставщик: {0}; Склад поступления: {1};", Counterparty, Warehouse);
				case "Документ производства":
					return String.Format ("Количество: {0}; Склад поступления: {1};", Amount, Warehouse); 
				case "Документ перемещения": 
					if (MDCategory == MovementDocumentCategory.counterparty)
						return String.Format ("\"{0}\" -> \"{1}\"", Counterparty, SecondCounterparty);
					return String.Format ("{0} -> {1}", Warehouse, SecondWarehouse); 
				case "Акт списания":
					if (Warehouse != String.Empty)
						return String.Format ("Со склада \"{0}\"", Warehouse);
					if (Counterparty != String.Empty)
						return String.Format ("От клиента \"{0}\"", Counterparty);
					return "";
				default:
					return "";
				}
			}
		}

		public string Counterparty { get; set; }

		public string SecondCounterparty { get; set; }

		public string Warehouse { get; set; }

		public string SecondWarehouse { get; set; }

		public int Amount { get; set; } 

		public MovementDocumentCategory MDCategory { get; set; }
	}
}

