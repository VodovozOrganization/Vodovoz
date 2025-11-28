using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Services.FileDialog;
using QS.Utilities;
using QS.ViewModels.Dialog;
using Vodovoz.Domain.Payments;
using Vodovoz.EntityRepositories.Payments;
using Vodovoz.Factories;
using Vodovoz.Nodes;

namespace Vodovoz.ViewModels
{
	public class ImportPaymentsFromAvangardSbpViewModel : UowDialogViewModelBase
	{
		#region Help

		private const string _help = "<b>Загрузка реестра оплат</b>:\n" +
									"Для выгрузки данных, необходимо выбрать файл с реестром оплат из Авангарда\n" +
									"и нажать \"Прочитать данные из файла\"\n" +
									"Загружать можно только файлы с расширением .csv\n" +
									"При загрузке идет проверка на дубли, как в уже подгруженных платежах так и тех, что сохранены в БД\n" +
									"Также отрицательные платежи не загружаются из реестра, если таковые имеются\n" +
									"<b>Сохранение данных</b>:\n" +
									"Производится по кнопке \"Загрузить\"";

		#endregion
		
		private const string _noFileSelected = "Файл не выбран";
		private const string _doneProgress = "Готово";
		private const string _errorProgress = "При загрузке реестра произошла ошибка";
		private const string _loadingProgress = "Сохраняем данные";
		private readonly IInteractiveService _interactiveService;
		private readonly IFileDialogService _fileDialogService;
		private readonly IPaymentFromAvangardFactory _paymentFromAvangardFactory;
		private readonly IPaymentsRepository _paymentsRepository;
		private readonly DialogSettings _loadDialogSettings;
		private DelegateCommand _helpCommand;
		private DelegateCommand _chooseFileCommand;
		private DelegateCommand _parsePaymentRegistryCommand;
		private int _countDuplicates;
		private bool _canParsePayments;
		private string _selectedFileTitle;
		private string _selectedFilePath;
		private string _progressTitle;

		public ImportPaymentsFromAvangardSbpViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			INavigationManager navigation,
			IInteractiveService interactiveService,
			IFileDialogService fileDialogService,
			IPaymentFromAvangardFactory paymentFromAvangardFactory,
			IPaymentsRepository paymentsRepository
			) : base(unitOfWorkFactory, navigation)
		{
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			_fileDialogService = fileDialogService ?? throw new ArgumentNullException(nameof(fileDialogService));
			_paymentFromAvangardFactory = paymentFromAvangardFactory ?? throw new ArgumentNullException(nameof(paymentFromAvangardFactory));
			_paymentsRepository = paymentsRepository ?? throw new ArgumentNullException(nameof(paymentsRepository));

			_loadDialogSettings = CreateLoadDialogSettings();
			Title = "Загрузка реестра оплат из Авангарда";
		}

		public IList<PaymentFromAvangard> AvangardPayments { get; } =
			new GenericObservableList<PaymentFromAvangard>();
		
		public string ParsingProgress => "Загружаем данные...";

		public bool IsParsingData { get; set; }
		
		public bool CanParsePayments
		{
			get => _canParsePayments;
			set => SetField(ref _canParsePayments, value);
		}
		
		public string SelectedFileTitle
		{
			get => _selectedFileTitle;
			set => SetField(ref _selectedFileTitle, value);
		}
		
		public string ProgressTitle
		{
			get => _progressTitle;
			set => SetField(ref _progressTitle, value);
		}

		public bool CanLoad => AvangardPayments.Any() && !IsParsingData;
		
		public DelegateCommand HelpCommand => _helpCommand ?? (_helpCommand = new DelegateCommand(
				() =>
				{
					_interactiveService.ShowMessage(ImportanceLevel.Info, _help);
				})
			);
		
		public DelegateCommand ChooseFileCommand => _chooseFileCommand ?? (_chooseFileCommand = new DelegateCommand(
				() =>
				{
					var result = _fileDialogService.RunOpenFileDialog(_loadDialogSettings);

					if(!result.Successful)
					{
						UpdateState(false, _noFileSelected);
						return;
					}

					_selectedFilePath = result.Path;
					if(string.IsNullOrWhiteSpace(_selectedFilePath))
					{
						_interactiveService.ShowMessage(ImportanceLevel.Warning, _noFileSelected);
						UpdateState(false, _noFileSelected);
						return;
					}
				
					UpdateState(true, _selectedFilePath);
				})
			);

		public DelegateCommand ParsePaymentRegistryCommand =>
			_parsePaymentRegistryCommand ?? (_parsePaymentRegistryCommand = new DelegateCommand(
					() =>
					{
						try
						{
							_countDuplicates = default(int);
							CheckForDuplicates(ParseData());
							
							var paymentsSum = AvangardPayments.Sum(p => p.TotalSum);
							ProgressTitle = $"{_doneProgress}. Загружено платежей <b>{AvangardPayments.Count}шт.</b>" +
								$" на сумму: <b>{paymentsSum.ToShortCurrencyString()}</b> " +
								$"Не загружено дублей: <b>{_countDuplicates}шт.</b>";
						}
						catch
						{
							ProgressTitle = _errorProgress;
							throw;
						}
						finally
						{
							IsParsingData = false;
							OnPropertyChanged(nameof(CanLoad));
						}
					}
				)
			);

		public override bool Save()
		{
			ProgressTitle = _loadingProgress;
			foreach(var payment in AvangardPayments)
			{
				UoW.Save(payment);
			}

			UoW.Commit();

			return true;
		}

		private AvangardOperations ParseData()
		{
			var serializer = new XmlSerializer(typeof(AvangardOperations));
			AvangardOperations avangardOperations;

			using(var reader = new StreamReader(_selectedFilePath))
			{
				avangardOperations = (AvangardOperations)serializer.Deserialize(reader);
			}

			return avangardOperations;
		}

		private void CheckForDuplicates(AvangardOperations avangardOperations)
		{
			if(avangardOperations.Operations is null)
			{
				return;
			}
			
			foreach(var node in avangardOperations.Operations)
			{
				if(node.Amount < 0)
				{
					continue;
				}

				var curPayment = AvangardPayments.SingleOrDefault(
					x => x.PaidDate == node.TransDate
						&& x.OrderNum == node.OrderNumber
						&& x.TotalSum == node.Amount);

				if(curPayment != null || _paymentsRepository.PaymentFromAvangardExists(UoW, node.TransDate, node.OrderNumber, node.Amount))
				{
					_countDuplicates++;
					continue;
				}

				AvangardPayments.Add(_paymentFromAvangardFactory.CreateNewPaymentFromAvangard(node));
			}
		}
		
		private DialogSettings CreateLoadDialogSettings()
		{
			var dialogSettings = new DialogSettings
			{
				Title = "Открыть",
			};
			dialogSettings.FileFilters.Add(new DialogFileFilter("XML", "*.xml"));

			return dialogSettings;
		}
		
		private void UpdateState(bool canParsePayments, string title)
		{
			CanParsePayments = canParsePayments;
			SelectedFileTitle = title;
		}
	}
}
