using QS.Commands;
using QS.Navigation;
using QS.ViewModels.Dialog;
using System;
using System.Linq;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Domain.Goods;
using VodovozBusiness.Domain.Goods;

namespace Vodovoz.ViewModels.ViewModels.Goods
{
	public class GtinViewModel : DialogViewModelBase
	{
		private Nomenclature _nomenclature;

		public GtinViewModel(INavigationManager navigationManager, Gtin gtin, Nomenclature nomenclature)
			: this(navigationManager)
		{
			Gtin = gtin ?? throw new ArgumentNullException(nameof(gtin));

			_nomenclature = nomenclature;

			Title = $"Редактирование {gtin}";
		}

		public GtinViewModel(INavigationManager navigationManager, Nomenclature nomenclature)
			: this(navigationManager)
		{
			_nomenclature = nomenclature ?? throw new ArgumentNullException(nameof(nomenclature));

			Gtin = new Gtin
			{
				Nomenclature = _nomenclature
			};

			Title = $"Новый Gtin для номенклатуры {nomenclature.Id} {nomenclature.Name}";
		}

		public GtinViewModel(INavigationManager navigationManager) : base(navigationManager)
		{
			CloseCommand = new DelegateCommand(CloseEditDialog);
		}

		private void CloseEditDialog()
		{
			if(Gtin.Id == 0 && !_nomenclature.Gtins.Any(x => x.GtinNumber == Gtin.GtinNumber))
			{
				_nomenclature.Gtins.Add(Gtin);
			}

			Close(false, CloseSource.Self);
		}

		public Gtin Gtin { get; }

		public DelegateCommand CloseCommand { get; set; }
	}
}
