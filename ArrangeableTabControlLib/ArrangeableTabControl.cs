using System.Collections;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;

namespace ArrangeableTabControlLib
{
    /// <summary>
    /// このカスタム コントロールを XAML ファイルで使用するには、手順 1a または 1b の後、手順 2 に従います。
    ///
    /// 手順 1a) 現在のプロジェクトに存在する XAML ファイルでこのカスタム コントロールを使用する場合
    /// この XmlNamespace 属性を使用場所であるマークアップ ファイルのルート要素に
    /// 追加します:
    ///
    ///     xmlns:MyNamespace="clr-namespace:ArrangeableTabControlLib"
    ///
    ///
    /// 手順 1b) 異なるプロジェクトに存在する XAML ファイルでこのカスタム コントロールを使用する場合
    /// この XmlNamespace 属性を使用場所であるマークアップ ファイルのルート要素に
    /// 追加します:
    ///
    ///     xmlns:MyNamespace="clr-namespace:ArrangeableTabControlLib;assembly=ArrangeableTabControlLib"
    ///
    /// また、XAML ファイルのあるプロジェクトからこのプロジェクトへのプロジェクト参照を追加し、
    /// リビルドして、コンパイル エラーを防ぐ必要があります:
    ///
    ///     ソリューション エクスプローラーで対象のプロジェクトを右クリックし、
    ///     [参照の追加] の [プロジェクト] を選択してから、このプロジェクトを参照し、選択します。
    ///
    ///
    /// 手順 2)
    /// コントロールを XAML ファイルで使用します。
    ///
    ///     <MyNamespace:ArrangeableTabControl/>
    ///
    /// </summary>
    public partial class ArrangeableTabControl : TabControl
    {
        private static List<ArrangeableTabControl> arrangeableTabControls = new List<ArrangeableTabControl>();

        private FrameworkElement? itemsPanel;

        private Point? arrangeStartPosition;
        private ArrangeableTabItem? arrangingTabItem;
        private TabItemArrangingAdorner? arrangeAdorner;

        public Func<(Window, ArrangeableTabControl)> TabHostFactory
        {
            get { return (Func<(Window, ArrangeableTabControl)>)GetValue(TabHostFactoryProperty); }
            set { SetValue(TabHostFactoryProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TabHostFactory.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TabHostFactoryProperty =
            DependencyProperty.Register("TabHostFactory", typeof(Func<(Window, ArrangeableTabControl)>), typeof(ArrangeableTabControl), new PropertyMetadata(null));

        static ArrangeableTabControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ArrangeableTabControl), new FrameworkPropertyMetadata(typeof(ArrangeableTabControl)));
        }

        public static readonly RoutedCommand CloseTabCommand = new RoutedCommand("closeTab", typeof(ArrangeableTabControl));

        public ArrangeableTabControl()
        {
            this.CommandBindings.Add(new CommandBinding(CloseTabCommand, OnCloseTabCommand));

            this.AddHandler(ArrangeableTabItem.ArrangeStartedEvent, (MouseButtonEventHandler)this.OnArrangeStart);
            this.AddHandler(ArrangeableTabItem.ArrangeRequiredEvent, (MouseEventHandler)this.OnArranged);

            arrangeableTabControls.Add(this);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            if (this.itemsPanel != null)
            {
                this.itemsPanel.RemoveHandler(MouseMoveEvent, (MouseEventHandler)this.OnArranging);
                this.itemsPanel.RemoveHandler(MouseUpEvent, (MouseButtonEventHandler)this.OnArangeCompleted);
                this.itemsPanel.RemoveHandler(MouseLeaveEvent, (MouseEventHandler)this.OnSeparated);
            }
            this.itemsPanel = (FrameworkElement)this.GetTemplateChild("PART_ItemsPanel");
            this.itemsPanel.AddHandler(MouseMoveEvent, (MouseEventHandler)this.OnArranging);
            this.itemsPanel.AddHandler(MouseUpEvent, (MouseButtonEventHandler)this.OnArangeCompleted);
            this.itemsPanel.AddHandler(MouseLeaveEvent, (MouseEventHandler)this.OnSeparated);
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new ArrangeableTabItem();
        }

        private void OnCloseTabCommand(object sender, ExecutedRoutedEventArgs e)
        {
            var tabItem = (TabItem)e.Parameter;
            var itemIndex = this.ItemContainerGenerator.IndexFromContainer(tabItem);
            new ItemsOperationWrapper(this).RemoveAt(itemIndex);
            e.Handled = true;
        }

