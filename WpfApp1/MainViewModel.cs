using ArrangeableTabControlLib;
using Reactive.Bindings;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace WpfApp1
{
    class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public ICommand AddItemCommand { get; }

        public ObservableCollection<ItemViewModel> Items { get; } = new();
        public ReactivePropertySlim<ItemViewModel> Item { get; } = new();
        public Func<(Window, ArrangeableTabControl)> TabHostFactory1 { get; }
        public Func<(Window, ArrangeableTabControl)> TabHostFactory2 { get; }

        public MainViewModel()
        {
            this.AddItemCommand = new ReactiveCommandSlim().WithSubscribe(this.AddItem);
            TabHostFactory1 = () =>
            {
                var win = new MainWindow();
                var tab = win.tabControl1;
                tab.Items.Clear();
                return (win, tab);
            };
            TabHostFactory2 = () =>
            {
                var win = new MainWindow();
                var tab = win.tabControl2;
                return (win, tab);
            };
        }

        private int i;

        private void AddItem()
        {
            var item = new ItemViewModel(this.i++);
            this.Item.Value = item;
            this.Items.Add(item);
        }
    }

    class ItemViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public object Data { get; }

        public ItemViewModel(object data)
        {
            this.Data = data;
        }
    }
}
