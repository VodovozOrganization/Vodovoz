using Core.Infrastructure;
using Gamma.Utilities;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.ViewModels.Dialog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using TrueMark.Contracts;
using TrueMark.Contracts.Responses;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Interfaces.TrueMark;
using Vodovoz.Models.TrueMark;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Edo;
using Vodovoz.ViewModels.Journals.FilterViewModels.Edo;
using Vodovoz.ViewModels.Journals.JournalViewModels.Edo;
using VodovozBusiness.Services.TrueMark;

namespace Vodovoz.ViewModels.TrueMark
{
	/// <summary>
	/// Диалог поиска и просмотра истории кода маркировки.
	/// </summary>
	public class TrueMarkCodeHistoryViewModel : DialogViewModelBase
	{
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly ITrueMarkCodeInfoService _trueMarkCodeInfoService;
		private readonly TrueMarkWaterCodeParser _trueMarkWaterCodeParser;
		private readonly ITrueMarkCodeHistoryRepository _trueMarkCodeHistoryRepository;
		private readonly IGuiDispatcher _guiDispatcher;
		private readonly IInteractiveService _interactiveService;
		private readonly IGtkTabsOpener _gtkTabsOpener;
		private readonly IClipboard _clipboard;

		private string _searchText;
		private bool? _isValidSearchCodeText;
		private string _parsedSearchCodeSerialNumber;
		private string _statusMessage;
		private bool _isLoading;
		private ITrueMarkWaterCode _currentCode;
		private string _currentRawCode;
		private IEnumerable<TrueMarkCodeInfoNode> _trueMarkInfoNodes = Enumerable.Empty<TrueMarkCodeInfoNode>();
		private IEnumerable<TrueMarkCodeHistoryNode> _historyNodes = Enumerable.Empty<TrueMarkCodeHistoryNode>();
		private TrueMarkCodeHistoryNode _selectedHistoryNode;
		private TrueMarkCodeInfoNode _selectedTrueMarkInfoNode;

		public TrueMarkCodeHistoryViewModel(
			INavigationManager navigation,
			IUnitOfWorkFactory uowFactory,
			ITrueMarkCodeInfoService trueMarkCodeInfoService,
			TrueMarkWaterCodeParser trueMarkWaterCodeParser,
			ITrueMarkCodeHistoryRepository trueMarkCodeHistoryRepository,
			IGuiDispatcher guiDispatcher,
			IInteractiveService interactiveService,
			IGtkTabsOpener gtkTabsOpener,
			IClipboard clipboard)
			: base(navigation)
		{
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_trueMarkCodeInfoService = trueMarkCodeInfoService ?? throw new ArgumentNullException(nameof(trueMarkCodeInfoService));
			_trueMarkWaterCodeParser = trueMarkWaterCodeParser ?? throw new ArgumentNullException(nameof(trueMarkWaterCodeParser));
			_trueMarkCodeHistoryRepository = trueMarkCodeHistoryRepository
				?? throw new ArgumentNullException(nameof(trueMarkCodeHistoryRepository));
			_guiDispatcher = guiDispatcher ?? throw new ArgumentNullException(nameof(guiDispatcher));
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			_gtkTabsOpener = gtkTabsOpener ?? throw new ArgumentNullException(nameof(gtkTabsOpener));
			_clipboard = clipboard ?? throw new ArgumentNullException(nameof(clipboard));

			Title = "Поиск и история кода маркировки";
			StatusMessage = "Введите код маркировки для поиска";

			SearchCommand = new AsyncCommand(_guiDispatcher, SearchAsync);
			CreateNavigationCommands();
		}

		/// <summary>
		/// Команда поиска кода маркировки.
		/// </summary>
		public AsyncCommand SearchCommand { get; }

		public ICommand OpenOrderCommand { get; private set; }
		public ICommand OpenOrderCodesCommand { get; private set; }
		public ICommand OpenEdoDialogCommand { get; private set; }
		public ICommand OpenEdoProcessCommand { get; private set; }
		public ICommand OpenSourceDocumentCommand { get; private set; }
		public ICommand CopyCodeCommand { get; private set; }
		public ICommand CopyTrueMarkInfoCodeCommand { get; private set; }
		public ICommand CopyTrueMarkInfoChildCodesCommand { get; private set; }
		public ICommand CopyHistoryChildCodesCommand { get; private set; }

		/// <summary>
		/// Текст поиска кода маркировки.
		/// </summary>
		public virtual string SearchText
		{
			get => _searchText;
			set
			{
				if(SetField(ref _searchText, value))
				{
					ValidateSearchCode();
				}
			}
		}

		/// <summary>
		/// Результат валидации введённого кода: null - код не введён, true - код корректен, false - код некорректен.
		/// </summary>
		public virtual bool? IsValidSearchCodeText
		{
			get => _isValidSearchCodeText;
			private set => SetField(ref _isValidSearchCodeText, value);
		}

		/// <summary>
		/// Серийный номер кода для подсветки поиска.
		/// </summary>
		public virtual string ParsedSearchCodeSerialNumber
		{
			get => _parsedSearchCodeSerialNumber;
			private set => SetField(ref _parsedSearchCodeSerialNumber, value);
		}

		/// <summary>
		/// Текущая информация из ЧЗ.
		/// </summary>
		public virtual IEnumerable<TrueMarkCodeInfoNode> TrueMarkInfoNodes
		{
			get => _trueMarkInfoNodes;
			private set => SetField(ref _trueMarkInfoNodes, value);
		}

		/// <summary>
		/// История кода в ДВ.
		/// </summary>
		public virtual IEnumerable<TrueMarkCodeHistoryNode> HistoryNodes
		{
			get => _historyNodes;
			private set => SetField(ref _historyNodes, value);
		}