        private void OnArrangeStart(object sender, MouseButtonEventArgs e)
        {
            var tabItem = (ArrangeableTabItem)e.OriginalSource;
            var position = e.GetPosition(this);
            var offset = e.GetPosition(tabItem);
            var items = new ItemsOperationWrapper(this);

            if (items.Count() == 1)
            {
                var win = Window.GetWindow(this);
                MergeableWindowMove(win, this, tabItem, offset);
            }
            else
            {
                this.ShowArrangeAdorner(tabItem, position, offset);
            }

            e.Handled = true;
        }

        private void ShowArrangeAdorner(ArrangeableTabItem tabItem, Point position, Point offset)
        {
            var adorner = new TabItemArrangingAdorner(
                tabItem.GetContentHost(),
                this,
                position: position,
                offset: new Point(-offset.X, -offset.Y));
            adorner.IsHitTestVisible = false;
            AdornerLayer.GetAdornerLayer(this).Add(adorner);

            tabItem.Visibility = Visibility.Hidden;

            this.arrangeStartPosition = offset;
            this.arrangingTabItem = tabItem;
            this.arrangeAdorner = adorner;
        }

        private void OnArranged(object sender, MouseEventArgs e)
        {
            if (this.arrangeAdorner is null) { return; }

            var tabItem = (TabItem)e.OriginalSource;
            var itemRelPos = this.PointFromScreen(default) - tabItem.PointFromScreen(default);
            switch (this.TabStripPlacement)
            {
                case Dock.Left:
                case Dock.Right:
                    this.arrangeAdorner.Position = new Point(
                        itemRelPos.X - this.arrangeAdorner.Offset.X,
                        e.GetPosition(this).Y);
                    break;
                case Dock.Top:
                case Dock.Bottom:
                    this.arrangeAdorner.Position = new Point(
                        e.GetPosition(this).X,
                        itemRelPos.Y - this.arrangeAdorner.Offset.Y);
                    break;
            }
            var index = this.ItemContainerGenerator.IndexFromContainer(tabItem);
            this.MoveCurrentTo(index);
            e.Handled = true;
        }

        private void OnArranging(object sender, MouseEventArgs e)
        {
            if (this.arrangeAdorner is null) { return; }

            switch (this.TabStripPlacement)
            {
                case Dock.Left:
                case Dock.Right:
                    this.arrangeAdorner.Position = new Point(
                        this.arrangeAdorner.Position.X,
                        e.GetPosition(this).Y);
                    break;
                case Dock.Top:
                case Dock.Bottom:
                    this.arrangeAdorner.Position = new Point(
                        e.GetPosition(this).X,
                        this.arrangeAdorner.Position.Y);
                    break;
            }
            e.Handled = true;
        }

        private void OnArangeCompleted(object sender, MouseButtonEventArgs e)
        {
            this.TerminateArrange();
            e.Handled = true;
        }

        private void TerminateArrange()
        {
            if (this.arrangeAdorner is null) { return; }
            AdornerLayer.GetAdornerLayer(this).Remove(this.arrangeAdorner);

            this.arrangingTabItem!.Visibility = Visibility.Visible;
            this.arrangeStartPosition = null;
            this.arrangingTabItem = null;
            this.arrangeAdorner = null;
        }

        private void OnSeparated(object sender, MouseEventArgs e)
        {
            if (this.arrangeAdorner is null) { return; }
            if (this.TabHostFactory is null) { return; }
            if (sender != e.Source) { return; }

            var arrangeStartPosition = this.arrangeStartPosition!.Value;
            this.TerminateArrange();
            (var win, var tabControl) = this.TabHostFactory.Invoke();

            var fromItems = new ItemsOperationWrapper(this);
            var toItems = new ItemsOperationWrapper(tabControl);
            var currentIndex = fromItems.CurrentIndex();
            var current = fromItems[currentIndex];
            fromItems.RemoveAt(currentIndex);
            toItems.Add(current!);
            void loaded(object _sender, RoutedEventArgs _e)
            {
                var tabControl = (ArrangeableTabControl)_e.Source;
                var win = Window.GetWindow(tabControl);
                var tabItem = (ArrangeableTabItem)tabControl.ItemContainerGenerator.ContainerFromIndex(0);
                var mousePosition = Mouse.GetPosition(win);
                var winPosition = win.PointFromScreen(default);
                var tabItemPosition = tabItem.PointFromScreen(default);
                win.Left = win.Left + mousePosition.X - winPosition.X + tabItemPosition.X - arrangeStartPosition.X;
                win.Top = win.Top + mousePosition.Y - winPosition.Y + tabItemPosition.Y - arrangeStartPosition.Y;
                tabControl.Loaded -= loaded;
            };
            tabControl.Loaded += loaded;
            win.Show();

            MergeableWindowMove(
                win,
                tabControl,
                (ArrangeableTabItem)tabControl.ItemContainerGenerator.ContainerFromIndex(0),
                arrangeStartPosition);
        }

