using Gamma.Binding.Core.RecursiveTreeConfig;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Journal.DataLoader;
using QS.Project.Journal.DataLoader.Hierarchy;
using QS.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.TrueMark;
using Vodovoz.ViewModels.Journals.JournalNodes.Roboats;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Roboats
{
	public class TrueMarkReceiptOrdersRegistryJournalViewModel : JournalViewModelBase
	{
		public TrueMarkReceiptOrdersRegistryJournalViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			INavigationManager navigation = null)
			: base(unitOfWorkFactory, commonServices.InteractiveService, navigation)
		{
			Title = "Реестр заказов для чека";

			var levelDataLoader = new HierarchicalQueryLoader<TrueMarkCashReceiptOrder, TrueMarkReceiptOrderNode>(unitOfWorkFactory);
			
			levelDataLoader.SetLevelingModel(GetQuery)
				.AddNextLevelSource(GetDetails);
			levelDataLoader.SetCountFunction(GetCount);

			RecuresiveConfig = levelDataLoader.TreeConfig;

			var threadDataLoader = new ThreadDataLoader<TrueMarkReceiptOrderNode>(unitOfWorkFactory);
			threadDataLoader.DynamicLoadingEnabled = true;
			threadDataLoader.QueryLoaders.Add(levelDataLoader);
			DataLoader = threadDataLoader;

			CreateNodeActions();
		}

		public IRecursiveConfig RecuresiveConfig { get; }

		void FilterViewModel_OnFiltered(object sender, EventArgs e)
		{
			Refresh();
		}

		public override JournalSelectionMode SelectionMode => JournalSelectionMode.Single;

		protected override void CreateNodeActions()
		{
			NodeActionsList.Clear();
		}

		#region Queries

		private IQueryOver<TrueMarkCashReceiptOrder> GetQuery(IUnitOfWork uow)
		{
			TrueMarkCashReceiptOrder trueMarkOrderAlias = null;
			TrueMarkReceiptOrderNode resultAlias = null;

			var query = uow.Session.QueryOver(() => trueMarkOrderAlias);

			query.Where(
				GetSearchCriterion(
					() => trueMarkOrderAlias.Id,
					() => trueMarkOrderAlias.Order.Id
				)
			);

			query.SelectList(list => list
				.SelectGroup(() => trueMarkOrderAlias.Id).WithAlias(() => resultAlias.Id)
				.Select(Projections.Constant(TrueMarkOrderNodeType.Order)).WithAlias(() => resultAlias.NodeType)
				.Select(Projections.Property(() => trueMarkOrderAlias.Date)).WithAlias(() => resultAlias.Time)
				.Select(Projections.Property(() => trueMarkOrderAlias.Order.Id)).WithAlias(() => resultAlias.OrderAndItemId)
				.Select(Projections.Property(() => trueMarkOrderAlias.Status)).WithAlias(() => resultAlias.OrderStatus)
				.Select(Projections.Property(() => trueMarkOrderAlias.UnscannedCodesReason)).WithAlias(() => resultAlias.UnscannedReason)
				.Select(Projections.Property(() => trueMarkOrderAlias.ErrorDescription)).WithAlias(() => resultAlias.ErrorDescription)
				.Select(Projections.Property(() => trueMarkOrderAlias.CashReceipt.Id)).WithAlias(() => resultAlias.ReceiptId)
			)
			.OrderByAlias(() => trueMarkOrderAlias.Id).Desc()
			.TransformUsing(Transformers.AliasToBean<TrueMarkReceiptOrderNode>());

			return query;
		}

		private IList<TrueMarkReceiptOrderNode> GetDetails(IEnumerable<TrueMarkReceiptOrderNode> parentNodes)
		{
			using(var uow = UnitOfWorkFactory.CreateWithoutRoot())
			{
				TrueMarkCashReceiptProductCode trueMarkReceiptCodeAlias = null;
				TrueMarkWaterIdentificationCode trueMarkSourceCodeAlias = null;
				TrueMarkWaterIdentificationCode trueMarkResultCodeAlias = null;
				TrueMarkReceiptOrderNode resultAlias = null;

				var query = uow.Session.QueryOver(() => trueMarkReceiptCodeAlias)
					.Left.JoinAlias(() => trueMarkReceiptCodeAlias.SourceCode, () => trueMarkSourceCodeAlias)
					.Left.JoinAlias(() => trueMarkReceiptCodeAlias.ResultCode, () => trueMarkResultCodeAlias)
					.Where(Restrictions.In(Projections.Property(() => trueMarkReceiptCodeAlias.TrueMarkCashReceiptOrder.Id), parentNodes.Select(x => x.Id).ToArray()));

				query.SelectList(list => list
					.SelectGroup(() => trueMarkReceiptCodeAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(Projections.Constant(TrueMarkOrderNodeType.Code)).WithAlias(() => resultAlias.NodeType)
					.Select(() => trueMarkReceiptCodeAlias.TrueMarkCashReceiptOrder.Id).WithAlias(() => resultAlias.ParentId)
					.Select(() => trueMarkReceiptCodeAlias.OrderItem.Id).WithAlias(() => resultAlias.OrderAndItemId)
					.Select(() => trueMarkReceiptCodeAlias.IsDefectiveSourceCode).WithAlias(() => resultAlias.IsDefectiveCode)
					.Select(() => trueMarkReceiptCodeAlias.IsDuplicateSourceCode).WithAlias(() => resultAlias.IsDuplicateCode)
					.Select(() => trueMarkSourceCodeAlias.GTIN).WithAlias(() => resultAlias.SourceGtin)
					.Select(() => trueMarkSourceCodeAlias.SerialNumber).WithAlias(() => resultAlias.SourceSerialnumber)
					.Select(() => trueMarkResultCodeAlias.GTIN).WithAlias(() => resultAlias.ResultGtin)
					.Select(() => trueMarkResultCodeAlias.SerialNumber).WithAlias(() => resultAlias.ResultSerialnumber)
				)
				.TransformUsing(Transformers.AliasToBean<TrueMarkReceiptOrderNode>());

				return query.List<TrueMarkReceiptOrderNode>();
			}
		}

		#endregion Queries

		private int GetCount(IUnitOfWork uow)
		{
			var query = GetQuery(uow);
			var count = query.List<TrueMarkReceiptOrderNode>().Count();
			return count;
		}
	}
}
