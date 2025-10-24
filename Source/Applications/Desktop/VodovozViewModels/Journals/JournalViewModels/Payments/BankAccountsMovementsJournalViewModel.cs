using System;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using QS.Banks.Domain;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.DB;
using QS.Project.Journal;
using QS.Project.Journal.DataLoader;
using Vodovoz.Core.Domain.Payments;
using Vodovoz.Domain.Payments;
using Vodovoz.Extensions;
using Vodovoz.Filters.ViewModels;
using Vodovoz.ViewModels.Journals.JournalNodes.Payments;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Payments
{
	public class BankAccountsMovementsJournalViewModel : JournalViewModelBase
	{
		private readonly BankAccountsMovementsJournalFilterViewModel _filterViewModel;

		public BankAccountsMovementsJournalViewModel(
			BankAccountsMovementsJournalFilterViewModel filterViewModel,
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigation) : base(unitOfWorkFactory, interactiveService, navigation)
		{
			_filterViewModel = filterViewModel ?? throw new ArgumentNullException(nameof(filterViewModel));
			Title = "Движения средств по расчетным счетам";

			var dataLoader = new ThreadDataLoader<BankAccountsMovementsJournalNode>(UnitOfWorkFactory);
			dataLoader.AddQuery(GetData);
			DataLoader = dataLoader;

			SearchEnabled = false;
			_filterViewModel.IsShow = true;
			SelectionMode = JournalSelectionMode.None;

			//_filterViewModel.OnFiltered += OnFilterViewModelFiltered;
		}
		
		private IQueryOver<BankAccountMovement> GetData(IUnitOfWork uow)
		{
			BankAccountMovement accountMovementAlias = null;
			BankAccountMovement subAccountMovementAlias = null;
			BankAccountMovementData accountMovementDataAlias = null;
			BankAccountMovementData subAccountMovementDataAlias = null;
			Account accountAlias = null;
			Account paymentOrganizationAccountAlias = null;
			Bank bankAlias = null;
			Payment paymentAlias = null;
			BankAccountsMovementsJournalNode resultAlias = null;
			
			var query = uow.Session.QueryOver(() => accountMovementAlias)
				.Left.JoinAlias(() => accountMovementAlias.BankAccountMovements, () => accountMovementDataAlias)
				.Left.JoinAlias(() => accountMovementAlias.Bank, () => bankAlias)
				.Left.JoinAlias(() => accountMovementAlias.Account, () => accountAlias);

			var accountMovementTypeProjection = Projections.Conditional(
				new[]
				{
					new ConditionalProjectionCase(
						Restrictions.Where(() => accountMovementDataAlias.AccountMovementDataType == BankAccountMovementDataType.InitialBalance),
						Projections.Constant(BankAccountMovementDataType.InitialBalance.GetEnumDisplayName())),
					new ConditionalProjectionCase(
						Restrictions.Where(() => accountMovementDataAlias.AccountMovementDataType == BankAccountMovementDataType.FinalBalance),
						Projections.Constant(BankAccountMovementDataType.FinalBalance.GetEnumDisplayName())),
					new ConditionalProjectionCase(
						Restrictions.Where(() => accountMovementDataAlias.AccountMovementDataType == BankAccountMovementDataType.TotalReceived),
						Projections.Constant(BankAccountMovementDataType.TotalReceived.GetEnumDisplayName())),
					new ConditionalProjectionCase(
						Restrictions.Where(() => accountMovementDataAlias.AccountMovementDataType == BankAccountMovementDataType.TotalWrittenOff),
						Projections.Constant(BankAccountMovementDataType.TotalWrittenOff.GetEnumDisplayName()))
				},
				Projections.Constant("Неизвестный тип")
			);
			
			var income = QueryOver.Of(() => paymentAlias)
				.JoinAlias(() => paymentAlias.OrganizationAccount, () => paymentOrganizationAccountAlias)
				.Where(p => p.RefundedPayment == null && p.RefundPaymentFromOrderId == null)
				.And(p => !p.IsManuallyCreated)
				.And(CustomRestrictions.Between(
					CustomProjections.Date(() => paymentAlias.Date),
					CustomProjections.Date(Projections.Property(() => accountMovementAlias.StartDate)),
					CustomProjections.Date(Projections.Property(() => accountMovementAlias.EndDate))))
				.And(() => paymentOrganizationAccountAlias.Id == accountAlias.Id)
				.Select(Projections.Sum(() => paymentAlias.Total));
			
			var yesterdayFinalBalance = QueryOver.Of(() => subAccountMovementDataAlias)
				.JoinAlias(() => subAccountMovementDataAlias.AccountMovement, () => subAccountMovementAlias)
				.Where(
					Restrictions.EqProperty(
						CustomProjections.Date(Projections.Property(() => subAccountMovementAlias.EndDate)),
						DateProjections.DateSub(
							CustomProjections.Date(Projections.Property(() => accountMovementAlias.StartDate)),
							Projections.Constant(1),
							"day")
					)
				)
				.And(() => subAccountMovementAlias.Id != accountMovementAlias.Id)
				.And(() => subAccountMovementAlias.Account.Id == accountMovementAlias.Account.Id)
				.And(() => subAccountMovementDataAlias.AccountMovementDataType == BankAccountMovementDataType.FinalBalance)
				.Select(Projections.Property(() => subAccountMovementDataAlias.Amount));

			var paymentsSubquery =
				Projections.Conditional(
					new[]
					{
						new ConditionalProjectionCase(
							Restrictions.Where(() => accountMovementDataAlias.AccountMovementDataType == BankAccountMovementDataType.InitialBalance),
							Projections.SubQuery(yesterdayFinalBalance)),
						new ConditionalProjectionCase(
							Restrictions.Where(() => accountMovementDataAlias.AccountMovementDataType == BankAccountMovementDataType.TotalReceived),
							Projections.SubQuery(income))
					},
					Projections.Constant(null, NHibernateUtil.Decimal)
				);

			var startDate = _filterViewModel.StartDate;
			var endDate = _filterViewModel.EndDate;

			if(startDate.HasValue)
			{
				query.Where(x => x.StartDate >= startDate.Value);
			}
			
			if(endDate.HasValue)
			{
				query.Where(x => x.EndDate <= endDate.Value);
			}

			query.SelectList(list => list
					.Select(bam => bam.Id).WithAlias(() => resultAlias.Id)
					.Select(bam => bam.StartDate).WithAlias(() => resultAlias.StartDate)
					.Select(bam => bam.EndDate).WithAlias(() => resultAlias.EndDate)
					.Select(() => accountAlias.Number).WithAlias(() => resultAlias.Account)
					.Select(() => bankAlias.Name).WithAlias(() => resultAlias.Bank)
					.Select(accountMovementTypeProjection).WithAlias(() => resultAlias.Name)
					.Select(() => accountMovementDataAlias.AccountMovementDataType).WithAlias(() => resultAlias.AccountMovementDataType)
					.Select(() => accountMovementDataAlias.Amount).WithAlias(() => resultAlias.Amount)
					.Select(paymentsSubquery).WithAlias(() => resultAlias.AmountFromProgram)
				)
				.TransformUsing(Transformers.AliasToBean<BankAccountsMovementsJournalNode>())
				.OrderBy(x => x.StartDate).Desc();
			
			return query;
		}
	}
}