        private void MoveCurrentTo(int index)
        {
            var items = new ItemsOperationWrapper(this);
            var currentIndex = items.CurrentIndex();
            if (currentIndex == index) { return; }
            Debug.WriteLine($"Move {currentIndex} to {index}");
            if (currentIndex < index)
            {
                for (int i = 0; i < index - currentIndex; ++i)
                {
                    var item = items[currentIndex + 1 + i];
                    items.RemoveAt(currentIndex + 1 + i);
                    items.Insert(currentIndex + i, item!);
                }
            }
            else
            {
                for (int i = 0; i < currentIndex - index; ++i)
                {
                    var item = items[currentIndex - 1 - i];
                    items.RemoveAt(currentIndex - 1 - i);
                    items.Insert(currentIndex - i, item!);
                }
            }
        }

        private void Win_LocationChanged(object? sender, EventArgs e)
        {
            var targetTabs = ArrangeableTabControlsFromTop().Where(tab => tab != this);
            foreach (var targetTab in targetTabs)
            {
                var targetItemsPanel = (FrameworkElement)targetTab.GetTemplateChild("PART_ItemsPanel");
                GetCursorPos(out var mousePos);
                var mousePosInPanel = targetItemsPanel.PointFromScreen(new Point(mousePos.X, mousePos.Y));
                if (0 < mousePosInPanel.X && mousePosInPanel.X < targetItemsPanel.ActualWidth &&
                    0 < mousePosInPanel.Y && mousePosInPanel.Y < targetItemsPanel.ActualHeight)
                {
                    if (targetTab.ItemsSource is null && this.ItemsSource is null)
                    {
                        var tabItem = (ArrangeableTabItem)this.Items[0];
                        this.Items.RemoveAt(0);
                        targetTab.Items.Add(tabItem);
                        var pos = Mouse.GetPosition(targetTab);
                        if (targetTab.TabStripPlacement is Dock.Left or Dock.Right)
                        {
                            pos.X = this.arrangeStartPosition!.Value.X;
                        }
                        else
                        {
                            pos.Y = this.arrangeStartPosition!.Value.Y;
                        }
                        targetTab.ShowArrangeAdorner(tabItem, pos, this.arrangeStartPosition!.Value);
                        Window.GetWindow(this).Close();
                        targetTab.SelectedItem = tabItem;
                    }
                    else if (targetTab.ItemsSource is IList dest && this.ItemsSource is IList src)
                    {
                        var item = src[0];
                        src.RemoveAt(0);
                        dest.Add(item);
                        var tabItem = (ArrangeableTabItem)targetTab.ItemContainerGenerator.ContainerFromItem(item);
                        void selectNewItemAndClosePrevWindow(object sender, RoutedEventArgs e)
                        {
                            var pos = Mouse.GetPosition(targetTab);
                            if (targetTab.TabStripPlacement is Dock.Left or Dock.Right)
                            {
                                pos.X = this.arrangeStartPosition!.Value.X;
                            }
                            else
                            {
                                pos.Y = this.arrangeStartPosition!.Value.Y;
                            }
                            targetTab.ShowArrangeAdorner(tabItem, pos, this.arrangeStartPosition!.Value);
                            Window.GetWindow(this).Close();
                            targetTab.SelectedItem = item;
                            tabItem.Loaded -= selectNewItemAndClosePrevWindow;
                        }
                        if (tabItem.IsLoaded)
                        {
                            selectNewItemAndClosePrevWindow(null, null);
                        }
                        else
                        {
                            tabItem.Loaded += selectNewItemAndClosePrevWindow;
                        }
                    }
                    break;
                }
            }
        }

        private static void MergeableWindowMove(
            Window window,
            ArrangeableTabControl tabControl,
            ArrangeableTabItem tabItem,
            Point moveOffset)
        {
            if (Mouse.LeftButton == MouseButtonState.Released) { return; }

            tabControl.arrangeStartPosition = moveOffset;
            tabControl.arrangingTabItem = tabItem;
            window.LocationChanged += tabControl.Win_LocationChanged;
            window.DragMove();
            window.LocationChanged -= tabControl.Win_LocationChanged;
            tabControl.arrangeStartPosition = null;
            tabControl.arrangingTabItem = null;
        }

