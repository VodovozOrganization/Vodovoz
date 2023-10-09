using QS.DomainModel.UoW;
using QS.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vodovoz.Models;
using Vodovoz.ViewModels.Journals.JournalViewModels.Sale;

namespace Vodovoz.ViewModels.Journals.JournalFactories
{
	public class GeoGroupJournalFactory : IGeoGroupJournalFactory
	{
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly ICommonServices _commonServices;
		private readonly IWarehouseJournalFactory _warehouseJournalFactory;
		private readonly GeoGroupVersionsModel _geoGroupVersionsModel;

		public GeoGroupJournalFactory(
			IUnitOfWorkFactory uowFactory,
			ICommonServices commonServices,
			IWarehouseJournalFactory warehouseJournalFactory,
			GeoGroupVersionsModel geoGroupVersionsModel)
		{
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			_warehouseJournalFactory = warehouseJournalFactory ?? throw new ArgumentNullException(nameof(warehouseJournalFactory));
			_geoGroupVersionsModel = geoGroupVersionsModel ?? throw new ArgumentNullException(nameof(geoGroupVersionsModel));
		}

		public GeoGroupJournalViewModel CreateJournal()
		{
			var journal = new GeoGroupJournalViewModel(_uowFactory, _commonServices, _warehouseJournalFactory, _geoGroupVersionsModel);
			return journal;
		}
	}
}
