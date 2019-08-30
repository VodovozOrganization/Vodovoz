using System;
using System.Collections.Generic;
using NHibernate;
using NHibernate.Transform;
using QS.DomainModel.Config;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Services;
using Vodovoz.Domain.WageCalculation;
using Vodovoz.JournalNodes;
using Vodovoz.ViewModels.WageCalculation;

namespace Vodovoz.JournalViewModels.WageCalculation
{
	public class WageParametersJournalViewModel : SingleEntityJournalViewModelBase<WageParameter, WageParameterViewModel, WageParameterJournalNode>
	{
		readonly ICommonServices commonServices;

		public WageParametersJournalViewModel(IEntityConfigurationProvider entityConfigurationProvider, ICommonServices commonServices) : base(entityConfigurationProvider, commonServices)
		{
			TabName = "Журнал параметров расчёта зарплат и премий";
			this.commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			SetOrder(
				new Dictionary<Func<WageParameterJournalNode, object>, bool> {
					{ x => x.IsArchive, false },
					{ x => x.WageCalcType, false }
				}
			);

			UpdateOnChanges(typeof(WageParameter));
		}

		protected override Func<WageParameterViewModel> CreateDialogFunction => () => new WageParameterViewModel(
			EntityConstructorParam.ForCreate(),
			commonServices
		);

		protected override Func<WageParameterJournalNode, WageParameterViewModel> OpenDialogFunction => n => new WageParameterViewModel(
			EntityConstructorParam.ForOpen(n.Id),
			commonServices
		);

		protected override Func<IQueryOver<WageParameter>> ItemsSourceQueryFunction => () => {
			WageParameterJournalNode resultAlias = null;

			var query = UoW.Session.QueryOver<WageParameter>();
			query.Where(
				GetSearchCriterion<WageParameter>(
					x => x.Id
				)
			);

			var result = query.SelectList(list => list
									.Select(x => x.Id).WithAlias(() => resultAlias.Id)
									.Select(x => x.WageCalcType).WithAlias(() => resultAlias.WageCalcType)
									.Select(x => x.WageCalcRate).WithAlias(() => resultAlias.WageCalcRate)
									.Select(x => x.QuantityOfFullBottlesToSell).WithAlias(() => resultAlias.QuantityOfFullBottlesToSell)
									.Select(x => x.QuantityOfEmptyBottlesToTake).WithAlias(() => resultAlias.QuantityOfEmptyBottlesToTake)
									.Select(x => x.IsArchive).WithAlias(() => resultAlias.IsArchive)
								)
								.TransformUsing(Transformers.AliasToBean<WageParameterJournalNode>())
								.OrderBy(x => x.WageCalcType).Asc
								.ThenBy(x => x.IsArchive).Asc
								;

			return result;
		};

	}
}
