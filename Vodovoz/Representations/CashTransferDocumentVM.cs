using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.ColumnConfig;
using Gamma.Utilities;
using NHibernate.Transform;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.RepresentationModel.GtkUI;
using QS.Utilities.Text;
using QSProjectsLib;
using Vodovoz.Core.Journal;
using Vodovoz.Dialogs.Cash.CashTransfer;
using Vodovoz.Domain.Cash.CashTransfer;
using Vodovoz.Domain.Employees;
using Vodovoz.Tools;
using Vodovoz.ViewModelBased;

namespace Vodovoz.Representations
{
	public class CashTransferDocumentVM : MultipleEntityModelBase<CashTransferDocumentVMNode>
	{
		private CashTransferDocumentVMNode resultAlias = null;

		public CashTransferDocumentVM()
		{
			UoW = UnitOfWorkFactory.CreateWithoutRoot();
			RepresentationFilter = null;
			JournalFilter = null;

			RegisterIncomeTransfer();
			RegisterCommonTransfer();

			FinalListFunction = OrderFunc;

			TreeViewConfig = FluentColumnsConfig<CashTransferDocumentVMNode>.Create()
				.AddColumn("№").AddTextRenderer(node => node.DocumentId.ToString())
				.AddColumn("Тип").AddTextRenderer(node => node.DisplayName)
				.AddColumn("Дата").AddTextRenderer(node => node.CreationDate.ToShortDateString())
				.AddColumn("Автор").AddTextRenderer(node => node.Author)
				.AddColumn("Статус").AddTextRenderer(node => node.Status.GetEnumTitle())
				.AddColumn("Сумма").AddTextRenderer(node => CurrencyWorks.GetShortCurrencyString(node.TransferedSum))

				.AddColumn("Отправлено из").AddTextRenderer(node => node.SubdivisionFrom)
				.AddColumn("Время отпр.").AddTextRenderer(node => node.SendTime.HasValue ? node.SendTime.Value.ToShortDateString() : "")
				.AddColumn("Отправил").AddTextRenderer(node => node.CashierSender)

				.AddColumn("Отправлено в").AddTextRenderer(node => node.SubdivisionTo)
				.AddColumn("Время принятия").AddTextRenderer(node => node.ReceiveTime.HasValue ? node.ReceiveTime.Value.ToShortDateString() : "")
				.AddColumn("Принял").AddTextRenderer(node => node.CashierReceiver)

				.AddColumn("Комментарий").AddTextRenderer(node => node.Comment)
				.Finish();

		}