		/// <summary>
		/// Выбранный узел истории.
		/// </summary>
		public virtual TrueMarkCodeHistoryNode SelectedHistoryNode
		{
			get => _selectedHistoryNode;
			set => SetField(ref _selectedHistoryNode, value);
		}

		/// <summary>
		/// Выбранный узел текущей информации из ЧЗ.
		/// </summary>
		public virtual TrueMarkCodeInfoNode SelectedTrueMarkInfoNode
		{
			get => _selectedTrueMarkInfoNode;
			set => SetField(ref _selectedTrueMarkInfoNode, value);
		}

		/// <summary>
		/// Сообщение о состоянии поиска.
		/// </summary>
		public virtual string StatusMessage
		{
			get => _statusMessage;
			private set => SetField(ref _statusMessage, value);
		}

		/// <summary>
		/// Выполняется загрузка данных.
		/// </summary>
		public virtual bool IsLoading
		{
			get => _isLoading;
			private set => SetField(ref _isLoading, value);
		}

		private void CreateNavigationCommands()
		{
			var openOrderCommand = new DelegateCommand(
				OpenOrder,
				() => SelectedHistoryNode?.OrderId.HasValue == true);
			openOrderCommand.CanExecuteChangedWith(this, x => x.SelectedHistoryNode);
			OpenOrderCommand = openOrderCommand;

			var openOrderCodesCommand = new DelegateCommand(
				OpenOrderCodes,
				() => SelectedHistoryNode?.OrderId.HasValue == true);
			openOrderCodesCommand.CanExecuteChangedWith(this, x => x.SelectedHistoryNode);
			OpenOrderCodesCommand = openOrderCodesCommand;

			var openEdoDialogCommand = new DelegateCommand(
				OpenEdoDialog,
				() => SelectedHistoryNode?.OrderId.HasValue == true);
			openEdoDialogCommand.CanExecuteChangedWith(this, x => x.SelectedHistoryNode);
			OpenEdoDialogCommand = openEdoDialogCommand;

			var openEdoProcessCommand = new DelegateCommand(
				OpenEdoProcess,
				() => SelectedHistoryNode?.OrderId.HasValue == true);
			openEdoProcessCommand.CanExecuteChangedWith(this, x => x.SelectedHistoryNode);
			OpenEdoProcessCommand = openEdoProcessCommand;

			var openSourceDocumentCommand = new DelegateCommand(
				OpenSourceDocument,
				() => SelectedHistoryNode?.SourceDocumentId.HasValue == true
					&& SelectedHistoryNode.SourceDocumentType != TrueMarkCodeHistoryProductCodeDocumentType.None);
			openSourceDocumentCommand.CanExecuteChangedWith(this, x => x.SelectedHistoryNode);
			OpenSourceDocumentCommand = openSourceDocumentCommand;

			var copyCodeCommand = new DelegateCommand(
				CopyCode,
				() => !string.IsNullOrWhiteSpace(SelectedHistoryNode?.Code));
			copyCodeCommand.CanExecuteChangedWith(this, x => x.SelectedHistoryNode);
			CopyCodeCommand = copyCodeCommand;

			var copyInfoCodeCommand = new DelegateCommand(
				CopyInfoCode,
				() => !string.IsNullOrWhiteSpace(SelectedTrueMarkInfoNode?.Code));
			copyInfoCodeCommand.CanExecuteChangedWith(this, x => x.SelectedTrueMarkInfoNode);
			CopyTrueMarkInfoCodeCommand = copyInfoCodeCommand;

			var copyInfoChildCodesCommand = new DelegateCommand(
				CopyInfoChildCodes,
				() => GetChildCodes(SelectedTrueMarkInfoNode).Any());
			copyInfoChildCodesCommand.CanExecuteChangedWith(this, x => x.SelectedTrueMarkInfoNode);
			CopyTrueMarkInfoChildCodesCommand = copyInfoChildCodesCommand;

			var copyHistoryChildCodesCommand = new DelegateCommand(
				CopyHistoryChildCodes,
				() => GetChildCodes(SelectedHistoryNode).Any());
			copyHistoryChildCodesCommand.CanExecuteChangedWith(this, x => x.SelectedHistoryNode);
			CopyHistoryChildCodesCommand = copyHistoryChildCodesCommand;
		}

		private void ValidateSearchCode()
		{
			if(_searchText.IsNullOrWhiteSpace())
			{
				_currentCode = null;
				_currentRawCode = null;
				ParsedSearchCodeSerialNumber = null;
				IsValidSearchCodeText = null;
				StatusMessage = "Введите код маркировки для поиска";
				TrueMarkInfoNodes = Enumerable.Empty<TrueMarkCodeInfoNode>();
				SetHistoryNodes(Enumerable.Empty<TrueMarkCodeHistoryNode>());
				return;
			}

			if(_trueMarkWaterCodeParser.FuzzyParse(_searchText, out var parsedCode))
			{
				_currentCode = parsedCode;
				_currentRawCode = null;
				ParsedSearchCodeSerialNumber = parsedCode.SerialNumber;
				IsValidSearchCodeText = true;
				StatusMessage = "Код корректен";
				return;
			}

			if(_trueMarkWaterCodeParser.IsTransportCode(_searchText))
			{
				_currentCode = null;
				_currentRawCode = _searchText;
				ParsedSearchCodeSerialNumber = _searchText;
				IsValidSearchCodeText = true;
				StatusMessage = "Транспортный код корректен";
				return;
			}

			_currentCode = null;
			_currentRawCode = null;
			ParsedSearchCodeSerialNumber = null;
			IsValidSearchCodeText = false;
			StatusMessage = "Введённый код имеет некорректный формат";
		}

