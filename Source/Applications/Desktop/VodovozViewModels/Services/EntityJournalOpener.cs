using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using QS.Dialog;
using QS.Navigation;
using QS.Project.Journal;
using QS.Tdi;
using QS.ViewModels.Dialog;
using Vodovoz.ViewModels.Journals.Mappings;

namespace Vodovoz.ViewModels.Services
{
	/// <summary>
	/// Позволяет открывать через навигатор новые журналы, если настроено соответствующее соответствие
	/// </summary>
	public class EntityJournalOpener
	{
		private readonly IInteractiveService _interactiveService;
		private readonly ITdiCompatibilityNavigation _navigation;
		private readonly EntityToJournalMappings _entityToJournalMappings;

		public EntityJournalOpener(
			IInteractiveService interactiveService,
			ITdiCompatibilityNavigation navigation,
			EntityToJournalMappings entityToJournalMappings
		)
		{
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			_navigation = navigation ?? throw new ArgumentNullException(nameof(navigation));
			_entityToJournalMappings = entityToJournalMappings ?? throw new ArgumentNullException(nameof(entityToJournalMappings));
		}
		
		/// <summary>
		/// Открытие журнала, через главную вкладку <c>ITdiTab</c>
		/// </summary>
		/// <param name="entityType">Тип сущности, журнал которой должен открываться</param>
		/// <param name="parentTab">Вкладка, на которой находится виджет</param>
		/// <param name="openPageOptions">Тип открытия вкладки</param>
		/// <returns></returns>
		/// <exception cref="InvalidOperationException">Ошибка, если метод в навигаторе не найден</exception>
		public IPage OpenJournalViewModelFromTdiTab(
			Type entityType,
			ITdiTab parentTab,
			OpenPageOptions openPageOptions = OpenPageOptions.AsSlave)
		{
			var baseType = typeof(JournalViewModelBase);
			var entityJournalMap = GetEntityJournalMap(entityType);
			const string methodName = "OpenViewModelOnTdi";

			if(entityJournalMap.JournalType.IsSubclassOf(baseType))
			{
				var genericMethods = typeof(ITdiCompatibilityNavigation).GetMethods()
					.Where(x => x.Name == methodName)
					.Where(x => x.IsGenericMethod);
			
				var initialMethod = (
						from genericMethod in genericMethods
						let parameters =
							genericMethod.GetParameters()
						where parameters[0].ParameterType == typeof(ITdiTab)
						      && parameters[1].ParameterType == typeof(OpenPageOptions)
						      && parameters[2].ParameterType == typeof(Action<>).MakeGenericType(genericMethod.GetGenericArguments())
						      && parameters[3].ParameterType == typeof(Action<ContainerBuilder>)
						select genericMethod)
					.FirstOrDefault();

				if(initialMethod is null)
				{
					throw new InvalidOperationException($"Can't find {methodName} on type {entityType.FullName}");
				}
					
				return (IPage)initialMethod.MakeGenericMethod(entityJournalMap.JournalType)
					.Invoke(_navigation, new [] { parentTab, openPageOptions, entityJournalMap.GetJournalConfigureAction(), null });
			}

			_interactiveService.ShowMessage(ImportanceLevel.Error, $"Не настроено открытие журналов не наследуемых от { baseType }");
			return null;
		}
		
		/// <summary>
		/// Открытие журнала, через главную вкладку <c>DialogViewModelBase</c>
		/// </summary>
		/// <param name="entityType">Тип сущности, журнал которой должен открываться</param>
		/// <param name="parentViewModel">Вкладка, на которой находится виджет</param>
		/// <param name="openPageOptions">Тип открытия вкладки</param>
		/// <returns></returns>
		/// <exception cref="InvalidOperationException">Ошибка, если метод в навигаторе не найден</exception>
		public IPage OpenJournalViewModelFromDialogViewModel(
			Type entityType,
			DialogViewModelBase parentViewModel,
			OpenPageOptions openPageOptions = OpenPageOptions.AsSlave)
		{
			var baseType = typeof(JournalViewModelBase);
			var entityJournalMap = GetEntityJournalMap(entityType);
			const string methodName = "OpenViewModel";

			if(entityJournalMap.JournalType.IsSubclassOf(baseType))
			{
				var genericMethods = typeof(INavigationManager).GetMethods()
					.Where(x => x.Name == methodName)
					.Where(x => x.IsGenericMethod);
		
				var initialMethod = (
						from genericMethod in genericMethods
						let parameters =
							genericMethod.GetParameters()
						where parameters[0].ParameterType == typeof(DialogViewModelBase)
						      && parameters[1].ParameterType == typeof(OpenPageOptions)
						      && parameters[2].ParameterType == typeof(Action<>).MakeGenericType(genericMethod.GetGenericArguments())
						      && parameters[3].ParameterType == typeof(Action<ContainerBuilder>)
						select genericMethod)
					.FirstOrDefault();

				if(initialMethod is null)
				{
					throw new InvalidOperationException($"Can't find {methodName} on type {entityType.FullName}");
				}
				
				return (IPage)initialMethod.MakeGenericMethod(entityJournalMap.JournalType)
					.Invoke(_navigation, new [] { parentViewModel, openPageOptions, entityJournalMap.GetJournalConfigureAction(), null });
			}
			
			_interactiveService.ShowMessage(ImportanceLevel.Error, $"Не настроено открытие журналов не наследуемых от { baseType }");
			return null;
		}

		private EntityToJournalMapping GetEntityJournalMap(Type entityType)
		{
			if(_entityToJournalMappings.Journals.TryGetValue(entityType, out var entityToJournalMap))
			{
				return entityToJournalMap;
			}
			
			throw new KeyNotFoundException($"Не зарегистрирован открываемый журнал в {nameof(EntityToJournalMappings)} для {entityType}");
		}
	}
}
