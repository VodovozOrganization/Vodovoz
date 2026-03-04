using System;
using System.Linq;
using QS.Commands;
using QS.Navigation;
using QS.ViewModels.Dialog;
using Vodovoz.Domain.Goods;
using VodovozBusiness.Domain.Goods;

namespace Vodovoz.ViewModels.ViewModels.Goods
{
	public class GtinViewModel : DialogViewModelBase
	{
		private readonly Nomenclature _nomenclature;
		private Gtin _gtin;

		public GtinViewModel(INavigationManager navigationManager) : base(navigationManager)
		{
			CloseCommand = new DelegateCommand(CloseEditDialog);
		}
		
		/// <summary>
		/// Добавление Gtin к номенклатуре
		/// </summary>
		/// <param name="navigationManager"></param>
		/// <param name="nomenclature"></param>
		/// <exception cref="ArgumentNullException"></exception>
		public GtinViewModel(INavigationManager navigationManager, Nomenclature nomenclature)
			: this(navigationManager)
		{
			_nomenclature = nomenclature ?? throw new ArgumentNullException(nameof(nomenclature));

			Gtin = new Gtin
			{
				Nomenclature = _nomenclature,
				Priority = 1
			};

			Title = $"Новый Gtin для номенклатуры {nomenclature.Id} {nomenclature.Name}";
		}

		/// <summary>
		/// Редактирование Gtin
		/// </summary>
		/// <param name="navigationManager"></param>
		/// <param name="gtin"></param>
		/// <param name="nomenclature"></param>
		/// <exception cref="ArgumentNullException"></exception>
		public GtinViewModel(INavigationManager navigationManager, Gtin gtin, Nomenclature nomenclature)
			: this(navigationManager)
		{
			Gtin = gtin ?? throw new ArgumentNullException(nameof(gtin));

			_nomenclature = nomenclature;

			Title = $"Редактирование {gtin}";
		}

		public Gtin Gtin
		{
			get => _gtin;
			set => SetField(ref _gtin, value);
		}

		private void CloseEditDialog()
		{
			if(Gtin.Id == 0 && _nomenclature.Gtins.All(x => x.GtinNumber != Gtin.GtinNumber))
			{
				_nomenclature.Gtins.Add(Gtin);
			}

			Close(false, CloseSource.Self);
		}

		public DelegateCommand CloseCommand { get; set; }
	}
}