		private async Task SearchAsync(CancellationToken cancellationToken)
		{
			var code = _currentCode;
			var rawCode = _currentRawCode;

			if((code is null || string.IsNullOrWhiteSpace(code.SerialNumber))
				&& string.IsNullOrWhiteSpace(rawCode))
			{
				return;
			}

			_guiDispatcher.RunInGuiTread(() =>
			{
				IsLoading = true;
				StatusMessage = "Загрузка данных...";
			});

			try
			{
				var trueMarkInfoTask = code is null
					? GetTrueMarkInfoByRawCodeAsync(rawCode, cancellationToken)
					: GetTrueMarkInfoAsync(code, cancellationToken);

				TrueMarkCodeHistoryData historyData;

				using(var uow = _uowFactory.CreateWithoutRoot("История кода маркировки"))
				{
					historyData = _trueMarkCodeHistoryRepository.GetCodeHistory(
						uow,
						code?.Gtin,
						code?.SerialNumber,
						rawCode);
				}

				var trueMarkInfoResult = await trueMarkInfoTask;

				_guiDispatcher.RunInGuiTread(() =>
				{
					try
					{
						TrueMarkInfoNodes = BuildTrueMarkInfoNodes(trueMarkInfoResult);
						SetHistoryNodes(BuildHistoryNodes(historyData));
						StatusMessage = BuildHistoryStatusMessage(historyData);
						IsLoading = false;
					}
					catch(Exception ex)
					{
						HandleException(ex, "Обновление дерева истории кода");
					}
				});
			}
			catch(Exception ex)
			{
				_guiDispatcher.RunInGuiTread(() =>
				{
					TrueMarkInfoNodes = new[]
					{
						new TrueMarkCodeInfoNode { Name = "Ошибка", Value = ex.Message }
					};
					SetHistoryNodes(Enumerable.Empty<TrueMarkCodeHistoryNode>());
					HandleException(ex, "Поиск кода маркировки");
				});
			}
		}

		private void HandleException(Exception exception, string context)
		{
			var errorText = $"{context}:{Environment.NewLine}{exception}";

			try
			{
				var logPath = Path.Combine(Path.GetTempPath(), "TrueMarkCodeHistory_error.txt");
				File.AppendAllText(
					logPath,
					$"{DateTime.Now:yyyy-MM-dd HH:mm:ss}{Environment.NewLine}{errorText}{Environment.NewLine}{Environment.NewLine}");
			}
			catch
			{
				// Логирование не должно скрывать исходную ошибку.
			}

			StatusMessage = $"Ошибка: {exception.Message}";
			IsLoading = false;
			_interactiveService.ShowMessage(
				ImportanceLevel.Error,
				errorText,
				"Ошибка истории кода маркировки");
		}

		private void SetHistoryNodes(IEnumerable<TrueMarkCodeHistoryNode> historyNodes)
		{
			HistoryNodes = historyNodes ?? Enumerable.Empty<TrueMarkCodeHistoryNode>();
		}

		private static string BuildHistoryStatusMessage(TrueMarkCodeHistoryData historyData)
		{
			if(historyData is null)
			{
				return "История кода не найдена";
			}

			if(historyData.Codes.Any())
			{
				return $"Найдено экземплярных кодов: {historyData.Codes.Count}; агрегаций: {historyData.Aggregations.Count}";
			}

			if(historyData.Aggregations.Any())
			{
				return $"Найдено агрегаций: {historyData.Aggregations.Count}";
			}

			return "В ДВ информация по коду не найдена";
		}

		private async Task<TrueMarkApiInfoResult> GetTrueMarkInfoByRawCodeAsync(
			string rawCode,
			CancellationToken cancellationToken)
		{
			if(string.IsNullOrWhiteSpace(rawCode))
			{
				return new TrueMarkApiInfoResult
				{
					ErrorMessage = "Код маркировки не задан"
				};
			}

			try
			{
				var response = await _trueMarkCodeInfoService.GetProductInstancesInfoWithChildrenAsync(
					new[] { rawCode },
					cancellationToken);

				return new TrueMarkApiInfoResult
				{
					Response = response
				};
			}
			catch(Exception ex)
			{
				return new TrueMarkApiInfoResult
				{
					ErrorMessage = ex.Message
				};
			}
		}

		private async Task<TrueMarkApiInfoResult> GetTrueMarkInfoAsync(
			ITrueMarkWaterCode code,
			CancellationToken cancellationToken)
		{
			if(string.IsNullOrWhiteSpace(code.Gtin))
			{
				return new TrueMarkApiInfoResult
				{
					ErrorMessage = "Для запроса в ЧЗ укажите полный код маркировки (с GTIN)"
				};
			}

			var identificationCode = $"01{code.Gtin}21{code.SerialNumber}";

			try
			{
				var response = await _trueMarkCodeInfoService.GetProductInstancesInfoWithChildrenAsync(
					new[] { identificationCode },
					cancellationToken);

				return new TrueMarkApiInfoResult
				{
					Response = response
				};
			}
			catch(Exception ex)
			{
				return new TrueMarkApiInfoResult
				{
					ErrorMessage = ex.Message
				};
			}
		}

