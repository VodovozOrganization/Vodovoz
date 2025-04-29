using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Edo;

namespace Edo.Problems.Custom
{
	public class EdoTaskCustomSourcesPersister
	{
		private readonly IUnitOfWorkFactory _uowFactory;

		private IEnumerable<EdoTaskProblemCustomSource> _persistedSources;
		private IEnumerable<EdoTaskProblemCustomSource> _registeredSources;

		public EdoTaskCustomSourcesPersister(
			IUnitOfWorkFactory uowFactory,
			IEnumerable<EdoTaskProblemCustomSource> customSources
			)
		{
			var sameNameSources = customSources.GroupBy(x => x.Name).Where(x => x.Count() > 1);
			if(sameNameSources.Count() > 0)
			{
				throw new InvalidOperationException($"Объекты описания проблем не могут иметь одинаковые имена: " +
					$"{string.Join(", ", sameNameSources.Select(x => x.Key))}");
			}

			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_registeredSources = customSources;

			Persist();
		}

		private void Persist()
		{
			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				var changed = false;
				var savedSources = uow.Session.QueryOver<EdoTaskProblemCustomSourceEntity>().List();
				foreach(var registeredSource in _registeredSources)
				{
					var savedSource = savedSources.FirstOrDefault(x => x.Name == registeredSource.Name);
					if(savedSource == null)
					{
						savedSource = new EdoTaskProblemCustomSourceEntity
						{
							Name = registeredSource.Name,
							Importance = registeredSource.Importance,
							Message = registeredSource.Message,
							Description = registeredSource.Description,
							Recommendation = registeredSource.Recommendation
						};
						uow.Save(savedSource);
						changed = true;
						continue;
					}

					if(savedSource.Equals(registeredSource))
					{
						continue;
					}

					savedSource.Importance = registeredSource.Importance;
					savedSource.Message = registeredSource.Message;
					savedSource.Description = registeredSource.Description;
					savedSource.Recommendation = registeredSource.Recommendation;

					uow.Save(savedSource);
					changed = true;
				}
				if(changed)
				{
					uow.Commit();
				}
			}

			_persistedSources = _registeredSources;
		}

		public T GetCustomSource<T>()
			where T : EdoTaskProblemCustomSource
		{
			return _persistedSources.OfType<T>().FirstOrDefault();
		}

		public IEnumerable<EdoTaskProblemCustomSource> GetEdoProblemCustomSources()
		{
			return _persistedSources;
		}
	}
}
