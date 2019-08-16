using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using AdmAssist.Helpers;
using AdmAssist.Models;
using AdmAssist.Services;
using AdmAssist.ViewModels;
using AdmAssist.Views.Converters;
using RemoteQueries;
using StringComparer = AdmAssist.Helpers.StringComparer;

namespace AdmAssist.Views.Pages
{
    /// <summary>
    /// Interaction logic for ScanningPage.xaml
    /// </summary>
    public partial class ScanningPage
    {
        private MainViewModel MainViewModel => (MainViewModel)DataContext;
        //lock object for synchronization;
        private static readonly object SyncLock = new object();

        private readonly List<string> _columnsToFilter = new List<string>();

        public ScanningPage()
        {
            InitializeComponent();

            BindingOperations.EnableCollectionSynchronization(DataGrid.Items, SyncLock);

            Loaded += ScanningPage_Loaded;
            //DataGrid.Columns.CollectionChanged += Columns_CollectionChanged;
        }

        private void Config_Saved()
        {
            UpdateInputBindings();
        }

        private void Config_Loaded()
        {
            UpdateInputBindings();
        }

        private void ScanningPage_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateInputBindings();
            ConfigurationManager.ConfigurationLoaded += Config_Loaded;
            ConfigurationManager.ConfigurationSaved += Config_Saved;
        }