		private IEnumerable<TrueMarkCodeInfoNode> BuildTrueMarkInfoNodes(TrueMarkApiInfoResult result)
		{
			if(result is null)
			{
				return new[]
				{
					new TrueMarkCodeInfoNode { Name = "Ошибка", Value = "Не удалось получить информацию из ЧЗ" }
				};
			}

			var rootNodes = new List<TrueMarkCodeInfoNode>();

			if(!string.IsNullOrWhiteSpace(result.ErrorMessage))
			{
				rootNodes.Add(new TrueMarkCodeInfoNode
				{
					Name = "Ошибка",
					Value = result.ErrorMessage
				});
			}

			var response = result.Response;

			if(response is null)
			{
				if(rootNodes.Any())
				{
					return rootNodes;
				}

				rootNodes.Add(new TrueMarkCodeInfoNode
				{
					Name = "Ошибка",
					Value = "ЧЗ вернул пустой ответ"
				});

				return rootNodes;
			}

			if(response.NoCodesFound)
			{
				rootNodes.Add(new TrueMarkCodeInfoNode
				{
					Name = "Информация из ЧЗ",
					Value = "В ЧЗ нет информации по переданному коду"
				});
			}

			if(!string.IsNullOrWhiteSpace(response.ErrorMessage))
			{
				rootNodes.Add(new TrueMarkCodeInfoNode
				{
					Name = "Ошибка ЧЗ",
					Value = response.ErrorMessage
				});
			}

			var statuses = response.InstanceStatuses?.ToList() ?? new List<ProductInstanceStatus>();

			if(!statuses.Any())
			{
				if(!rootNodes.Any())
				{
					rootNodes.Add(new TrueMarkCodeInfoNode
					{
						Name = "Информация из ЧЗ",
						Value = "Данные отсутствуют"
					});
				}

				return rootNodes;
			}

			var nodesByCode = statuses
				.Where(x => !string.IsNullOrWhiteSpace(x.IdentificationCode))
				.GroupBy(x => x.IdentificationCode)
				.ToDictionary(x => x.Key, x => CreateInfoNode(x.First()));

			var placeholderCodes = new HashSet<string>();
			var addedNodes = new HashSet<TrueMarkCodeInfoNode>();

			foreach(var status in statuses)
			{
				if(string.IsNullOrWhiteSpace(status.IdentificationCode)
					|| !nodesByCode.TryGetValue(status.IdentificationCode, out var node)
					|| !addedNodes.Add(node))
				{
					continue;
				}

				if(!string.IsNullOrWhiteSpace(status.ParentId)
					&& nodesByCode.TryGetValue(status.ParentId, out var parentNode)
					&& !ReferenceEquals(parentNode, node))
				{
					node.Parent = parentNode;
					parentNode.Children.Add(node);
				}
				else
				{
					rootNodes.Add(node);
				}
			}

			foreach(var status in statuses)
			{
				if(string.IsNullOrWhiteSpace(status.IdentificationCode)
					|| !nodesByCode.TryGetValue(status.IdentificationCode, out var node))
				{
					continue;
				}

				foreach(var childCode in status.Childs ?? Enumerable.Empty<string>())
				{
					if(string.IsNullOrWhiteSpace(childCode))
					{
						continue;
					}

					if(nodesByCode.TryGetValue(childCode, out var childNode)
						&& !ReferenceEquals(childNode, node)
						&& childNode.Parent is null)
					{
						rootNodes.Remove(childNode);
						childNode.Parent = node;
						node.Children.Add(childNode);
						continue;
					}

					if(nodesByCode.ContainsKey(childCode) || !placeholderCodes.Add(childCode))
					{
						continue;
					}

					var placeholderNode = new TrueMarkCodeInfoNode
					{
						Parent = node,
						Name = childCode,
						Value = "Информация не получена",
						Code = childCode
					};

					node.Children.Add(placeholderNode);
				}
			}

			return rootNodes;
		}

		private TrueMarkCodeInfoNode CreateInfoNode(ProductInstanceStatus status)
		{
			var node = new TrueMarkCodeInfoNode
			{
				Name = status.IdentificationCode,
				Value = GetInstanceStatusTitle(status.Status),
				Code = status.IdentificationCode
			};

			AddInfoChild(node, "Статус", GetInstanceStatusTitle(status.Status));
			AddInfoChild(node, "Владелец", status.OwnerName);
			AddInfoChild(node, "ИНН владельца", status.OwnerInn);
			AddInfoChild(node, "GTIN", status.Gtin);
			AddInfoChild(node, "Тип упаковки", GetGeneralPackageTypeTitle(status.GeneralPackageType));
			AddInfoChild(node, "Уровень упаковки", GetPackageTypeTitle(status.PackageType));
			AddInfoChild(node, "Дата производства", status.ProducedDate?.ToString("dd.MM.yyyy"));
			AddInfoChild(node, "Срок годности", status.ExpirationDate?.ToString("dd.MM.yyyy"));
			AddInfoChild(node, "Родительский код", status.ParentId);
			AddInfoChild(node, "Дочерние коды", status.Childs?.Any() == true
				? string.Join(", ", status.Childs)
				: null);

			return node;
		}

		private static void AddInfoChild(TrueMarkCodeInfoNode parent, string name, string value)
		{
			if(string.IsNullOrWhiteSpace(value))
			{
				return;
			}

			var child = new TrueMarkCodeInfoNode
			{
				Parent = parent,
				Name = name,
				Value = value
			};

			parent.Children.Add(child);
		}

		private IEnumerable<TrueMarkCodeHistoryNode> BuildHistoryNodes(TrueMarkCodeHistoryData data)
		{
			if(data is null)
			{
				return new[]
				{
					new TrueMarkCodeHistoryNode
					{
						Title = "История кода в ДВ",
						Details = "Данные не найдены",
						NodeType = TrueMarkCodeHistoryNodeType.Group
					}
				};
			}

			var result = new List<TrueMarkCodeHistoryNode>();

			result.Add(BuildProductCodesHistoryNode(data));
			result.Add(BuildAggregationsHistoryNode(data));
			result.Add(BuildTasksHistoryNode(data));
			result.Add(BuildOrdersHistoryNode(data));
			result.Add(BuildDocumentFlowsHistoryNode(data));

			return result;
		}