		private void RegisterIncomeTransfer()
		{
			var incomeTransferConfig = RegisterEntity<IncomeCashTransferDocument>();
			//функция получения данных
			incomeTransferConfig.AddDataFunction(() => {
				IList<CashTransferDocumentVMNode> incomeTransferResultList = new List<CashTransferDocumentVMNode>();

				IncomeCashTransferDocument incomeTransferAlias = null;
				Employee authorAlias = null;
				Employee cashierSenderAlias = null;
				Employee cashierReceiverAlias = null;
				Subdivision subdivisionFromAlias = null;
				Subdivision subdivisionToAlias = null;

				var incomeTransferQuery = UoW.Session.QueryOver<IncomeCashTransferDocument>(() => incomeTransferAlias);

				incomeTransferResultList = incomeTransferQuery
					.JoinQueryOver(() => incomeTransferAlias.Author, () => authorAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
					.JoinQueryOver(() => incomeTransferAlias.CashierSender, () => cashierSenderAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
					.JoinQueryOver(() => incomeTransferAlias.CashierReceiver, () => cashierReceiverAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
					.JoinQueryOver(() => incomeTransferAlias.CashSubdivisionFrom, () => subdivisionFromAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
					.JoinQueryOver(() => incomeTransferAlias.CashSubdivisionTo, () => subdivisionToAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
					.SelectList(list => list
						.Select(() => incomeTransferAlias.Id).WithAlias(() => resultAlias.DocumentId)
						.Select(() => incomeTransferAlias.CreationDate).WithAlias(() => resultAlias.CreationDate)
						.Select(() => incomeTransferAlias.TransferedSum).WithAlias(() => resultAlias.TransferedSum)
						.Select(() => incomeTransferAlias.Comment).WithAlias(() => resultAlias.Comment)
						.Select(() => incomeTransferAlias.Status).WithAlias(() => resultAlias.Status)
						.Select(() => incomeTransferAlias.SendTime).WithAlias(() => resultAlias.SendTime)
						.Select(() => incomeTransferAlias.ReceiveTime).WithAlias(() => resultAlias.ReceiveTime)

						.Select(() => authorAlias.Name).WithAlias(() => resultAlias.AuthorName)
						.Select(() => authorAlias.LastName).WithAlias(() => resultAlias.AuthorSurname)
						.Select(() => authorAlias.Patronymic).WithAlias(() => resultAlias.AuthorPatronymic)

						.Select(() => cashierSenderAlias.Name).WithAlias(() => resultAlias.CashierSenderName)
						.Select(() => cashierSenderAlias.LastName).WithAlias(() => resultAlias.CashierSenderSurname)
						.Select(() => cashierSenderAlias.Patronymic).WithAlias(() => resultAlias.CashierSenderPatronymic)

						.Select(() => cashierReceiverAlias.Name).WithAlias(() => resultAlias.CashierReceiverName)
						.Select(() => cashierReceiverAlias.LastName).WithAlias(() => resultAlias.CashierReceiverSurname)
						.Select(() => cashierReceiverAlias.Patronymic).WithAlias(() => resultAlias.CashierReceiverPatronymic)

						.Select(() => subdivisionFromAlias.Name).WithAlias(() => resultAlias.SubdivisionFrom)
						.Select(() => subdivisionToAlias.Name).WithAlias(() => resultAlias.SubdivisionTo)
					)

					.TransformUsing(Transformers.AliasToBean<CashTransferDocumentVMNode<IncomeCashTransferDocument>>())
					.List<CashTransferDocumentVMNode>();

				return incomeTransferResultList;
			});

			incomeTransferConfig.AddViewModelBasedDocumentConfiguration<IncomeCashTransferDlg>(
				//функция идентификации документа 
				(CashTransferDocumentVMNode node) => {
					return node.EntityType == typeof(IncomeCashTransferDocument);
				},
				//заголовок действия для создания нового документа
				"По ордерам",
				//функция диалога создания документа
				() => {
					var viewModel = new IncomeCashTransferDocumentViewModel(EntityOpenOption.Create());
					return viewModel.View as IncomeCashTransferDlg;
				},
				//функция диалога открытия документа
				(CashTransferDocumentVMNode node) => {
					var viewModel = new IncomeCashTransferDocumentViewModel(EntityOpenOption.Open(node.DocumentId));
					return viewModel.View as IncomeCashTransferDlg;
				}
			);

			//завершение конфигурации
			incomeTransferConfig.FinishConfiguration();
		}

		private void RegisterCommonTransfer()
		{
			var commonTransferConfig = RegisterEntity<CommonCashTransferDocument>();
			//функция получения данных
			commonTransferConfig.AddDataFunction(() => {
				IList<CashTransferDocumentVMNode> commonTransferResultList = new List<CashTransferDocumentVMNode>();

				CommonCashTransferDocument commonTransferAlias = null;
				Employee authorAlias = null;
				Employee cashierSenderAlias = null;
				Employee cashierReceiverAlias = null;
				Subdivision subdivisionFromAlias = null;
				Subdivision subdivisionToAlias = null;

				var commonTransferQuery = UoW.Session.QueryOver<CommonCashTransferDocument>(() => commonTransferAlias);

				commonTransferResultList = commonTransferQuery
					.JoinQueryOver(() => commonTransferAlias.Author, () => authorAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
					.JoinQueryOver(() => commonTransferAlias.CashierSender, () => cashierSenderAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
					.JoinQueryOver(() => commonTransferAlias.CashierReceiver, () => cashierReceiverAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
					.JoinQueryOver(() => commonTransferAlias.CashSubdivisionFrom, () => subdivisionFromAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
					.JoinQueryOver(() => commonTransferAlias.CashSubdivisionTo, () => subdivisionToAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
					.SelectList(list => list
						.Select(() => commonTransferAlias.Id).WithAlias(() => resultAlias.DocumentId)
						.Select(() => commonTransferAlias.CreationDate).WithAlias(() => resultAlias.CreationDate)
						.Select(() => commonTransferAlias.TransferedSum).WithAlias(() => resultAlias.TransferedSum)
						.Select(() => commonTransferAlias.Comment).WithAlias(() => resultAlias.Comment)
						.Select(() => commonTransferAlias.Status).WithAlias(() => resultAlias.Status)
						.Select(() => commonTransferAlias.SendTime).WithAlias(() => resultAlias.SendTime)
						.Select(() => commonTransferAlias.ReceiveTime).WithAlias(() => resultAlias.ReceiveTime)

						.Select(() => authorAlias.Name).WithAlias(() => resultAlias.AuthorName)
						.Select(() => authorAlias.LastName).WithAlias(() => resultAlias.AuthorSurname)
						.Select(() => authorAlias.Patronymic).WithAlias(() => resultAlias.AuthorPatronymic)

						.Select(() => cashierSenderAlias.Name).WithAlias(() => resultAlias.CashierSenderName)
						.Select(() => cashierSenderAlias.LastName).WithAlias(() => resultAlias.CashierSenderSurname)
						.Select(() => cashierSenderAlias.Patronymic).WithAlias(() => resultAlias.CashierSenderPatronymic)

						.Select(() => cashierReceiverAlias.Name).WithAlias(() => resultAlias.CashierReceiverName)
						.Select(() => cashierReceiverAlias.LastName).WithAlias(() => resultAlias.CashierReceiverSurname)
						.Select(() => cashierReceiverAlias.Patronymic).WithAlias(() => resultAlias.CashierReceiverPatronymic)

						.Select(() => subdivisionFromAlias.Name).WithAlias(() => resultAlias.SubdivisionFrom)
						.Select(() => subdivisionToAlias.Name).WithAlias(() => resultAlias.SubdivisionTo)
					)

					.TransformUsing(Transformers.AliasToBean<CashTransferDocumentVMNode<CommonCashTransferDocument>>())
					.List<CashTransferDocumentVMNode>();

				return commonTransferResultList;
			});

			commonTransferConfig.AddViewModelBasedDocumentConfiguration<CommonCashTransferDlg>(
				//функция идентификации документа 
				(CashTransferDocumentVMNode node) => {
					return node.EntityType == typeof(CommonCashTransferDocument);
				},
				//заголовок действия для создания нового документа
				"На сумму",
				//функция диалога создания документа
				() => {
					var viewModel = new CommonCashTransferDocumentViewModel(EntityOpenOption.Create());
					return viewModel.View as CommonCashTransferDlg;
				},
				//функция диалога открытия документа
				(CashTransferDocumentVMNode node) => {
					var viewModel = new CommonCashTransferDocumentViewModel(EntityOpenOption.Open(node.DocumentId));
					return viewModel.View as CommonCashTransferDlg;
				}
			);

			//завершение конфигурации
			commonTransferConfig.FinishConfiguration();
		}

		List<CashTransferDocumentVMNode> OrderFunc(List<CashTransferDocumentVMNode> arg)
		{
			return arg.OrderByDescending(x => x.CreationDate).ToList();
		}
	}

	public class CashTransferDocumentVMNode<TEntity> : CashTransferDocumentVMNode
		where TEntity : class, IDomainObject
	{
		public CashTransferDocumentVMNode()
		{
			EntityType = typeof(TEntity);
		}
	}

	public class CashTransferDocumentVMNode : MultipleEntityVMNodeBase
	{
		#region MultipleDocumentJournalVMNodeBase implementation

		public override Type EntityType { get; set; }

		public override int DocumentId { get; set; }

		[UseForSearch]
		public override string DisplayName {
			get {
				if(EntityType == typeof(IncomeCashTransferDocument)) {
					return "По ордерам";
				}else if(EntityType == typeof(CommonCashTransferDocument)) {
					return "На сумму";
				} else {
					return "Перемещение д/с";
				}
			}
		}

		#endregion

		public DateTime CreationDate { get; set; }
		public CashTransferDocumentStatuses Status { get; set; }

		public string AuthorSurname { get; set; }
		public string AuthorName { get; set; }
		public string AuthorPatronymic { get; set; }
		[UseForSearch]
		public string Author => PersonHelper.PersonNameWithInitials(AuthorSurname, AuthorName, AuthorPatronymic);

		[UseForSearch]
		public decimal TransferedSum { get; set; }
		public string SubdivisionFrom { get; set; }
		public string SubdivisionTo { get; set; }
		public DateTime? SendTime { get; set; }
		public DateTime? ReceiveTime { get; set; }

		public string CashierSenderSurname { get; set; }
		public string CashierSenderName { get; set; }
		public string CashierSenderPatronymic { get; set; }
		[UseForSearch]
		public string CashierSender => PersonHelper.PersonNameWithInitials(CashierSenderSurname, CashierSenderName, CashierSenderPatronymic);

		public string CashierReceiverSurname { get; set; }
		public string CashierReceiverName { get; set; }
		public string CashierReceiverPatronymic { get; set; }
		[UseForSearch]
		public string CashierReceiver => PersonHelper.PersonNameWithInitials(CashierReceiverSurname, CashierReceiverName, CashierReceiverPatronymic);

		[UseForSearch]
		public string Comment { get; set; }
	}
}
