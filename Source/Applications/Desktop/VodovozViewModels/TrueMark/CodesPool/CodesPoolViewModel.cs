using NHibernate;
using NHibernate.Transform;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.ViewModels;
using System.Collections.Generic;
using System.Linq;

namespace Vodovoz.ViewModels.TrueMark.CodesPool
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

			RefreshCommand = new DelegateCommand(UpdateCodesPoolData);
			LoadCodesToPoolCommand = new DelegateCommand(LoadCodesToPool);

			UpdateCodesPoolData();
		}

		public IEnumerable<CodesPoolDataNode> CodesPoolData
		{
			get => _codesPoolData;
			set => SetField(ref _codesPoolData, value);
		}

		public DelegateCommand RefreshCommand { get; }
		public DelegateCommand LoadCodesToPoolCommand { get; }

		private void UpdateCodesPoolData()
		{
			CodesPoolData = GetCodesPoolData();
		}

		private IList<CodesPoolDataNode> GetCodesPoolData()
		{
			var sql = @"
		SELECT
			g.gtin as Gtin,
			pool.count_in_pool as CountInPool,
			stock.sold_yesterday as SoldYesterday,
			GROUP_CONCAT(DISTINCT n.official_name SEPARATOR '|') as Nomenclatures
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
				.AddScalar("Gtin", NHibernateUtil.String)
				.AddScalar("CountInPool", NHibernateUtil.Int32)
				.AddScalar("SoldYesterday", NHibernateUtil.Int32)
				.AddScalar("Nomenclatures", NHibernateUtil.String)
				.SetResultTransformer(Transformers.AliasToBean<CodesPoolDataNode>())
				.List<CodesPoolDataNode>()
				.ToList();

			return codesData;
		}

		private void LoadCodesToPool()
		{
		}

		public class CodesPoolDataNode
		{
			public string Gtin { get; set; }
			public int CountInPool { get; set; }
			public int SoldYesterday { get; set; }
			public string Nomenclatures { get; set; }
			public bool IsNotEnoughCodes => CountInPool < SoldYesterday;
		}
	}
}
