using NHibernate.Criterion;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.Project.Journal;
using QS.ViewModels;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Vodovoz.ViewModels.Widgets.Search
{
	public partial class CompositeAlgebraicSearchViewModel : WidgetViewModelBase, IJournalSearch
	{
		private readonly OperandType _defaultOperandType = OperandType.And;
		private readonly IInteractiveService _interactiveService;

		private int _searchEntryShownCount = 1;
		private string _entrySearchText1;
		private string _entrySearchText2;
		private string _entrySearchText3;
		private string _entrySearchText4;
		private OperandType _operand1;
		private OperandType _operand2;
		private OperandType _operand3;
		private string[] _searchValues;
		private string _searchByInfo = string.Empty;

		public event EventHandler OnSearch;

		public CompositeAlgebraicSearchViewModel(IInteractiveService interactiveService)
		{
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));

			AddSearchEntryCommand = new DelegateCommand(AddSearchEntry, () => CanAddSearchEntry);
			AddSearchEntryCommand.CanExecuteChangedWith(this, x => x.CanAddSearchEntry);

			RemoveSearchEntryCommand = new DelegateCommand(RemoveSearchEntry, () => CanRemoveSearchEntry);
			RemoveSearchEntryCommand.CanExecuteChangedWith(this, x => CanRemoveSearchEntry);

			ClearSearchEntriesTextCommand = new DelegateCommand(ClearSearchEntriesText, () => CanClearSearchEntriesText);
			ClearSearchEntriesTextCommand.CanExecuteChangedWith(this, x => x.CanClearSearchEntriesText);

			ShowSearchInformation = new DelegateCommand(ShowInformation);
		}

		#region Properties

		public virtual string[] SearchValues
		{
			get => _searchValues ?? new string[] { };
			set => SetField(ref _searchValues, value);
		}

		public string EntrySearchText1
		{
			get => _entrySearchText1;
			set => SetField(ref _entrySearchText1, value);
		}

		public OperandType Operand1
		{
			get => _operand1;
			set => SetField(ref _operand1, value);
		}

		public string EntrySearchText2
		{
			get => _entrySearchText2;
			set => SetField(ref _entrySearchText2, value);
		}

		public OperandType Operand2
		{
			get => _operand2;
			set => SetField(ref _operand2, value);
		}

		public string EntrySearchText3
		{
			get => _entrySearchText3;
			set => SetField(ref _entrySearchText3, value);
		}

		public OperandType Operand3
		{
			get => _operand3;
			set => SetField(ref _operand3, value);
		}

		public string EntrySearchText4
		{
			get => _entrySearchText4;
			set => SetField(ref _entrySearchText4, value);
		}

		[PropertyChangedAlso(
			nameof(CanAddSearchEntry),
			nameof(CanRemoveSearchEntry))]
		public int SearchEntryShownCount
		{
			get => _searchEntryShownCount;
			set => SetField(ref _searchEntryShownCount, value);
		}

		public bool CanAddSearchEntry => SearchEntryShownCount < 4;

		public bool CanRemoveSearchEntry => SearchEntryShownCount > 1;

		public bool CanClearSearchEntriesText => true;

		public DelegateCommand AddSearchEntryCommand { get; }
		public DelegateCommand RemoveSearchEntryCommand { get; }
		public DelegateCommand ClearSearchEntriesTextCommand { get; }
		public DelegateCommand ShowSearchInformation { get; }

		#endregion Properties

		public ICriterion GetSearchCriterion(params Expression<Func<object>>[] aliasPropertiesExpr)
		{
			if(aliasPropertiesExpr.Any())
			{
				_searchByInfo = string.Join(
					"\n",
					aliasPropertiesExpr
						.Select(ape =>
						{
							var memberInfo = GetMemberInfo(ape).Member;

							var displayAttribute = memberInfo.GetCustomAttribute<DisplayAttribute>();

							return displayAttribute is null ? memberInfo.Name : displayAttribute.Name;
						})
						.Select(name => $"- {name}"));
			}

			UpdateSearchValues();

			return new CompositeAlgebraicSearchCriterion(this)
				.By(aliasPropertiesExpr)
				.Finish();
		}

		private static MemberExpression GetMemberInfo(LambdaExpression method)
		{
			if(method == null)
			{
				throw new ArgumentNullException("method");
			}

			MemberExpression memberExpr = null;

			if(method.Body.NodeType == ExpressionType.Convert)
			{
				memberExpr =
					((UnaryExpression)method.Body).Operand as MemberExpression;
			}
			else if(method.Body.NodeType == ExpressionType.MemberAccess)
			{
				memberExpr = method.Body as MemberExpression;
			}

			if(memberExpr == null)
			{
				throw new ArgumentException("method");
			}

			return memberExpr;
		}

		public void Update()
		{
			UpdateSearchValues();
			OnSearch?.Invoke(this, new EventArgs());
		}

		private void ShowInformation()
		{
			_interactiveService.ShowMessage(
				ImportanceLevel.Info,
				"Интерфейс поиска состоит из следующих элементов:\n" +
				"- Текстовые поля - в них необходимо вводить текст для поиска по опреджеленным полям*\n" +
				"\t Если поля будут пустыми - поиск по ним происходить не будет,\n" +
				"\t так же не будет осуществлен поиск если будет заполнено поле,\n" +
				"\t но предыдущие окажутся пустыми, либо содержащие только пробелы\n" +
				"- Выпадающие списки операций - в них необходимо выбрать тип операции - \"И\" или \"Или\"\n" +
				"\t Операции применяются в порядке приоритета: сначала \"И\", затем \"Или\"\n" +
				"- Кнопки\n" +
				"\t - Добавить поле поиска - добавляет выпадающий список и поле за ним, если это возможно\n" +
				"\t - Убрать поле поиска - убирает последнее поле и выпадающий список перед ним\n" +
				"\t - Очистить текстовые поля поиска - очищает все текстовые поля\n" +
				"\t - Информация о поиске - открывает это окно\n" +
				"\n" +
				"* Поля ко которым производится поиск определяются журналом\n" +
				"\n" +
				"В данном журнале поиск производится по следующим параметрам:\n" +
				$"{_searchByInfo}",
				"Информация о поиске");
		}

		private void UpdateSearchValues()
		{
			SearchValues = new string[] { EntrySearchText1, EntrySearchText2, EntrySearchText3, EntrySearchText4 }
				.Where(x => !string.IsNullOrEmpty(x))
				.ToArray();
		}

		private void AddSearchEntry()
		{
			SearchEntryShownCount++;
			switch(SearchEntryShownCount)
			{
				case 2:
					Operand1 = _defaultOperandType;
					break;
				case 3:
					Operand2 = _defaultOperandType;
					break;
				case 4:
					Operand3 = _defaultOperandType;
					break;
			}
		}

		private void RemoveSearchEntry()
		{
			SearchEntryShownCount--;
			switch(SearchEntryShownCount)
			{
				case 1:
					Operand1 = OperandType.Disabled;
					break;
				case 2:
					Operand2 = OperandType.Disabled;
					break;
				case 3:
					Operand3 = OperandType.Disabled;
					break;
			}
		}

		private void ClearSearchEntriesText()
		{
			EntrySearchText1 = string.Empty;
			EntrySearchText2 = string.Empty;
			EntrySearchText3 = string.Empty;
			EntrySearchText4 = string.Empty;

			Update();
		}
	}
}
