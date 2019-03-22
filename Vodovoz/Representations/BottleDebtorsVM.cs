using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.ColumnConfig;
using Gamma.Utilities;
using Gtk;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QSOrmProject.RepresentationModel;
using Vodovoz.Domain;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Operations;
using Vodovoz.JournalFilters;

namespace Vodovoz.Representations
{
	public class BottleDebtorsVM : RepresentationModelEntityBase<DeliveryPoint , BottleDebtorsVMNode>
	{
		public BottleDebtorsFilter Filter {
			get { return RepresentationFilter as BottleDebtorsFilter; }
			set { RepresentationFilter = value as IRepresentationFilter; }
		}

		public override void UpdateNodes()
		{
			DeliveryPoint deliveryPointAlias = null;
			DeliveryPoint deliveryPointOrderAlias = null;
			Counterparty counterpartyAlias = null;
			BottlesMovementOperation bottlesMovementAlias = null;
			BottleDebtorsVMNode resultAlias = null;
			Residue residueAlias = null;
			CallTask taskAlias = null;
			Domain.Orders.Order orderAlias = null;

			var pointsQuery = UoW.Session.QueryOver<DeliveryPoint>(() => deliveryPointAlias)
			.Where(() => deliveryPointAlias.IsActive == true);

			var bottleDebtByAddressQuery = UoW.Session.QueryOver<BottlesMovementOperation>(() => bottlesMovementAlias)
			.Where(() => bottlesMovementAlias.DeliveryPoint.Id == deliveryPointAlias.Id)
			.Select(
				Projections.SqlFunction(new SQLFunctionTemplate(NHibernateUtil.Int32, "( ?2 - ?1 )"),
					NHibernateUtil.Int32, new IProjection[] {
					Projections.Sum(() => bottlesMovementAlias.Returned),
					Projections.Sum(() => bottlesMovementAlias.Delivered)}
				));

			var residueQuery = UoW.Session.QueryOver<Residue>(() => residueAlias)
			.Where(() => residueAlias.DeliveryPoint.Id == deliveryPointAlias.Id)
			.Select((res) => res.Id) 
			.Take(1);

			var bottleDebtByClientQuery = UoW.Session.QueryOver<BottlesMovementOperation>(() => bottlesMovementAlias)
			.Where(() => bottlesMovementAlias.Counterparty.Id == counterpartyAlias.Id)
			.Select(
				Projections.SqlFunction(new SQLFunctionTemplate(NHibernateUtil.Int32, "( ?2 - ?1 )"),
					NHibernateUtil.Int32, new IProjection[] {
								Projections.Sum(() => bottlesMovementAlias.Returned),
								Projections.Sum(() => bottlesMovementAlias.Delivered)}
				));

			var LastOrderQuery = UoW.Session.QueryOver<Vodovoz.Domain.Orders.Order>(() => orderAlias)
				.JoinAlias(c => c.DeliveryPoint, () => deliveryPointOrderAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.Where((x) => x.DeliveryPoint.Id == deliveryPointAlias.Id)
				.Select((x) => x.DeliveryDate)
				.OrderBy(() => orderAlias.Id).Desc
				.Take(1);

			var TaskExistQuery = UoW.Session.QueryOver<CallTask>(() => taskAlias)
				.Where(x => x.Address.Id == deliveryPointAlias.Id)
				.Select(x => x.Id)
				.Take(1);

			if(Filter.Client != null)
				pointsQuery = pointsQuery.Where((arg) => arg.Counterparty.Id == Filter.Client.Id);
			if(Filter.Address != null)
				pointsQuery = pointsQuery.Where((arg) => arg.Id == Filter.Address.Id);
			if(Filter.OPF != null)
				pointsQuery = pointsQuery.Where((arg) => arg.Counterparty.PersonType == Filter.OPF);

			var debtorslist = pointsQuery
				.JoinAlias(c => c.Counterparty, () => counterpartyAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.SelectList(list => list
				   .Select(() => deliveryPointAlias.Id).WithAlias(() => resultAlias.AddressId)
				   .Select(() => counterpartyAlias.Id).WithAlias(() => resultAlias.ClientId)
				   .Select(() => counterpartyAlias.Name).WithAlias(() => resultAlias.ClientName)
				   .Select(() => deliveryPointAlias.ShortAddress).WithAlias(() => resultAlias.AddressName)
				   .Select(() => deliveryPointAlias.BottleReserv).WithAlias(() => resultAlias.Reserve)
				   .Select(() => counterpartyAlias.PersonType).WithAlias(() => resultAlias.OPF)
				   .SelectSubQuery((QueryOver<Domain.Orders.Order>)LastOrderQuery).WithAlias(() => resultAlias.LastOrderDate)
				   .SelectSubQuery((QueryOver<Residue>)residueQuery).WithAlias(() => resultAlias.Residue)
				   .SelectSubQuery((QueryOver<BottlesMovementOperation>)bottleDebtByAddressQuery).WithAlias(() => resultAlias.DebtByAddress)
				   .SelectSubQuery((QueryOver<BottlesMovementOperation>)bottleDebtByClientQuery).WithAlias(() => resultAlias.DebtByClient)
				   .SelectSubQuery((QueryOver<CallTask>)TaskExistQuery).WithAlias(() => resultAlias.ExistTask)
					 )
				.TransformUsing(Transformers.AliasToBean<BottleDebtorsVMNode>())
				.OrderBy(x => x.Counterparty.Id).Desc
				.List<BottleDebtorsVMNode>();

			if(Filter.OPF != null)
				debtorslist = debtorslist.Where((arg) => arg.OPF == Filter.OPF).ToList();
			if(Filter.StartDate != null && Filter.EndDate != null)
				debtorslist = debtorslist.Where((arg) => Filter.StartDate.Value <= arg.LastOrderDate && arg.LastOrderDate <= Filter.EndDate.Value ).ToList();
			if(Filter.DebtFrom != null && Filter.DebtBy != null)
				debtorslist = debtorslist.Where((arg) => Filter.DebtFrom.Value <= arg.DebtByAddress && arg.DebtByAddress <= Filter.DebtBy.Value).ToList();

			SetItemsSource(debtorslist);
		}


		IColumnsConfig columnsConfig = FluentColumnsConfig<BottleDebtorsVMNode>.Create()
			.AddColumn("Номер").AddTextRenderer(x => x.AddressId.ToString())
			.AddColumn("Клиент").AddTextRenderer(node => node.ClientName)
			.AddColumn("Адрес").AddTextRenderer(node => node.AddressName)
			.AddColumn("ОПФ").AddTextRenderer(node => node.OPF.GetEnumTitle())
			.AddColumn("Последний заказ по адресу").AddTextRenderer(node => node.LastOrderDate != null ? node.LastOrderDate.Value.ToString("dd / MM / yyyy") : String.Empty)
			.AddColumn("Долг по таре (по адресу)").AddTextRenderer(node => node.DebtByAddress.ToString())
			.AddColumn("Долг по таре (по клиенту)").AddTextRenderer(node => node.DebtByClient.ToString())
			.AddColumn("Ввод остат.").AddTextRenderer(node => node.IsResidue ? "есть" : "нет")
			.AddColumn("Резерв").AddTextRenderer(node => node.Reserve.ToString())
			.RowCells().AddSetter<CellRendererText>((c, n) => c.Foreground = n.RowColor)
			.Finish();

		public override IColumnsConfig ColumnsConfig {
			get { return columnsConfig; }
		}

		protected override bool NeedUpdateFunc(DeliveryPoint updatedSubject) => true;

		public BottleDebtorsVM() 
		{
			this.UoW = UnitOfWorkFactory.CreateWithoutRoot();
		}

		public BottleDebtorsVM(IUnitOfWork uow)
		{
			this.UoW = uow;
		}

		public BottleDebtorsVM(BottleDebtorsFilter filter) : this(filter.UoW)
		{
			Filter = filter;
		}

		public void CreateTask(BottleDebtorsVMNode[] bottleDebtors)
		{
			foreach(var item in bottleDebtors) {
				CallTask task = new CallTask();
				task.Address = UoW.GetById<DeliveryPoint>(item.AddressId);
				task.DateOfTaskCreation = DateTime.Now;
				task.Deadline = DateTime.Now.AddDays(1);
				UoW.Save(task);
			}
			UoW.Commit();
		}
	}

	public class BottleDebtorsVMNode
	{
		[UseForSearch]
		public int AddressId { get; set; }

		[UseForSearch]
		public string AddressName { get; set; }

		public int ClientId { get; set; }

		[UseForSearch]
		public string ClientName { get; set; }

		public PersonType OPF { get; set; }

		public DateTime? LastOrderDate{ get; set; }

		public int DebtByAddress { get; set; }

		public int DebtByClient{ get; set; }

		public int Reserve { get; set; }

		public string RowColor { get { return IsTaskExist ? "grey" : "black"; } }

		public bool IsResidue { get { return Residue != null; } }

		public int? Residue { get; set; } //FIXME : костыль для проверки на наличие ввода остатков (заменить на IF NOT EXIST)

		public bool IsTaskExist { get { return ExistTask != null; } }

		public int? ExistTask { get; set; } //FIXME : костыль для проверки на наличие созданной задачи (заменить на IF NOT EXIST)
	}
}