		private TrueMarkCodeHistoryNode BuildProductCodesHistoryNode(TrueMarkCodeHistoryData data)
		{
			var root = new TrueMarkCodeHistoryNode
			{
				Title = "Коды товара ЧЗ (TrueMarkProductCode)",
				Details = $"{data.ProductCodes.Count}",
				NodeType = TrueMarkCodeHistoryNodeType.Group
			};

			if(!data.ProductCodes.Any())
			{
				AddNoDataChild(root);
				return root;
			}

			var addedProductCodeIds = new HashSet<int>();

			foreach(var productCode in data.ProductCodes)
			{
				if(!addedProductCodeIds.Add(productCode.Id))
				{
					continue;
				}

				var node = new TrueMarkCodeHistoryNode
				{
					Parent = root,
					Title = $"Код товара #{productCode.Id}",
					Details = $"{productCode.Type}; статус: {productCode.Status.GetEnumTitle()}; проблема: {productCode.Problem.GetEnumTitle()}",
					Date = productCode.LastModified?.ToString("dd.MM.yyyy HH:mm"),
					NodeType = TrueMarkCodeHistoryNodeType.ProductCode,
					OrderId = productCode.OrderId,
					TaskId = productCode.TaskId
				};

				root.Children.Add(node);

				if(!string.IsNullOrWhiteSpace(productCode.SourceCode))
				{
					AddHistoryChild(
						node,
						"Исходный код",
						productCode.SourceCode,
						null,
						TrueMarkCodeHistoryNodeType.Code,
						productCode.SourceCode);
				}

				if(!string.IsNullOrWhiteSpace(productCode.ResultCode))
				{
					AddHistoryChild(
						node,
						"Итоговый код",
						productCode.ResultCode,
						null,
						TrueMarkCodeHistoryNodeType.Code,
						productCode.ResultCode);
				}

				if(productCode.OrderId.HasValue)
				{
					AddHistoryChild(
						node,
						$"Заказ #{productCode.OrderId.Value}",
						"Перейти в заказ",
						null,
						TrueMarkCodeHistoryNodeType.Order,
						null,
						productCode.OrderId);
				}

				if(productCode.TaskId.HasValue)
				{
					var taskOrderId = data.Tasks
						.FirstOrDefault(x => x.Id == productCode.TaskId.Value)
						?.OrderId
						?? productCode.OrderId;

					var taskNode = AddHistoryChild(
						node,
						$"Задача ЭДО #{productCode.TaskId.Value}",
						"Перейти в задачу",
						null,
						TrueMarkCodeHistoryNodeType.Task,
						null,
						taskOrderId);

					taskNode.TaskId = productCode.TaskId;
				}

				if(productCode.SourceDocumentId.HasValue
					&& productCode.SourceDocumentType != TrueMarkCodeHistoryProductCodeDocumentType.None)
				{
					var sourceDocumentNode = new TrueMarkCodeHistoryNode
					{
						Parent = node,
						Title = GetSourceDocumentTitle(productCode.SourceDocumentType, productCode.SourceDocumentId.Value),
						Details = "Открыть документ",
						NodeType = TrueMarkCodeHistoryNodeType.SourceDocument,
						SourceDocumentId = productCode.SourceDocumentId,
						SourceDocumentType = productCode.SourceDocumentType,
						OrderId = productCode.OrderId
					};

					node.Children.Add(sourceDocumentNode);
				}
			}

			return root;
		}

		private TrueMarkCodeHistoryNode BuildAggregationsHistoryNode(TrueMarkCodeHistoryData data)
		{
			var root = new TrueMarkCodeHistoryNode
			{
				Title = "Агрегации и экземпляры",
				Details = $"{data.Aggregations.Count + data.Codes.Count}",
				NodeType = TrueMarkCodeHistoryNodeType.Group
			};

			if(!data.Aggregations.Any() && !data.Codes.Any())
			{
				AddNoDataChild(root);
				return root;
			}

			var aggregationNodes = new Dictionary<string, TrueMarkCodeHistoryNode>();

			foreach(var aggregation in data.Aggregations)
			{
				var key = GetAggregationKey(aggregation.Type, aggregation.Id);

				if(!aggregationNodes.ContainsKey(key))
				{
					aggregationNodes.Add(key, new TrueMarkCodeHistoryNode
					{
						Title = $"{aggregation.Type} #{aggregation.Id}",
						Details = GetAggregationDetails(aggregation),
						Code = aggregation.Code,
						NodeType = TrueMarkCodeHistoryNodeType.Aggregation
					});
				}
			}

			var addedAggregationNodes = new HashSet<TrueMarkCodeHistoryNode>();

			foreach(var aggregation in data.Aggregations)
			{
				var key = GetAggregationKey(aggregation.Type, aggregation.Id);

				if(!aggregationNodes.TryGetValue(key, out var node)
					|| !addedAggregationNodes.Add(node))
				{
					continue;
				}

				if(aggregation.ParentId.HasValue
					&& !string.IsNullOrWhiteSpace(aggregation.ParentType)
					&& aggregationNodes.TryGetValue(
						GetAggregationKey(aggregation.ParentType, aggregation.ParentId.Value),
						out var parentNode)
					&& !ReferenceEquals(parentNode, node)
					&& !IsAncestor(node, parentNode))
				{
					node.Parent = parentNode;
					parentNode.Children.Add(node);
				}
				else
				{
					root.Children.Add(node);
				}
			}

			var addedCodeIds = new HashSet<int>();

			foreach(var code in data.Codes)
			{
				if(!addedCodeIds.Add(code.Id))
				{
					continue;
				}

				var codeNode = new TrueMarkCodeHistoryNode
				{
					Title = $"Экземпляр #{code.Id}",
					Details = code.IsInvalid
						? $"{code.IdentificationCode} (невалидный)"
						: code.IdentificationCode,
					Code = code.IdentificationCode,
					NodeType = TrueMarkCodeHistoryNodeType.Code
				};

				var parentKey = code.ParentWaterGroupCodeId.HasValue
					? GetAggregationKey("Групповой код", code.ParentWaterGroupCodeId.Value)
					: code.ParentTransportCodeId.HasValue
						? GetAggregationKey("Транспортный код", code.ParentTransportCodeId.Value)
						: null;

				if(parentKey != null && aggregationNodes.TryGetValue(parentKey, out var parentNode))
				{
					codeNode.Parent = parentNode;
					parentNode.Children.Add(codeNode);
				}
				else
				{
					root.Children.Add(codeNode);
				}
			}

			return root;
		}

