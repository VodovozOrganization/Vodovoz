using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using NHibernate.Collection.Generic;
using NHibernate.DebugHelpers;
using NHibernate.Engine;
using NHibernate.Persister.Collection;

namespace VodovozInfrastructure.Utils.NHibernate.Collections
{
    [Serializable]
    [DebuggerTypeProxy(typeof(CollectionProxy<>))]
    public class PersistentObservableBag<T> : PersistentGenericBag<T>, INotifyCollectionChanged, INotifyPropertyChanged
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PersistentObservableBag{T}" /> class.
        /// </summary>
        /// <param name="session">
        /// The session.
        /// </param>
        public PersistentObservableBag(ISessionImplementor session) : base(session) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="PersistentObservableBag{T}" /> class.
        /// </summary>
        /// <param name="session">
        /// The session.
        /// </param>
        /// <param name="coll">
        /// The collection.
        /// </param>
        public PersistentObservableBag(ISessionImplementor session, ICollection<T> coll) : base(session, coll)
        {
            if (coll != null) {
                ((INotifyCollectionChanged)coll).CollectionChanged += OnCollectionChanged;
                ((INotifyPropertyChanged)coll).PropertyChanged += OnPropertyChanged;
            }
        }

        /// <summary>
        /// Occurs when the collection changes.
        /// </summary>
        public event NotifyCollectionChangedEventHandler CollectionChanged;
        
        /// <summary>
        /// Occurs when the property changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Before the initialize.
        /// </summary>
        /// <param name="persister">
        /// The persister.
        /// </param>
        /// <param name="anticipatedSize">
        /// Size of the anticipated.
        /// </param>
        public override void BeforeInitialize(ICollectionPersister persister, int anticipatedSize)
        {
            base.BeforeInitialize(persister, anticipatedSize);
            ((INotifyCollectionChanged)InternalBag).CollectionChanged += OnCollectionChanged;
            ((INotifyPropertyChanged)InternalBag).PropertyChanged += OnPropertyChanged;
        }

        /// <summary>
        /// Called when [collection changed].
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="args">
        /// The <see cref="System.Collections.Specialized.NotifyCollectionChangedEventArgs" /> instance containing the event data.
        /// </param>
        protected void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args) => 
            CollectionChanged?.Invoke(this, args);
        
        /// <summary>
        /// Called when [property changed].
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="args">
        /// The <see cref="System.ComponentModel.PropertyChangedEventArgs" /> instance containing the event data.
        /// </param>
        protected void OnPropertyChanged(object sender, PropertyChangedEventArgs args) => 
            PropertyChanged?.Invoke(this, args);
    }
}