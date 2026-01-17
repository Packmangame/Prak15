using Prakt15.Models;
using Prakt15.Services;
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



namespace Prakt15
{
    /// <summary>
    /// Логика взаимодействия для ManageTagsWindow.xaml
    /// </summary>
    public partial class ManageTagsWindow : Window
    {
        private readonly Prak15Context _db = DBService.Instance.Context;
        private ObservableCollection<Tag> _tags = new ObservableCollection<Tag>();

        public ManageTagsWindow()
        {
            InitializeComponent();
            Loaded += ManageTagsWindow_Loaded;
        }

        private void ManageTagsWindow_Loaded(object sender, RoutedEventArgs e)
        {
            LoadTags();
        }

        private void LoadTags()
        {
            try
            {
                _tags.Clear();

                var tags = _db.Tags
                    .OrderBy(t => t.Name)
                    .ToList();

                foreach (var tag in tags)
                {
                    _tags.Add(tag);
                }

                lstTags.ItemsSource = _tags;
                txtNewTag.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки тегов: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string tagName = txtNewTag.Text.Trim();

                if (string.IsNullOrWhiteSpace(tagName))
                {
                    MessageBox.Show("Введите название тега", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtNewTag.Focus();
                    return;
                }

                if (tagName.Length > 255)
                {
                    MessageBox.Show("Название тега слишком длинное", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtNewTag.Focus();
                    return;
                }

                bool exists = _db.Tags.Any(t => t.Name.ToLower() == tagName.ToLower());

                if (exists)
                {
                    MessageBox.Show("Тег с таким названием уже существует", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtNewTag.SelectAll();
                    txtNewTag.Focus();
                    return;
                }

                var newTag = new Tag
                {
                    Id = _db.Tags.Any() ? _db.Tags.Max(t => t.Id) + 1 : 1,
                    Name = tagName
                };

                _db.Tags.Add(newTag);
                _db.SaveChanges();

                MessageBox.Show($"Тег \"{tagName}\" добавлен", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                txtNewTag.Clear();
                LoadTags();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag != null &&
                int.TryParse(button.Tag.ToString(), out int tagId))
            {
                var tag = _db.Tags.FirstOrDefault(t => t.Id == tagId);
                if (tag == null) return;

                string input = Microsoft.VisualBasic.Interaction.InputBox(
                    "Введите новое название:",
                    "Изменение тега",
                    tag.Name);

                if (!string.IsNullOrWhiteSpace(input))
                {
                    bool exists = _db.Tags.Any(t =>
                        t.Name.ToLower() == input.ToLower() && t.Id != tag.Id);

                    if (exists)
                    {
                        MessageBox.Show("Тег с таким названием уже существует", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    tag.Name = input;
                    _db.SaveChanges();
                    LoadTags();
                }
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag != null &&
                int.TryParse(button.Tag.ToString(), out int tagId))
            {
                var tag = _db.Tags.FirstOrDefault(t => t.Id == tagId);
                if (tag == null) return;

                // Проверка использования тега в товарах
                bool hasProducts = _db.Products.Any(p => p.Tags.Any(t => t.Id == tagId));

                if (hasProducts)
                {
                    MessageBox.Show("Нельзя удалить тег, который используется в товарах",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var result = MessageBox.Show(
                    $"Удалить тег \"{tag.Name}\"?",
                    "Подтверждение",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        _db.Tags.Remove(tag);
                        _db.SaveChanges();
                        LoadTags();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                lstTags.ItemsSource = _tags;
                return;
            }

            string searchText = txtSearch.Text.ToLower();
            var filtered = _tags.Where(t =>
                t.Name.ToLower().Contains(searchText))
                .ToList();

            lstTags.ItemsSource = filtered;
        }

        private void TxtNewTag_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BtnAdd_Click(sender, e);
            }
        }

        private void BtnClearSearch_Click(object sender, RoutedEventArgs e)
        {
            txtSearch.Clear();
            lstTags.ItemsSource = _tags;
        }
    }

}

