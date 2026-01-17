using Microsoft.VisualBasic;
using Prakt15.Models;
using Prakt15.Services;


//using Prakt15.Models;
//using Prakt15.Services;
using Prakt15.Validation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Логика взаимодействия для ManageCategoriesWindow.xaml
    /// </summary>
    public partial class ManageCategoriesWindow : Window
    {
        private readonly Prak15Context _db = DBService.Instance.Context;
        private ObservableCollection<Category> _categories = new ObservableCollection<Category>();

        public ManageCategoriesWindow()
        {
            InitializeComponent();
            Loaded += ManageCategoriesWindow_Loaded;
        }

        private void ManageCategoriesWindow_Loaded(object sender, RoutedEventArgs e)
        {
            LoadCategories();
        }

        private void LoadCategories()
        {
            try
            {
                _categories.Clear();
                var categories = _db.Categories
                    .OrderBy(c => c.Name)
                    .ToList();

                foreach (var category in categories)
                {
                    _categories.Add(category);
                }

                lstCategories.ItemsSource = _categories;
                txtNewCategory.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки категорий: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string categoryName = txtNewCategory.Text.Trim();

                if (string.IsNullOrWhiteSpace(categoryName))
                {
                    MessageBox.Show("Введите название категории", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtNewCategory.Focus();
                    return;
                }

                if (categoryName.Length > 255)
                {
                    MessageBox.Show("Название категории слишком длинное", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtNewCategory.Focus();
                    return;
                }

                bool exists = _db.Categories.Any(c =>
                    c.Name.ToLower() == categoryName.ToLower());

                if (exists)
                {
                    MessageBox.Show("Категория с таким названием уже существует", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtNewCategory.SelectAll();
                    txtNewCategory.Focus();
                    return;
                }

                var newCategory = new Category
                {
                    Id = _db.Categories.Any() ? _db.Categories.Max(c => c.Id) + 1 : 1,
                    Name = categoryName
                };

                _db.Categories.Add(newCategory);
                _db.SaveChanges();

                MessageBox.Show($"Категория \"{categoryName}\" успешно добавлена", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                txtNewCategory.Clear();
                LoadCategories();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении категории: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is Button button && button.Tag != null &&
                    int.TryParse(button.Tag.ToString(), out int categoryId))
                {
                    var category = _db.Categories.FirstOrDefault(c => c.Id == categoryId);
                    if (category == null) return;

                    string input = Microsoft.VisualBasic.Interaction.InputBox(
                        "Введите новое название:",
                        "Изменение категории",
                        category.Name);

                    if (!string.IsNullOrWhiteSpace(input))
                    {
                        bool exists = _db.Categories.Any(c =>
                            c.Name.ToLower() == input.ToLower() && c.Id != category.Id);

                        if (exists)
                        {
                            MessageBox.Show("Категория с таким названием уже существует", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }

                        category.Name = input;
                        _db.SaveChanges();
                        LoadCategories();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при редактировании: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is Button button && button.Tag != null &&
                    int.TryParse(button.Tag.ToString(), out int categoryId))
                {
                    var category = _db.Categories.FirstOrDefault(c => c.Id == categoryId);
                    if (category == null) return;

                    bool hasProducts = _db.Products.Any(p => p.CategoryId == categoryId);

                    if (hasProducts)
                    {
                        MessageBox.Show("Нельзя удалить категорию, в которой есть товары",
                            "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    var result = MessageBox.Show(
                        $"Удалить категорию \"{category.Name}\"?",
                        "Подтверждение",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning);

                    if (result == MessageBoxResult.Yes)
                    {
                        _db.Categories.Remove(category);
                        _db.SaveChanges();
                        LoadCategories();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                lstCategories.ItemsSource = _categories;
                return;
            }

            string searchText = txtSearch.Text.ToLower();
            var filtered = _categories.Where(c =>
                c.Name.ToLower().Contains(searchText)).ToList();

            lstCategories.ItemsSource = filtered;
        }
    }
}
