//using Prakt15.Models;
//using Prakt15.Services;
using Prakt15.Models;
using Prakt15.Services;
using Prakt15.Validation;
using System;
using System.Collections.Generic;
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
    /// Логика взаимодействия для EditProductWindow.xaml
    /// </summary>
    public partial class EditProductWindow : Window
    {
        private readonly Prak15Context _db = DBService.Instance.Context;
        private Product? _product;
        private List<Tag> _allTags = new();
        private List<Tag> _selectedTags = new();

        public EditProductWindow(Product? product = null)
        {
            InitializeComponent();
            _product = product;
            LoadData();
            if (product != null)
                LoadProductData();
        }

        private void LoadData()
        {
            try
            {
                cmbCategory.ItemsSource = _db.Categories.OrderBy(c => c.Name).ToList();
                cmbBrand.ItemsSource = _db.Brands.OrderBy(b => b.Name).ToList();

                _allTags = _db.Tags.OrderBy(t => t.Name).ToList();
                lstTags.ItemsSource = _allTags;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadProductData()
        {
            if (_product == null) return;

            try
            {
                txtName.Text = _product.Name;
                txtDescription.Text = _product.Description;
                txtPrice.Text = _product.Price.ToString("F2");
                txtStock.Text = _product.Stock.ToString();
                txtRating.Text = _product.Rating.ToString("F1");
                txtCreatedAt.Text = _product.CreatedAt.ToString("yyyy-MM-dd");

                cmbCategory.SelectedItem = _db.Categories.FirstOrDefault(c => c.Id == _product.CategoryId);
                cmbBrand.SelectedItem = _db.Brands.FirstOrDefault(b => b.Id == _product.BrandId);

                if (_product.Tags != null)
                {
                    _selectedTags = _product.Tags.ToList();
                    lstTags.SelectedItems.Clear();
                    foreach (var tag in _selectedTags)
                    {
                        lstTags.SelectedItems.Add(tag);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки товара: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Валидация
                if (string.IsNullOrWhiteSpace(txtName.Text))
                {
                    MessageBox.Show("Введите название товара", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    txtName.Focus();
                    return;
                }

                if (!decimal.TryParse(txtPrice.Text, out decimal price) || price < 0)
                {
                    MessageBox.Show("Введите корректную цену", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    txtPrice.Focus();
                    return;
                }

                if (!int.TryParse(txtStock.Text, out int stock) || stock < 0)
                {
                    MessageBox.Show("Введите корректное количество на складе", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    txtStock.Focus();
                    return;
                }

                if (!double.TryParse(txtRating.Text, out double rating) || rating < 0 || rating > 5)
                {
                    MessageBox.Show("Введите корректный рейтинг (0-5)", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    txtRating.Focus();
                    return;
                }

                if (!DateOnly.TryParse(txtCreatedAt.Text, out DateOnly createdAt))
                {
                    MessageBox.Show("Введите корректную дату", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    txtCreatedAt.Focus();
                    return;
                }

                if (cmbCategory.SelectedItem is not Category selectedCategory)
                {
                    MessageBox.Show("Выберите категорию", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    cmbCategory.Focus();
                    return;
                }

                if (cmbBrand.SelectedItem is not Brand selectedBrand)
                {
                    MessageBox.Show("Выберите бренд", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    cmbBrand.Focus();
                    return;
                }

               
                _product ??= new Product();

                if (_product.Id == 0)
                {
                    _product.Id = _db.Products.Any() ? _db.Products.Max(p => p.Id) + 1 : 1;
                }

                _product.Name = txtName.Text.Trim();
                _product.Description = txtDescription.Text.Trim();
                _product.Price = price;
                _product.Stock = stock;
                _product.Rating = rating;
                _product.CreatedAt = createdAt;
                _product.CategoryId = selectedCategory.Id;
                _product.BrandId = selectedBrand.Id;

                
                _product.Tags = lstTags.SelectedItems.Cast<Tag>().ToList();

              
                if (_db.Products.Any(p => p.Id == _product.Id))
                {
                    _db.Products.Update(_product);
                }
                else
                {
                    _db.Products.Add(_product);
                }

                _db.SaveChanges();

                MessageBox.Show("Товар успешно сохранен", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            var textBox = sender as TextBox;

            
            if (textBox.Name == "txtPrice" || textBox.Name == "txtRating")
            {
                if (!char.IsDigit(e.Text, 0) && e.Text != "." && e.Text != "-")
                {
                    e.Handled = true;
                    return;
                }

              
                if ((e.Text == "." && textBox.Text.Contains('.')) ||
                    (e.Text == "-" && textBox.Text.Contains('-')))
                {
                    e.Handled = true;
                }
            }
            else if (textBox.Name == "txtStock")
            {
               
                if (!char.IsDigit(e.Text, 0))
                {
                    e.Handled = true;
                }
            }
        }
    }
}