        private static IEnumerable<ArrangeableTabControl> ArrangeableTabControlsFromTop()
        {
            foreach (var orphanedTab in arrangeableTabControls.Select(tab => (tab: tab, win: Window.GetWindow(tab)))
                .Where(tabwin => Application.Current.Windows.Cast<Window>().FirstOrDefault(win => tabwin.win == win) is null).ToList())
            {
                arrangeableTabControls.Remove(orphanedTab.tab);
            }
            var handleTab = arrangeableTabControls.Select(tab => (tab: tab, win: Window.GetWindow(tab)))
                .Where(tabwin => Application.Current.Windows.Cast<Window>().FirstOrDefault(win => tabwin.win == win) is not null)
                .GroupBy(wintab => ((HwndSource)PresentationSource.FromVisual(wintab.win)).Handle)
                .ToDictionary(wintabg => wintabg.Key, wintabg => wintabg);
                
            for (IntPtr hWnd = GetTopWindow(IntPtr.Zero); hWnd != IntPtr.Zero; hWnd = GetWindow(hWnd, GW_HWNDNEXT))
            {
                if (handleTab.TryGetValue(hWnd, out IGrouping<nint, (ArrangeableTabControl, Window)>? value))
                {
                    foreach (var grp in value)
                    {
                        yield return grp.Item1;
                    }
                }
            }
        }

        private const uint GW_HWNDNEXT = 2;
        [LibraryImport("user32.dll")] 
        static private partial IntPtr GetTopWindow(IntPtr hWnd);
        [LibraryImport("user32.dll")]
        static private partial IntPtr GetWindow(IntPtr hWnd, uint wCmd);
        [LibraryImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static private partial bool GetCursorPos(out Win32Point point);
        [StructLayout(LayoutKind.Sequential)]
        struct Win32Point
        {
            public int X;
            public int Y;
        }


        private class ItemsOperationWrapper
        {
            private readonly Selector selector;

            public ItemsOperationWrapper(Selector selector)
            {
                this.selector = selector;
            }

            public int CurrentIndex()
            {
                if (this.selector.ItemsSource is null)
                {
                    return this.selector.Items.IndexOf(this.selector.SelectedItem);
                }
                else if (this.selector.ItemsSource is IList itemsList)
                {
                    return itemsList.IndexOf(this.selector.SelectedItem);
                }
                else
                {
                    return this.selector.ItemsSource
                        .Cast<object>()
                        .Select((obj, i) => (obj, i))
                        .FirstOrDefault(pair => pair.obj == this.selector.SelectedItem)
                        .i;
                }
            }

            public object? this[int index]
            {
                get
                {
                    return this.selector.ItemsSource is null ?
                        this.selector.Items[index] :
                        this.selector.ItemsSource is IList itemsList ?
                        itemsList[index] :
                        this.selector.ItemsSource.Cast<object>().ElementAt(index);
                }
            }

            public void RemoveAt(int index)
            {
                if (this.selector.ItemsSource is null)
                {
                    this.selector.Items.RemoveAt(index);
                }
                else if (this.selector.ItemsSource is IList itemsList)
                {
                    itemsList.RemoveAt(index);
                }
                else
                {
                    var itemsEnum = this.selector.ItemsSource.Cast<object>();
                    this.selector.ItemsSource = itemsEnum.Take(index).Concat(itemsEnum.Skip(index + 1)).ToList();
                }
            }

            public void Insert(int index, object item)
            {
                if (this.selector.ItemsSource is null)
                {
                    this.selector.Items.Insert(index, item);
                }
                else if (this.selector.ItemsSource is IList itemsList)
                {
                    itemsList.Insert(index, item);
                }
                else
                {
                    var itemsEnum = this.selector.ItemsSource.Cast<object>();
                    this.selector.ItemsSource = itemsEnum.Take(index).Concat(new[] { item }).Concat(itemsEnum.Skip(index)).ToList();
                }
            }

            public void Add(object item)
            {
                if (this.selector.ItemsSource is null)
                {
                    this.selector.Items.Add(item);
                }
                else if (this.selector.ItemsSource is IList itemsList)
                {
                    itemsList.Add(item);
                }
                else
                {
                    var itemsEnum = this.selector.ItemsSource.Cast<object>();
                    this.selector.ItemsSource = itemsEnum.Concat(new[] { item });
                }
            }

            public int Count()
            {
                if (this.selector.ItemsSource is null)
                {
                    return this.selector.Items.Count;
                }
                else if (this.selector.ItemsSource is IList itemsList)
                {
                    return itemsList.Count;
                }
                else
                {
                    var itemsEnum = this.selector.ItemsSource.Cast<object>();
                    return itemsEnum.Count();
                }
            }
        }
    }
}
