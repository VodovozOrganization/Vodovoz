using System;
using System.Collections.ObjectModel;
using System.Linq;
using QS.DomainModel.Entity;
using QS.MachineConfig;
using QS.ViewModels;
using WhereIsTheBottle.Commands;
using WhereIsTheBottle.Models;

namespace WhereIsTheBottle.ViewModels
{
	public class EditConnectionViewModel : ViewModelBase
	{
		private RelayCommand _addConnectionCommand;
		private RelayCommand _cancelCommand;
		private ObservableCollection<ConnectionItem> _connectionItems;
		private RelayCommand<int> _deleteConnectionCommand;
		private RelayCommand _saveCommand;
		private ConnectionItem _selectedConnectionItem;
		private RelayCommand<ConnectionItem> _selectionChangedCommand;

		public Action Close;

		public EditConnectionViewModel(IEditConnectionModel editConnectionModel)
		{
			EditConnectionModel = editConnectionModel ?? throw new ArgumentNullException(nameof(editConnectionModel));
			ConnectionItems = new ObservableCollection<ConnectionItem>();
			foreach(var connection in editConnectionModel.GetConnections())
			{
				ConnectionItems.Add(new ConnectionItem { Connection = connection });
			}
		}

		public IEditConnectionModel EditConnectionModel { get; }

		public ObservableCollection<ConnectionItem> ConnectionItems
		{
			get => _connectionItems;
			set => SetField(ref _connectionItems, value);
		}

		public ConnectionItem SelectedConnectionItem
		{
			get => _selectedConnectionItem;
			set => SetField(ref _selectedConnectionItem, value);
		}

		public RelayCommand AddConnectionCommand => _addConnectionCommand ??= new RelayCommand(
			() =>
			{
				var newConnectionItem = new ConnectionItem { Connection = EditConnectionModel.GetNewConnection() };
				_connectionItems.Add(newConnectionItem);
				SelectedConnectionItem = newConnectionItem;
			}
		);

		public RelayCommand<int> DeleteConnectionCommand => _deleteConnectionCommand ??= new RelayCommand<int>(
			index => { ConnectionItems.RemoveAt(index); },
			index => SelectedConnectionItem != null
		);

		public RelayCommand<ConnectionItem> SelectionChangedCommand => _selectionChangedCommand ??= new RelayCommand<ConnectionItem>(
			connection => { ValidateAll(); }
		);

		public RelayCommand CancelCommand => _cancelCommand ??= new RelayCommand(
			() => EditConnectionModel.ConnectionsSaved = false
		);

		public RelayCommand SaveCommand => _saveCommand ??= new RelayCommand(
			() =>
			{
				EditConnectionModel.SaveConnections(ConnectionItems
					.Where(x => x.IsValid)
					.Select(x => x.Connection)
					.ToList());
				EditConnectionModel.ConnectionsSaved = true;
				Close?.Invoke();
			});

		private void ValidateAll()
		{
			foreach(var connectionItem in ConnectionItems)
			{
				connectionItem.ErrorDescription = null;

				if(String.IsNullOrWhiteSpace(connectionItem.Connection.ConnectionName))
				{
					connectionItem.ErrorDescription += "Название подключения не заполнено.";
					continue;
				}
				if(String.IsNullOrWhiteSpace(connectionItem.Connection.Server))
				{
					connectionItem.ErrorDescription = "Адрес сервера не заполнен.";
					continue;
				}
				if(String.IsNullOrWhiteSpace(connectionItem.Connection.BaseName))
				{
					connectionItem.ErrorDescription = "Название базы не заполнено.";
					continue;
				}
			}
		}
	}

	public class ConnectionItem : PropertyChangedBase
	{
		private Connection _connection;
		private string _errorDescription;

		public Connection Connection
		{
			get => _connection;
			set => SetField(ref _connection, value, () => Connection);
		}

		public string ErrorDescription
		{
			get => _errorDescription;
			set => SetField(ref _errorDescription, value, () => ErrorDescription);
		}

		public bool IsValid => String.IsNullOrWhiteSpace(ErrorDescription);
	}
}