        private void ScanningPage_OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            MainViewModel.ScanningProcessStarted += MainViewModel_ScanningProcessStarted;
        }

        private void OnCbObjectCheckBoxChecked(object sender, RoutedEventArgs e)
        {
            var cb = (CheckBox)sender;

            if ((string)cb.Content == "<all>")
                foreach (SelectableObject<string> cbObject in ColumnChoser.Items)
                    if (cb.IsChecked != null) cbObject.IsSelected = cb.IsChecked.Value;

            StringBuilder sb = new StringBuilder();
            foreach (SelectableObject<string> cbObject in ColumnChoser.Items)
                if (cbObject.IsSelected && cbObject.ObjectData != "<all>")
                    sb.Append($"{cbObject.ObjectData}\n");
            TbColumns.Text = sb.ToString().Trim().TrimEnd(',');
            ColumnChoser.ToolTip = TbColumns.Text;
        }

        private void OnCbObjectsSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBox = (ComboBox)sender;
            comboBox.SelectedItem = null;
        }

        private void TextBoxBase_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            var view = CollectionViewSource.GetDefaultView(DataGrid.ItemsSource);

            if (view == null) return;

            _columnsToFilter.Clear();

            foreach (var column in MainViewModel.FilterColumns)
                if (column.IsSelected) _columnsToFilter.Add(column.ObjectData);

            if (_columnsToFilter.Count == 0) return;

            if (!string.IsNullOrWhiteSpace(SearchBox.Text)) view.Filter = FilterVariables;
            else view.Filter = item => true;

            MainViewModel.AcknowledgedFilterString = SearchBox.Text;
        }

        private bool FilterVariables(object obj)
        {
            var item = (NotifyDynamicDictionary)obj;

            string textFilter = SearchBox.Text.ToLower();

            foreach (var columnName in _columnsToFilter)
            {
                var cell = item[columnName];

                if (cell != null && cell.ToString().ContainsIgnoreCase(textFilter))
                    return true;
            }

            return false;
        }

        private void MainViewModel_ScanningProcessStarted()
        {
            Dispatcher.Invoke(() =>
            {
                TbColumns.Text = "Chose columns...";

                if (DataGrid == null) return;

                DataGrid.Columns.Clear();

                var columns = MainViewModel.Config.UserQuerySet;

                DataGrid.Columns.Add(new DataGridTextColumn { Header = Constants.IpColumnName, Binding = new Binding(Constants.IpColumnName) });
                DataGrid.Columns.Add(new DataGridTextColumn { Header = Constants.StatusColumnName, Binding = new Binding(Constants.StatusColumnName) });

                foreach (var column in columns)
                {
                    var dgColumnBinding = new Binding(column) {Mode = BindingMode.OneWay};

                    if (MainViewModel.RemoteParametersTypes[column] == typeof(PhysicalAddress))
                        dgColumnBinding.Converter = new PhysicalAddressToStringConverter();

                    if (column.Equals(ParameterSet.WmiSysDrvFreeSpace))
                    {
                        dgColumnBinding.Converter = new BytesToMbytesConverter();
                        dgColumnBinding.StringFormat = "{0:F1} (MB)";
                        dgColumnBinding.TargetNullValue = string.Empty;
                        
                    }

                    if (column.Equals(ParameterSet.WmiPhysMemory))
                    {
                        dgColumnBinding.Converter = new KbytesToMbytesConverter();
                        dgColumnBinding.StringFormat = "{0:F1} (MB)";
                        dgColumnBinding.TargetNullValue = string.Empty;
                    }

                    DataGrid.Columns.Add(new DataGridTextColumn { Header = column, Binding = dgColumnBinding });
                }
            });
        }

        private void DataGrid_OnSorting(object sender, DataGridSortingEventArgs e)
        {
            var columnName = e.Column.Header as string;

            if (string.IsNullOrWhiteSpace(columnName)) return;

            Type columnDataType;

            if (columnName.Equals(Constants.IpColumnName))
                columnDataType = typeof(IPAddress);
            else if (columnName.Equals(Constants.StatusColumnName))
                columnDataType = typeof(bool?);
            else
                columnDataType = MainViewModel.RemoteParametersTypes[columnName];

            if (columnDataType == null) return;

            var view = (ListCollectionView)CollectionViewSource.GetDefaultView(DataGrid.ItemsSource);

            if (columnDataType == typeof(IPAddress))
            {
                e.Handled = true;

                if (e.Column.SortDirection == ListSortDirection.Ascending)
                {
                    view.CustomSort = new IpAddressComparer(false, columnName);
                    e.Column.SortDirection = ListSortDirection.Descending;
                }
                else
                {
                    view.CustomSort = new IpAddressComparer(true, columnName);
                    e.Column.SortDirection = ListSortDirection.Ascending;
                }

                return;
            }
            if (columnDataType == typeof(string))
            {
                e.Handled = true;

                if (e.Column.SortDirection == ListSortDirection.Ascending)
                {
                    view.CustomSort = new StringComparer(false, columnName);
                    e.Column.SortDirection = ListSortDirection.Descending;
                }
                else
                {
                    view.CustomSort = new StringComparer(true, columnName);
                    e.Column.SortDirection = ListSortDirection.Ascending;
                }
            }
        }

        //private void Columns_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        //{
        //    if (e.Action == NotifyCollectionChangedAction.Move)
        //    {
        //        var temp = MainViewModel.Config.UserQuerySet[e.OldStartingIndex];

        //        MainViewModel.Config.UserQuerySet.RemoveAt(e.OldStartingIndex - BuiltInCoumnsCount);
        //        MainViewModel.Config.UserQuerySet.Insert(e.NewStartingIndex - BuiltInCoumnsCount, temp);
        //    }
        //}

        private void UpdateInputBindings()
        {
            DataGrid.InputBindings.Clear();

            foreach (var hostCommandNode in MainViewModel.HostOperationsMenuTree.CommandNodesToEnumerable())
            {
                InputBindings.Add(new KeyBinding(hostCommandNode.HostCommand,
                        hostCommandNode.Shortcut.ToInputGesture())
                    { CommandParameter = DataGrid.SelectedItems });
            }
        }

        private void Row_Loaded(object sender, RoutedEventArgs e)
        {
            if (MainViewModel.DoubleClickCommand == null) return;

            var row = sender as DataGridRow;
            row?.InputBindings.Add(new MouseBinding(MainViewModel.DoubleClickCommand,
                new MouseGesture { MouseAction = MouseAction.LeftDoubleClick }) {CommandParameter = DataGrid.SelectedItems});
        }

        private void ScanClick(object sender, RoutedEventArgs e)
        {
            SearchBox.Text = string.Empty;
        }
    }
}