		private TrueMarkCodeHistoryNode BuildTasksHistoryNode(TrueMarkCodeHistoryData data)
		{
			var root = new TrueMarkCodeHistoryNode
			{
				Title = "Задачи ЭДО",
				Details = $"{data.Tasks.Count}",
				NodeType = TrueMarkCodeHistoryNodeType.Group
			};

			if(!data.Tasks.Any())
			{
				AddNoDataChild(root);
				return root;
			}

			var addedTaskIds = new HashSet<int>();

			foreach(var task in data.Tasks.OrderByDescending(x => x.CreationTime))
			{
				if(!addedTaskIds.Add(task.Id))
				{
					continue;
				}

				var node = new TrueMarkCodeHistoryNode
				{
					Parent = root,
					Title = $"Задача #{task.Id}",
					Details = $"{task.Type.GetEnumTitle()}; статус: {task.Status.GetEnumTitle()}",
					Date = task.CreationTime.ToString("dd.MM.yyyy HH:mm"),
					NodeType = TrueMarkCodeHistoryNodeType.Task,
					TaskId = task.Id,
					OrderId = task.OrderId
				};

				root.Children.Add(node);

				if(task.StartTime.HasValue)
				{
					AddHistoryChild(node, "Начало", task.StartTime.Value.ToString("dd.MM.yyyy HH:mm"), null,
						TrueMarkCodeHistoryNodeType.Group, null, task.OrderId);
				}

				if(task.EndTime.HasValue)
				{
					AddHistoryChild(node, "Завершение", task.EndTime.Value.ToString("dd.MM.yyyy HH:mm"), null,
						TrueMarkCodeHistoryNodeType.Group, null, task.OrderId);
				}

				if(task.OrderId.HasValue)
				{
					AddHistoryChild(node, $"Заказ #{task.OrderId.Value}", "Перейти в заказ", null,
						TrueMarkCodeHistoryNodeType.Order, null, task.OrderId);
				}
			}

			return root;
		}

		private TrueMarkCodeHistoryNode BuildOrdersHistoryNode(TrueMarkCodeHistoryData data)
		{
			var root = new TrueMarkCodeHistoryNode
			{
				Title = "Заказы",
				Details = $"{data.Orders.Count}",
				NodeType = TrueMarkCodeHistoryNodeType.Group
			};

			if(!data.Orders.Any())
			{
				AddNoDataChild(root);
				return root;
			}

			var addedOrderIds = new HashSet<int>();

			foreach(var order in data.Orders.OrderBy(x => x.Id))
			{
				if(!addedOrderIds.Add(order.Id))
				{
					continue;
				}

				root.Children.Add(new TrueMarkCodeHistoryNode
				{
					Parent = root,
					Title = $"Заказ #{order.Id}",
					Details = $"Статус: {order.OrderStatus.GetEnumTitle()}",
					Date = order.DeliveryDate?.ToString("dd.MM.yyyy"),
					NodeType = TrueMarkCodeHistoryNodeType.Order,
					OrderId = order.Id
				});
			}

			return root;
		}

