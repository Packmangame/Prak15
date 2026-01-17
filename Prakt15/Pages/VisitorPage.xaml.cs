using Microsoft.EntityFrameworkCore;
using Prakt15.Models;
using Prakt15.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Prakt15
{
    /// <summary>
    /// Логика взаимодействия для VisitorPage.xaml
    /// </summary>
    public partial class VisitorPage : Page
    {
        private readonly Prak15Context _db = DBService.Instance.Context;
        private ObservableCollection<Product> _products = new ObservableCollection<Product>();
        private ICollectionView? _productsView;
        private string _searchQuery = "";
        private decimal? _priceFrom;
        private decimal? _priceTo;

        public VisitorPage()
        {
            InitializeComponent();
            Loaded += VisitorPage_Loaded;
        }

        private void VisitorPage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                _products.Clear();

                var products = _db.Products
                    .Include(p => p.Category)
                    .Include(p => p.Brand)
                    .Include(p => p.Tags)
                    .OrderBy(p => p.Name)
                    .ToList();

                foreach (var product in products)
                {
                    _products.Add(product);
                }


                cmbCategory.ItemsSource = _db.Categories.OrderBy(c => c.Name).ToList();
                cmbBrand.ItemsSource = _db.Brands.OrderBy(b => b.Name).ToList();

                
                _productsView = CollectionViewSource.GetDefaultView(_products);
                _productsView.Filter = FilterProduct;
                listViewProducts.ItemsSource = _productsView;

                UpdateCounters();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool FilterProduct(object obj)
        {
            if (obj is not Product product)
                return false;

            // Поиск
            if (!string.IsNullOrEmpty(_searchQuery))
            {
                string searchLower = _searchQuery.ToLower();
                bool nameMatch = product.Name.ToLower().Contains(searchLower);
                bool descMatch = product.Description.ToLower().Contains(searchLower);
                if (!nameMatch && !descMatch)
                    return false;
            }

            // Категория
            if (cmbCategory.SelectedItem is Category selectedCategory &&
                product.CategoryId != selectedCategory.Id)
                return false;

            // Бренд
            if (cmbBrand.SelectedItem is Brand selectedBrand &&
                product.BrandId != selectedBrand.Id)
                return false;

            // Цена
            if (_priceFrom.HasValue && product.Price < _priceFrom.Value)
                return false;

            if (_priceTo.HasValue && product.Price > _priceTo.Value)
                return false;

            return true;
        }

        private void UpdateCounters()
        {
            txtTotalCount.Text = $"Всего товаров: {_products.Count}";
            txtFilteredCount.Text = $"Показано: {(_productsView?.Cast<Product>().Count() ?? 0)}";
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            _searchQuery = txtSearch.Text;
            _productsView?.Refresh();
            UpdateCounters();
        }

        private void CmbCategory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _productsView?.Refresh();
            UpdateCounters();
        }

        private void CmbBrand_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _productsView?.Refresh();
            UpdateCounters();
        }

        private void TxtPriceFrom_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtPriceFrom.Text))
            {
                _priceFrom = null;
                txtPriceFrom.ClearValue(Border.BorderBrushProperty);
            }
            else if (decimal.TryParse(txtPriceFrom.Text, out decimal price) && price >= 0)
            {
                _priceFrom = price;
                txtPriceFrom.ClearValue(Border.BorderBrushProperty);
            }
            else
            {
                txtPriceFrom.BorderBrush = Brushes.Red;
                return;
            }

            _productsView?.Refresh();
            UpdateCounters();
        }

        private void TxtPriceTo_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtPriceTo.Text))
            {
                _priceTo = null;
                txtPriceTo.ClearValue(Border.BorderBrushProperty);
            }
            else if (decimal.TryParse(txtPriceTo.Text, out decimal price) && price >= 0)
            {
                _priceTo = price;
                txtPriceTo.ClearValue(Border.BorderBrushProperty);
            }
            else
            {
                txtPriceTo.BorderBrush = Brushes.Red;
                return;
            }

            _productsView?.Refresh();
            UpdateCounters();
        }

        private void TxtPrice_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox textBox = (TextBox)sender;

            foreach (char c in e.Text)
            {
                if (!char.IsDigit(c) && c != '.')
                {
                    e.Handled = true;
                    return;
                }
            }

            if (e.Text == "." && textBox.Text.Contains('.'))
            {
                e.Handled = true;
            }
        }

        private void CmbSort_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_productsView == null) return;

            _productsView.SortDescriptions.Clear();

            if (cmbSort.SelectedItem is ComboBoxItem selectedItem && selectedItem.Tag != null)
            {
                string sortProperty = selectedItem.Tag.ToString() switch
                {
                    "Name" => "Name",
                    "PriceAsc" or "PriceDesc" => "Price",
                    "StockAsc" or "StockDesc" => "Stock",
                    _ => ""
                };

                var direction = selectedItem.Tag.ToString() switch
                {
                    "PriceDesc" or "StockDesc" => ListSortDirection.Descending,
                    _ => ListSortDirection.Ascending
                };

                if (!string.IsNullOrEmpty(sortProperty))
                {
                    _productsView.SortDescriptions.Add(new SortDescription(sortProperty, direction));
                }
            }
        }

        private void TxtPrice_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtPriceFrom.Text))
                _priceFrom = null;
            else if (double.TryParse(txtPriceFrom.Text, out double fromPrice))
                _priceFrom = Convert.ToDecimal( fromPrice);

            if (string.IsNullOrWhiteSpace(txtPriceTo.Text))
                _priceTo = null;
            else if (double.TryParse(txtPriceTo.Text, out double toPrice))
                _priceTo = Convert.ToDecimal(toPrice);

            _productsView?.Refresh();
            UpdateCounters();
        }

        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            txtSearch.Clear();
            cmbCategory.SelectedIndex = -1;
            cmbBrand.SelectedIndex = -1;
            txtPriceFrom.Clear();
            txtPriceTo.Clear();
            cmbSort.SelectedIndex = 0;

            _searchQuery = "";
            _priceFrom = null;
            _priceTo = null;

            if (_productsView != null)
            {
                _productsView.SortDescriptions.Clear();
                _productsView.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
                _productsView.Refresh();
            }

            UpdateCounters();
        }

        private void TxtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                _productsView?.Refresh();
                UpdateCounters();
            }
        }
    }

   
    public class SelectableItem<T>
    {
        public T? Item { get; set; }
        public string DisplayName { get; set; } = "";
        public bool IsAllOption { get; set; }
    }
}

