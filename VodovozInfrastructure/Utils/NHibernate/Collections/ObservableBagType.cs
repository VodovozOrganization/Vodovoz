using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NHibernate.Collection;
using NHibernate.Engine;
using NHibernate.Persister.Collection;
using NHibernate.UserTypes;

namespace VodovozInfrastructure.Utils.NHibernate.Collections
{
    /// <summary>
    /// The NHibernate type for a generic bag collection that fires events
    /// when item(s) have been added to or removed from the collection.
    /// </summary>
    /// <typeparam name="T">
    /// The type of items in the bag
    /// </typeparam>
    /// <author>Adrian Alexander</author>
    public class ObservableBagType<T> : IUserCollectionType
    {
        /// <summary>
        /// Optional operation. Does the collection contain the entity instance?
        /// </summary>
        /// <param name="collection">
        /// The collection.
        /// </param>
        /// <param name="entity">
        /// The entity.
        /// </param>
        /// <returns>
        /// The contains.
        /// </returns>
        /// <inheritdoc />
        public bool Contains(object collection, object entity) => ((IList<T>)collection).Contains((T)entity);

        /// <summary>
        /// Return an <see cref="T:System.Collections.IEnumerable" /> over the elements of this collection - the passed collection
        /// instance may or may not be a wrapper
        /// </summary>
        /// <param name="collection">
        /// The collection.
        /// </param>
        /// <returns>
        /// The <see cref="IEnumerable" />.
        /// </returns>
        /// <inheritdoc />
        public IEnumerable GetElements(object collection) => (IEnumerable)collection;

        /// <summary>
        /// Optional operation. Return the index of the entity in the collection.
        /// </summary>
        /// <param name="collection">
        /// The collection.
        /// </param>
        /// <param name="entity">
        /// The entity.
        /// </param>
        /// <returns>
        /// The index of.
        /// </returns>
        /// <inheritdoc />
        public object IndexOf(object collection, object entity) => ((IList<T>)collection).IndexOf((T) entity);

        /// <summary>
        /// Instantiate an empty instance of the "underlying" collection (not a wrapper),
        /// but with the given anticipated size (i.e. accounting for initial size
        /// and perhaps load factor).
        /// </summary>
        /// <param name="anticipatedSize">
        /// The anticipated size of the instantiated collection
        /// after we are done populating it.  Note, may be negative to indicate that
        /// we not yet know anything about the anticipated size (i.e., when initializing
        /// from a result set row by row).
        /// </param>
        /// <returns>
        /// The instantiate.
        /// </returns>
        /// <inheritdoc />
        public object Instantiate(int anticipatedSize) => new ObservableCollection<T>();

        /// <summary>
        /// Instantiate an uninitialized instance of the collection wrapper
        /// </summary>
        /// <param name="session">
        /// The session.
        /// </param>
        /// <param name="persister">
        /// The persister.
        /// </param>
        /// <returns>
        /// The <see cref="IPersistentCollection" />.
        /// </returns>
        /// <inheritdoc />
        public IPersistentCollection Instantiate(ISessionImplementor session, ICollectionPersister persister) => 
            new PersistentObservableBag<T>(session);

        /// <summary>
        /// Replace the elements of a collection with the elements of another collection
        /// </summary>
        /// <param name="original">
        /// The original.
        /// </param>
        /// <param name="target">
        /// The target.
        /// </param>
        /// <param name="persister">
        /// The persister.
        /// </param>
        /// <param name="owner">
        /// The owner.
        /// </param>
        /// <param name="copyCache">
        /// The copy cache dictionary.
        /// </param>
        /// <param name="session">
        /// The session.
        /// </param>
        /// <returns>
        /// The replace elements.
        /// </returns>
        /// <inheritdoc />
        public object ReplaceElements(object original, object target, ICollectionPersister persister, object owner, IDictionary copyCache, ISessionImplementor session)
        {
            var result = (IList<T>)target;
            result.Clear();
            
            foreach (var item in (IEnumerable)original) {
                result.Add((T)item);
            }

            return result;
        }

        /// <summary>
        /// Wrap an instance of a collection
        /// </summary>
        /// <param name="session">
        /// The session.
        /// </param>
        /// <param name="collection">
        /// The collection.
        /// </param>
        /// <returns>
        /// The <see cref="IPersistentCollection" />.
        /// </returns>
        /// <inheritdoc />
        public IPersistentCollection Wrap(ISessionImplementor session, object collection) => 
            new PersistentObservableBag<T>(session, (ObservableCollection<T>)collection);
    }
}