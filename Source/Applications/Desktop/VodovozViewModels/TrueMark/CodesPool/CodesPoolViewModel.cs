using NHibernate;
using NHibernate.Transform;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Services.FileDialog;
using QS.ViewModels;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Vodovoz.Models.TrueMark;

namespace Vodovoz.ViewModels.TrueMark.CodesPool
{
	public class CodesPoolViewModel : DialogTabViewModelBase
	{
		private IEnumerable<CodesPoolDataNode> _codesPoolData = new List<CodesPoolDataNode>();
		private readonly IInteractiveService _interactiveService;
		private readonly IFileDialogService _fileDialogService;
		private readonly TrueMarkCodePoolLoader _codePoolLoader;

		public CodesPoolViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigation,
			IFileDialogService fileDialogService,
			TrueMarkCodePoolLoader codePoolLoader)
			: base(unitOfWorkFactory, interactiveService, navigation)
		{
			_interactiveService = interactiveService ?? throw new System.ArgumentNullException(nameof(interactiveService));
			_fileDialogService = fileDialogService ?? throw new System.ArgumentNullException(nameof(fileDialogService));
			_codePoolLoader = codePoolLoader ?? throw new System.ArgumentNullException(nameof(codePoolLoader));

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
			var dialogSettings = CreateDialogSettings();

			var result = _fileDialogService.RunOpenFileDialog(dialogSettings);

			if(!result.Successful)
			{
				return;
			}

			try
			{
				var lodingResult = _codePoolLoader.LoadFromFile(result.Path);

				_interactiveService.ShowMessage(ImportanceLevel.Info,
					$"Найдено кодов: {lodingResult.TotalFound}" +
					$"\nЗагружено: {lodingResult.SuccessfulLoaded}" +
					$"\nУже существуют в системе: {lodingResult.TotalFound - lodingResult.SuccessfulLoaded}");
			}
			catch(IOException ex)
			{
				_interactiveService.ShowMessage(ImportanceLevel.Error, ex.Message);
			}
		}

		private DialogSettings CreateDialogSettings()
		{
			var dialogSettings = new DialogSettings
			{
				SelectMultiple = false,
				Title = "Выберите файл содержащий коды"
			};
			dialogSettings.FileFilters.Add(new DialogFileFilter("Файлы содержащие коды", "*.xlsx", "*.mxl", "*.csv", "*.txt"));

			return dialogSettings;
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