		private TrueMarkCodeHistoryNode BuildDocumentFlowsHistoryNode(TrueMarkCodeHistoryData data)
		{
			var root = new TrueMarkCodeHistoryNode
			{
				Title = "Документооборот",
				Details = $"{data.DocumentFlows.Count}",
				NodeType = TrueMarkCodeHistoryNodeType.Group
			};

			if(!data.DocumentFlows.Any())
			{
				AddNoDataChild(root);
				return root;
			}

			var addedDocumentFlowIds = new HashSet<int>();

			foreach(var documentFlow in data.DocumentFlows.OrderByDescending(x => x.CreationTime))
			{
				if(!addedDocumentFlowIds.Add(documentFlow.OutgoingDocumentId))
				{
					continue;
				}

				var node = new TrueMarkCodeHistoryNode
				{
					Parent = root,
					Title = $"Документооборот #{documentFlow.OutgoingDocumentId}",
					Details = $"{documentFlow.DocumentType.GetEnumTitle()}; {documentFlow.EdoType}; статус: {documentFlow.Status.GetEnumTitle()}",
					Date = documentFlow.CreationTime.ToString("dd.MM.yyyy HH:mm"),
					NodeType = TrueMarkCodeHistoryNodeType.DocumentFlow,
					OrderId = documentFlow.OrderId,
					TaskId = documentFlow.OrderTaskId
				};

				root.Children.Add(node);

				if(documentFlow.SendTime.HasValue)
				{
					AddHistoryChild(node, "Отправлен", documentFlow.SendTime.Value.ToString("dd.MM.yyyy HH:mm"), null,
						TrueMarkCodeHistoryNodeType.Group, null, documentFlow.OrderId);
				}

				if(documentFlow.AcceptTime.HasValue)
				{
					AddHistoryChild(node, "Принят", documentFlow.AcceptTime.Value.ToString("dd.MM.yyyy HH:mm"), null,
						TrueMarkCodeHistoryNodeType.Group, null, documentFlow.OrderId);
				}

				AddHistoryChild(node, "Состояние расхождения", documentFlow.DiscrepancyState.GetEnumTitle(), null,
					TrueMarkCodeHistoryNodeType.Group, null, documentFlow.OrderId);
				AddHistoryChild(node, "Документооборот Такском", documentFlow.TaxcomDocflowId?.ToString(), null,
					TrueMarkCodeHistoryNodeType.Group, null, documentFlow.OrderId);
				AddHistoryChild(node, "Статус Такском", documentFlow.TaxcomDocflowStatus?.GetEnumTitle(), null,
					TrueMarkCodeHistoryNodeType.Group, null, documentFlow.OrderId);

				if(!string.IsNullOrWhiteSpace(documentFlow.ErrorMessage))
				{
					AddHistoryChild(node, "Ошибка документооборота", documentFlow.ErrorMessage, null,
						TrueMarkCodeHistoryNodeType.Group, null, documentFlow.OrderId);
				}

				if(documentFlow.OrderId.HasValue)
				{
					AddHistoryChild(node, $"Заказ #{documentFlow.OrderId.Value}", "Перейти в заказ", null,
						TrueMarkCodeHistoryNodeType.Order, null, documentFlow.OrderId);
				}
			}

			return root;
		}

		private static void AddNoDataChild(TrueMarkCodeHistoryNode root)
		{
			root.Children.Add(new TrueMarkCodeHistoryNode
			{
				Parent = root,
				Title = "Нет данных",
				NodeType = TrueMarkCodeHistoryNodeType.Group
			});
		}

		private static TrueMarkCodeHistoryNode AddHistoryChild(
			TrueMarkCodeHistoryNode parent,
			string title,
			string details,
			string date,
			TrueMarkCodeHistoryNodeType nodeType,
			string code = null,
			int? orderId = null)
		{
			var node = new TrueMarkCodeHistoryNode
			{
				Parent = parent,
				Title = title,
				Details = details,
				Date = date,
				NodeType = nodeType,
				Code = code,
				OrderId = orderId
			};

			parent.Children.Add(node);

			return node;
		}

		private static string GetAggregationKey(string type, int id)
		{
			return $"{type}|{id}";
		}

		private static bool IsAncestor(TrueMarkCodeHistoryNode possibleAncestor, TrueMarkCodeHistoryNode node)
		{
			var current = node;

			while(current != null)
			{
				if(ReferenceEquals(current, possibleAncestor))
				{
					return true;
				}

				current = current.Parent;
			}

			return false;
		}

		private static string GetAggregationDetails(TrueMarkCodeHistoryAggregation aggregation)
		{
			var details = new List<string>();

			if(!string.IsNullOrWhiteSpace(aggregation.Code))
			{
				details.Add(aggregation.Code);
			}

			if(!string.IsNullOrWhiteSpace(aggregation.Gtin))
			{
				details.Add($"GTIN: {aggregation.Gtin}");
			}

			if(!string.IsNullOrWhiteSpace(aggregation.SerialNumber))
			{
				details.Add($"S/N: {aggregation.SerialNumber}");
			}

			if(aggregation.IsInvalid)
			{
				details.Add("невалидный");
			}

			return string.Join("; ", details);
		}

		private static string GetSourceDocumentTitle(
			TrueMarkCodeHistoryProductCodeDocumentType sourceDocumentType,
			int sourceDocumentId)
		{
			switch(sourceDocumentType)
			{
				case TrueMarkCodeHistoryProductCodeDocumentType.RouteList:
					return $"Маршрутный лист #{sourceDocumentId}";
				case TrueMarkCodeHistoryProductCodeDocumentType.CarLoadDocument:
					return $"Талон погрузки #{sourceDocumentId}";
				case TrueMarkCodeHistoryProductCodeDocumentType.SelfDeliveryDocument:
					return $"Самовывоз #{sourceDocumentId}";
				default:
					return $"Документ #{sourceDocumentId}";
			}
		}

		private static string GetInstanceStatusTitle(ProductInstanceStatusEnum? status)
		{
			if(!status.HasValue)
			{
				return null;
			}

			switch(status.Value)
			{
				case ProductInstanceStatusEnum.Emitted:
					return "Эмитирован. Выпущен";
				case ProductInstanceStatusEnum.Applied:
					return "Эмитирован. Получен";
				case ProductInstanceStatusEnum.AppliedPaid:
					return "Нанесён. Оплачен";
				case ProductInstanceStatusEnum.Introduced:
					return "В обороте";
				case ProductInstanceStatusEnum.WrittenOff:
					return "Списан";
				case ProductInstanceStatusEnum.Retired:
					return "Выбыл";
				case ProductInstanceStatusEnum.Withdrawn:
					return "Выбыл (вывод из оборота)";
				case ProductInstanceStatusEnum.Disaggregation:
					return "Расформирован";
				case ProductInstanceStatusEnum.Disaggregated:
					return "Расформирован";
				case ProductInstanceStatusEnum.AppliedNotPaid:
					return "Нанесён, не оплачен";
				default:
					return status.Value.GetEnumTitle();
			}
		}

