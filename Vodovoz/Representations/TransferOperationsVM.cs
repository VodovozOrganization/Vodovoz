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
	public class TransferOperationsVM: RepresentationModelEntityBase<TransferOperationDocument, TransferOperationVMNode>
	{
		public override void UpdateNodes()
		{
			TransferOperationDocument transferAlias = null;
			Employee authorAlias = null;
			Employee lastEditorAlias = null;
			Counterparty fromCounterpartyAlias = null;
			Counterparty toCounterpartyAlias = null;
			DeliveryPoint fromDeliveryPointAlias = null;
			DeliveryPoint toDeliveryPointAlias = null;
			TransferOperationVMNode resultAlias = null;
			var result = new List<TransferOperationVMNode>();

			var transferQuery = UoW.Session.QueryOver<TransferOperationDocument>(() => transferAlias)
								   .JoinQueryOver(() => transferAlias.Author, () => authorAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
								   .JoinQueryOver(() => transferAlias.LastEditor, () => lastEditorAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
								   .JoinQueryOver(() => transferAlias.FromClient, () => fromCounterpartyAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
								   .JoinQueryOver(() => transferAlias.ToClient, () => toCounterpartyAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
								   .JoinQueryOver(() => transferAlias.FromDeliveryPoint, () => fromDeliveryPointAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
								   .JoinQueryOver(() => transferAlias.ToDeliveryPoint, () => toDeliveryPointAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin);

			var transferList = transferQuery
				.SelectList(list => list
							.Select(() => transferAlias.Id).WithAlias(() => resultAlias.Id)
							.Select(() => transferAlias.TimeStamp).WithAlias(() => resultAlias.Date)
				            .Select(() => transferAlias.Comment).WithAlias(() => resultAlias.Comment)
							.Select(() => fromCounterpartyAlias.Name).WithAlias(() => resultAlias.FromCounterparty)
							.Select(() => toCounterpartyAlias.Name).WithAlias(() => resultAlias.ToCounterparty)
							.Select(() => fromDeliveryPointAlias.ShortAddress).WithAlias(() => resultAlias.FromDeliveryPoint)
							.Select(() => toDeliveryPointAlias.ShortAddress).WithAlias(() => resultAlias.ToDeliveryPoint)
							.Select(() => authorAlias.LastName).WithAlias(() => resultAlias.AuthorSurname)
							.Select(() => authorAlias.Name).WithAlias(() => resultAlias.AuthorName)
							.Select(() => authorAlias.Patronymic).WithAlias(() => resultAlias.AuthorPatronymic)
							.Select(() => lastEditorAlias.LastName).WithAlias(() => resultAlias.LastEditorSurname)
							.Select(() => lastEditorAlias.Name).WithAlias(() => resultAlias.LastEditorName)
							.Select(() => lastEditorAlias.Patronymic).WithAlias(() => resultAlias.LastEditorPatronymic))
				.TransformUsing(Transformers.AliasToBean<TransferOperationVMNode>())
				.List<TransferOperationVMNode>();
			
			result.AddRange(transferList);

			result.Sort((x, y) => {
				if(x.Date < y.Date) return 1;
				if(x.Date == y.Date) return 0;
				return -1;
			});

			SetItemsSource(result);

		}

		IColumnsConfig columnsConfig = FluentColumnsConfig<TransferOperationVMNode>.Create()
										.AddColumn("ID").AddTextRenderer(node => String.Format("Перенос №{0}", node.Id)).SearchHighlight()
										.AddColumn("Дата").SetDataProperty(node => node.DateString)
										.AddColumn("От клиента").SetDataProperty(node => node.FromCounterparty)
										.AddColumn("Откуда").SetDataProperty(node => node.FromDeliveryPoint)
										.AddColumn("К клиенту").SetDataProperty(node => node.ToCounterparty)
										.AddColumn("Куда").SetDataProperty(node => node.ToDeliveryPoint)
										.AddColumn("Автор").SetDataProperty(node => node.Author)
										.AddColumn("Автор последней правки").SetDataProperty(node => node.LastEditor)
										.AddColumn("Коментарий").SetDataProperty(node => node.Comment)
										.Finish();

		public override IColumnsConfig ColumnsConfig {
			get { return columnsConfig; }
		}

		protected override bool NeedUpdateFunc(TransferOperationDocument updatedSubject)
		{
			return true;
		}

		public TransferOperationsVM() : this(UnitOfWorkFactory.CreateWithoutRoot())
		{
			
		}

		public TransferOperationsVM(IUnitOfWork uow)
		{
			this.UoW = uow;
		}
	}

	public class TransferOperationVMNode
	{
		[UseForSearch]
		public int Id { get; set; }

		public DateTime Date { get; set; }

		public string DateString { get { return Date.ToShortDateString() + " " + Date.ToShortTimeString(); } }

		public string FromCounterparty { get; set; }
		public string FromDeliveryPoint { get; set; }
		public string ToCounterparty { get; set; }
		public string ToDeliveryPoint { get; set; }

		public string Comment { get; set; }

		public string AuthorSurname { get; set; }
		public string AuthorName { get; set; }
		public string AuthorPatronymic { get; set; }

		public string Author { get { return StringWorks.PersonNameWithInitials(AuthorSurname, AuthorName, AuthorPatronymic); } }

		public string LastEditorSurname { get; set; }
		public string LastEditorName { get; set; }
		public string LastEditorPatronymic { get; set; }

		public string LastEditor { get { return StringWorks.PersonNameWithInitials(LastEditorSurname, LastEditorName, LastEditorPatronymic); } }
	}
}
