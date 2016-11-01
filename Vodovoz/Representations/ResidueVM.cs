using System;
using System.Collections.Generic;
using Gamma.ColumnConfig;
using Gamma.Utilities;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using QSOrmProject;
using QSOrmProject.RepresentationModel;
using QSProjectsLib;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Store;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain;

namespace Vodovoz.ViewModel
{
	public class ResidueVM : RepresentationModelEntityBase<Residue, ResidueVMNode>
	{
		

		#region IRepresentationModel implementation

		public override void UpdateNodes ()
		{
			Counterparty counterpartyAlias = null;
			Employee authorAlias = null;
			Employee lastEditorAlias = null;
			//DepositOperation depositEquipmentOperation = null;
			//DepositOperation depositBottlesOperation = null;
			ResidueVMNode resultAlias = null;
			//MoneyMovementOperation moneyMovementOperation = null;
			Residue residueAlias = null;
			DeliveryPoint deliveryPointAlias = null;
			var result = new List<ResidueVMNode> ();


			  
			var residueQuery = UoW.Session.QueryOver<Residue>(() => residueAlias) 
				.JoinQueryOver(() => residueAlias.Customer, () => counterpartyAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.JoinQueryOver(() => residueAlias.DeliveryPoint, () => deliveryPointAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.JoinQueryOver(() => residueAlias.LastEditAuthor, () => lastEditorAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.JoinQueryOver(() => residueAlias.Author, () => authorAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin);



				var residueList = residueQuery
					.SelectList (list => list
						.Select (() => residueAlias.Id).WithAlias (() => resultAlias.Id)
						.Select (() => residueAlias.Date).WithAlias (() => resultAlias.Date)
						.Select (() => counterpartyAlias.Name).WithAlias (() => resultAlias.Counterparty)
						.Select (() => deliveryPointAlias.ShortAddress).WithAlias (() => resultAlias.DeliveryPoint)
						.Select (() => authorAlias.LastName).WithAlias (() => resultAlias.AuthorSurname)
						.Select (() => authorAlias.Name).WithAlias (() => resultAlias.AuthorName)
						.Select (() => authorAlias.Patronymic).WithAlias (() => resultAlias.AuthorPatronymic)
						.Select (() => lastEditorAlias.LastName).WithAlias (() => resultAlias.LastEditorSurname)
						.Select (() => lastEditorAlias.Name).WithAlias (() => resultAlias.LastEditorName)
						.Select (() => lastEditorAlias.Patronymic).WithAlias (() => resultAlias.LastEditorPatronymic)
						.Select (() => residueAlias.LastEditTime).WithAlias (() => resultAlias.LastEditedTime))
					.TransformUsing (Transformers.AliasToBean<ResidueVMNode> ())
					.List<ResidueVMNode> ();

				result.AddRange (residueList);


			result.Sort ((x, y) => { 
				if (x.Date < y.Date)  return 1;
				if (x.Date == y.Date) return 0;
									  return -1;
			});

			SetItemsSource (result);
		}

		IColumnsConfig columnsConfig = FluentColumnsConfig<ResidueVMNode>.Create()
			.AddColumn("Документ").AddTextRenderer(node => String.Format("Ввод остатков №{0}", node.Id)).SearchHighlight()
			.AddColumn("Дата").SetDataProperty(node => node.DateString)
			.AddColumn("Контрагент").SetDataProperty(NodeType => NodeType.Counterparty)
			.AddColumn("Точка доставки").SetDataProperty(NodeType => NodeType.DeliveryPoint)
			.AddColumn ("Автор").SetDataProperty (node => node.Author)
			.AddColumn ("Изменил").SetDataProperty (node => node.LastEditor)
			.AddColumn ("Послед. изменения").AddTextRenderer(node => node.LastEditedTime != default(DateTime) ? node.LastEditedTime.ToString() : String.Empty)
			//.AddColumn ("Комментарий").SetDataProperty (node => node.Comment)
			.Finish ();

		public override IColumnsConfig ColumnsConfig {
			get { return columnsConfig; }
		}

		#endregion

		#region implemented abstract members of RepresentationModelBase

		protected override bool NeedUpdateFunc (Residue updatedSubject)
		{
			return true;
		}

		#endregion



		public ResidueVM () : this(UnitOfWorkFactory.CreateWithoutRoot ())
		{
			//CreateRepresentationFilter = () => new StockDocumentsFilter(UoW);
		}

		public ResidueVM (IUnitOfWork uow)
		{
			this.UoW = uow;
		}
	}

	public class ResidueVMNode
	{
		[UseForSearch]
		public int Id { get; set; }

		public DateTime Date { get; set; }

		public string DateString { get { return  Date.ToShortDateString () + " " + Date.ToShortTimeString (); } }

		public string Counterparty { get; set; }


		public string Comment { get; set; }


		public DateTime LastEditedTime { get; set; }

		public string AuthorSurname { get; set; }
		public string AuthorName { get; set; }
		public string AuthorPatronymic { get; set; }

		public string Author {get{return StringWorks.PersonNameWithInitials(AuthorSurname, AuthorName, AuthorPatronymic);}}

		public string LastEditorSurname { get; set; }
		public string LastEditorName { get; set; }
		public string LastEditorPatronymic { get; set; }

		public string LastEditor {get{return StringWorks.PersonNameWithInitials(LastEditorSurname, LastEditorName, LastEditorPatronymic);}}

		public string DeliveryPoint { get; set; }
	}
}

