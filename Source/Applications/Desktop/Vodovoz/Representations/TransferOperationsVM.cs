using System;
using System.Collections.Generic;
using Gamma.ColumnConfig;
using NHibernate;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Utilities.Text;
using QSOrmProject.RepresentationModel;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Employees;

namespace Vodovoz.ViewModel
{
	public class TransferOperationsVM : RepresentationModelEntityBase<TransferOperationDocument, TransferOperationVMNode>
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
								   .JoinEntityAlias(() => authorAlias, () => transferAlias.AuthorId == authorAlias.Id, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
								   .JoinEntityAlias(() => lastEditorAlias, () => transferAlias.LastEditorId == lastEditorAlias.Id, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
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
			.AddColumn("Дата").AddTextRenderer(node => node.DateString)
			.AddColumn("От клиента").AddTextRenderer(node => node.FromCounterparty)
				.WrapMode(Pango.WrapMode.WordChar)
				.WrapWidth(500)
			.AddColumn("Откуда").AddTextRenderer(node => node.FromDeliveryPoint)
				.WrapMode(Pango.WrapMode.WordChar)
				.WrapWidth(500)
			.AddColumn("К клиенту").AddTextRenderer(node => node.ToCounterparty)
				.WrapMode(Pango.WrapMode.WordChar)
				.WrapWidth(500)
			.AddColumn("Куда").AddTextRenderer(node => node.ToDeliveryPoint)
				.WrapMode(Pango.WrapMode.WordChar)
				.WrapWidth(500)
			.AddColumn("Автор").AddTextRenderer(node => node.Author)
			.AddColumn("Автор последней правки").AddTextRenderer(node => node.LastEditor)
			.AddColumn("Коментарий").AddTextRenderer(node => node.Comment)
				.WrapMode(Pango.WrapMode.WordChar)
				.WrapWidth(500)
			.Finish();

		public override IColumnsConfig ColumnsConfig => columnsConfig;

		protected override bool NeedUpdateFunc(TransferOperationDocument updatedSubject) => true;

		public TransferOperationsVM() { }

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

		public string DateString => Date.ToShortDateString() + " " + Date.ToShortTimeString();

		[UseForSearch]
		[SearchHighlight]
		public string FromCounterparty { get; set; }

		string fromDeliveryPoint;
		[UseForSearch]
		[SearchHighlight]
		public string FromDeliveryPoint {
			get => string.IsNullOrEmpty(fromDeliveryPoint) ? "Самовывоз" : fromDeliveryPoint;
			set => fromDeliveryPoint = value;
		}
		[UseForSearch]
		[SearchHighlight]
		public string ToCounterparty { get; set; }

		string toDeliveryPoint;
		[UseForSearch]
		[SearchHighlight]
		public string ToDeliveryPoint {
			get => string.IsNullOrEmpty(toDeliveryPoint) ? "Самовывоз" : toDeliveryPoint;
			set => toDeliveryPoint = value;
		}

		public string Comment { get; set; }

		public string AuthorSurname { get; set; }
		public string AuthorName { get; set; }
		public string AuthorPatronymic { get; set; }

		public string Author => PersonHelper.PersonNameWithInitials(AuthorSurname, AuthorName, AuthorPatronymic);

		public string LastEditorSurname { get; set; }
		public string LastEditorName { get; set; }
		public string LastEditorPatronymic { get; set; }

		public string LastEditor => PersonHelper.PersonNameWithInitials(LastEditorSurname, LastEditorName, LastEditorPatronymic);
	}
}