		private static string GetGeneralPackageTypeTitle(GeneralPackageType? packageType)
		{
			if(!packageType.HasValue)
			{
				return null;
			}

			switch(packageType.Value)
			{
				case GeneralPackageType.Box:
					return "Транспортная упаковка";
				case GeneralPackageType.Group:
					return "Групповая упаковка";
				case GeneralPackageType.Unit:
					return "Индивидуальная упаковка";
				default:
					return packageType.Value.GetEnumTitle();
			}
		}

		private static string GetPackageTypeTitle(PackageType? packageType)
		{
			if(!packageType.HasValue)
			{
				return null;
			}

			switch(packageType.Value)
			{
				case PackageType.Level1:
					return "Уровень 1";
				case PackageType.Level2:
					return "Уровень 2";
				case PackageType.Level3:
					return "Уровень 3";
				case PackageType.Level4:
					return "Уровень 4";
				case PackageType.Level5:
					return "Уровень 5";
				default:
					return packageType.Value.GetEnumTitle();
			}
		}

		private void OpenOrder()
		{
			if(SelectedHistoryNode?.OrderId is int orderId)
			{
				_gtkTabsOpener.OpenOrderDlgFromViewModelByNavigator(this, orderId);
			}
		}

		private void OpenOrderCodes()
		{
			if(SelectedHistoryNode?.OrderId is int orderId)
			{
				NavigationManager.OpenViewModel<OrderCodesDialogViewModel, int>(
					this,
					orderId,
					OpenPageOptions.IgnoreHash);
			}
		}

		private void OpenEdoDialog()
		{
			if(SelectedHistoryNode?.OrderId is int orderId)
			{
				NavigationManager.OpenViewModel<EdoViewModel, int>(
					this,
					orderId,
					OpenPageOptions.IgnoreHash);
			}
		}

		private void OpenEdoProcess()
		{
			if(!(SelectedHistoryNode?.OrderId is int orderId))
			{
				return;
			}

			var page = NavigationManager.OpenViewModel<EdoProcessJournalViewModel>(
				this,
				OpenPageOptions.IgnoreHash);

			if(page?.ViewModel?.JournalFilter is EdoProcessFilterViewModel filter)
			{
				filter.OrderId = orderId;
			}
		}

		private void OpenSourceDocument()
		{
			var node = SelectedHistoryNode;

			if(!(node?.SourceDocumentId is int documentId))
			{
				return;
			}

			switch(node.SourceDocumentType)
			{
				case TrueMarkCodeHistoryProductCodeDocumentType.RouteList:
					NavigationManager.OpenViewModel<RouteListKeepingViewModel, IEntityUoWBuilder>(
						this,
						EntityUoWBuilder.ForOpen(documentId));
					break;
				case TrueMarkCodeHistoryProductCodeDocumentType.CarLoadDocument:
					_gtkTabsOpener.OpenCarLoadDocumentDlg(documentId);
					break;
				case TrueMarkCodeHistoryProductCodeDocumentType.SelfDeliveryDocument:
					_gtkTabsOpener.OpenSelfDeliveryDocumentDlg(documentId);
					break;
			}
		}

		private void CopyCode()
		{
			if(!string.IsNullOrWhiteSpace(SelectedHistoryNode?.Code))
			{
				_clipboard.SetText(SelectedHistoryNode.Code);
			}
		}

		private void CopyInfoCode()
		{
			if(!string.IsNullOrWhiteSpace(SelectedTrueMarkInfoNode?.Code))
			{
				_clipboard.SetText(SelectedTrueMarkInfoNode.Code);
			}
		}

		private void CopyInfoChildCodes()
		{
			CopyCodes(GetChildCodes(SelectedTrueMarkInfoNode));
		}

		private void CopyHistoryChildCodes()
		{
			CopyCodes(GetChildCodes(SelectedHistoryNode));
		}

		private void CopyCodes(IEnumerable<string> codes)
		{
			var distinctCodes = codes?
				.Where(x => !string.IsNullOrWhiteSpace(x))
				.Distinct()
				.ToList();

			if(distinctCodes is null || !distinctCodes.Any())
			{
				return;
			}

			_clipboard.SetText(string.Join(Environment.NewLine, distinctCodes));
		}

		private static IEnumerable<string> GetChildCodes(TrueMarkCodeInfoNode node)
		{
			if(node is null)
			{
				yield break;
			}

			foreach(var child in node.Children)
			{
				foreach(var code in GetCodes(child, true))
				{
					yield return code;
				}
			}
		}

		private static IEnumerable<string> GetCodes(TrueMarkCodeInfoNode node, bool includeSelf)
		{
			if(node is null)
			{
				yield break;
			}

			if(includeSelf && !string.IsNullOrWhiteSpace(node.Code))
			{
				yield return node.Code;
			}

			foreach(var child in node.Children)
			{
				foreach(var code in GetCodes(child, true))
				{
					yield return code;
				}
			}
		}

		private static IEnumerable<string> GetChildCodes(TrueMarkCodeHistoryNode node)
		{
			if(node is null)
			{
				yield break;
			}

			foreach(var child in node.Children)
			{
				foreach(var code in GetCodes(child, true))
				{
					yield return code;
				}
			}
		}

		private static IEnumerable<string> GetCodes(TrueMarkCodeHistoryNode node, bool includeSelf)
		{
			if(node is null)
			{
				yield break;
			}

			if(includeSelf && !string.IsNullOrWhiteSpace(node.Code))
			{
				yield return node.Code;
			}

			foreach(var child in node.Children)
			{
				foreach(var code in GetCodes(child, true))
				{
					yield return code;
				}
			}
		}

		private sealed class TrueMarkApiInfoResult
		{
			public ProductInstancesInfoResponse Response { get; set; }
			public string ErrorMessage { get; set; }
		}
	}
}
