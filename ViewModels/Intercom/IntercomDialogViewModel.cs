using System.Collections.ObjectModel;
using System.Windows.Input;
using ReactiveUI;
using System.Linq;
using System.IO;
using System.Text.Json;
using System;

public class RelayCommand : ICommand
{
    private readonly Action<object?> _execute;
    private readonly Func<object?, bool>? _canExecute;

    public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute;
    }

    public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;
    public void Execute(object? parameter) => _execute(parameter);
    public event EventHandler? CanExecuteChanged;
}

namespace ORControlPanelNew.ViewModels.Intercom
{
    public class Contact
    {
        public string Name { get; set; } = string.Empty;
        public string Number { get; set; } = string.Empty;
    }

    public class IntercomDialogViewModel : ReactiveObject
    {
        private const string CONTACTS_FILE = "contacts.json";
        private string _dialedNumber = string.Empty;
        public string DialedNumber
        {
            get => _dialedNumber;
            set => this.RaiseAndSetIfChanged(ref _dialedNumber, value);
        }

        private string _newContactName = string.Empty;
        public string NewContactName
        {
            get => _newContactName;
            set => this.RaiseAndSetIfChanged(ref _newContactName, value);
        }

        private string _newContactNumber = string.Empty;
        public string NewContactNumber
        {
            get => _newContactNumber;
            set => this.RaiseAndSetIfChanged(ref _newContactNumber, value);
        }

        public static ObservableCollection<Contact> SharedContacts { get; } = new ObservableCollection<Contact>();
        public ObservableCollection<Contact> Contacts => SharedContacts;

        public ICommand DialCommand { get; }
        public ICommand BackspaceCommand { get; }
        public ICommand SaveContactCommand { get; }
        public ICommand DeleteContactCommand { get; }

        public IntercomDialogViewModel()
        {
            LoadContacts();
            
            DialCommand = ReactiveCommand.Create<string>(ch => DialedNumber += ch);
            BackspaceCommand = ReactiveCommand.Create(() =>
            {
                if (!string.IsNullOrEmpty(DialedNumber))
                    DialedNumber = DialedNumber.Substring(0, DialedNumber.Length - 1);
            });
            SaveContactCommand = ReactiveCommand.Create(() =>
            {
                if (!string.IsNullOrWhiteSpace(NewContactName) && !string.IsNullOrWhiteSpace(NewContactNumber))
                {
                    if (!Contacts.Any(c => c.Name == NewContactName && c.Number == NewContactNumber))
                    {
                        Contacts.Add(new Contact { Name = NewContactName, Number = NewContactNumber });
                        SaveContacts();
                    }
                    NewContactName = string.Empty;
                    NewContactNumber = string.Empty;
                }
            });
            DeleteContactCommand = new RelayCommand(DeleteContact);
        }

        private void LoadContacts()
        {
            try
            {
                if (File.Exists(CONTACTS_FILE))
                {
                    string json = File.ReadAllText(CONTACTS_FILE);
                    var contacts = JsonSerializer.Deserialize<Contact[]>(json);
                    if (contacts != null)
                    {
                        SharedContacts.Clear();
                        foreach (var contact in contacts)
                        {
                            SharedContacts.Add(contact);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading contacts: {ex.Message}");
            }
        }

        private void SaveContacts()
        {
            try
            {
                string json = JsonSerializer.Serialize(Contacts.ToArray(), new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(CONTACTS_FILE, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving contacts: {ex.Message}");
            }
        }

        private void DeleteContact(object? parameter)
        {
            if (parameter is Contact contact && Contacts.Contains(contact))
            {
                Contacts.Remove(contact);
                SaveContacts();
            }
        }

        public IntercomDialogViewModel Self => this;
    }
} 