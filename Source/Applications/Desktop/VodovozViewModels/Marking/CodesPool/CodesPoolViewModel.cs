using NHibernate;
using NHibernate.Transform;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.ViewModels;
using System.Collections.Generic;
using System.Linq;

namespace Vodovoz.ViewModels.Marking.CodesPool
{
	public class CodesPoolViewModel : DialogTabViewModelBase
	{
		private IEnumerable<CodesPoolDataNode> _codesPoolData = new List<CodesPoolDataNode>();

		public CodesPoolViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigation)
			: base(unitOfWorkFactory, interactiveService, navigation)
		{
			Title = "Пул кодов маркировки";

			UpdateCodesPoolData();
		}

		public IEnumerable<CodesPoolDataNode> CodesPoolData
		{
			get => _codesPoolData;
			set => SetField(ref _codesPoolData, value);
		}

		private void UpdateCodesPoolData()
		{
			CodesPoolData = GetCodesPoolData();
		}

		private IList<CodesPoolDataNode> GetCodesPoolData()
		{
			var sql = @"
		SELECT
			g.gtin,
			pool.count_in_pool,
			stock.sold_yesterday,
			GROUP_CONCAT(DISTINCT n.official_name SEPARATOR '|') as nomenclatures
		FROM
			gtins g
		LEFT JOIN
			(
				SELECT
					tmic.gtin,
					COUNT(DISTINCT tmcpn.id) as count_in_pool
				FROM
					true_mark_codes_pool_new tmcpn
				LEFT JOIN true_mark_identification_code tmic ON
					tmic.id = tmcpn.code_id
				WHERE
					tmcpn.code_id = tmcpn.code_id
				GROUP BY
					tmic.gtin
			) pool ON pool.gtin = g.gtin
		LEFT JOIN
			(
				SELECT
					g.gtin,
					SUM(oi.actual_count) as sold_yesterday
				FROM
					orders o
				LEFT JOIN order_items oi ON oi.order_id = o.id
				LEFT JOIN gtins g ON g.nomenclature_id = oi.nomenclature_id
				WHERE
					o.delivery_date = DATE_ADD(CURRENT_DATE(), INTERVAL -1 DAY)
				GROUP BY
					g.gtin
			) stock ON stock.gtin = g.gtin
		LEFT JOIN nomenclature n ON n.id = g.nomenclature_id
		GROUP BY g.gtin";

			var codesData = UoW.Session.CreateSQLQuery(sql)
				.AddScalar("gtin", NHibernateUtil.String)
				.AddScalar("count_in_pool", NHibernateUtil.Int32)
				.AddScalar("sold_yesterday", NHibernateUtil.Int32)
				.AddScalar("nomenclatures", NHibernateUtil.String)
				.SetResultTransformer(Transformers.AliasToBean<CodesPoolDataNode>())
				.List<CodesPoolDataNode>()
				.ToList();

			return codesData;
		}

		public class CodesPoolDataNode
		{
			public string Gtin { get; set; }
			public int CountInPool { get; set; }
			public int SoldYesterdayCount { get; set; }
			public string NomenclatureName { get; set; }
			public bool IsNotEnoughCodes => CountInPool < SoldYesterdayCount;
		}
	}
}
