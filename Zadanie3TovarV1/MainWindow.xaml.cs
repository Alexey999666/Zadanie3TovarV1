using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using Zadanie3TovarV1.ModelsDB;

namespace Zadanie3TovarV1
{
    public partial class MainWindow : Window
    {
        private Trade1Context _db;
        private List<Product> _allProducts;
        private string _currentSearch = "";
        private string _currentManufacturer = "";
        private int _currentSort = 0;

        public MainWindow()
        {
            InitializeComponent();
            _db = new Trade1Context();

            // Настройка прав доступа
            if (Data.CurrentUser != null)
            {
                tbUserInfo.Text = $"{Data.CurrentUser.UserSurname} {Data.CurrentUser.UserName} {Data.CurrentUser.UserPatronymic}";

                if (Data.CurrentUser.UserRoleNavigation?.RoleName != "Администратор")
                {
                    btnAdd.IsEnabled = false;
                    btnEdit.IsEnabled = false;
                    btnDelete.IsEnabled = false;
                }
            }
            else
            {
                tbUserInfo.Text = "Гость";
                btnAdd.IsEnabled = false;
                btnEdit.IsEnabled = false;
                btnDelete.IsEnabled = false;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadData();
            LoadManufacturers();

        }

        private void Window_Closed(object sender, EventArgs e)
        {
            _db?.Dispose();
        }

        private void LoadData()
        {
            try
            {
                _db.ChangeTracker.Clear();
                _allProducts = _db.Products.ToList();
                ApplyFilters();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}");
            }
        }

        private void LoadManufacturers()
        {
            var manufacturers = _db.Products
                .Select(p => p.ProductManufacturer)
                .Distinct()
                .OrderBy(m => m)
                .ToList();

            cmbManufacturer.Items.Clear();
            cmbManufacturer.Items.Add("Все производители");

            foreach (var m in manufacturers)
            {
                cmbManufacturer.Items.Add(m);
            }

            cmbManufacturer.SelectedIndex = 0;
        }

        private void ApplyFilters()
        {
            if (_allProducts == null) return;

            var filtered = _allProducts.AsEnumerable();

            // Поиск
            if (!string.IsNullOrWhiteSpace(_currentSearch))
            {
                filtered = filtered.Where(p =>
                    (p.ProductName?.Contains(_currentSearch, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (p.ProductDescription?.Contains(_currentSearch, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (p.ProductManufacturer?.Contains(_currentSearch, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (p.ProductCategory?.Contains(_currentSearch, StringComparison.OrdinalIgnoreCase) ?? false));
            }

            // Фильтр по производителю
            if (!string.IsNullOrWhiteSpace(_currentManufacturer) && _currentManufacturer != "Все производители")
            {
                filtered = filtered.Where(p => p.ProductManufacturer == _currentManufacturer);
            }

            // Сортировка
            switch (_currentSort)
            {
                case 1: // по возрастанию
                    filtered = filtered.OrderBy(p => p.ProductCost);
                    break;
                case 2: // по убыванию
                    filtered = filtered.OrderByDescending(p => p.ProductCost);
                    break;
            }

            var filteredList = filtered.ToList();
            listViewProducts.ItemsSource = filteredList;

            // Общее количество товаров такого же типа (с таким же производителем)
            int totalSameType = _allProducts.Count;
            if (!string.IsNullOrWhiteSpace(_currentManufacturer) && _currentManufacturer != "Все производители")
            {
                totalSameType = _allProducts.Count(p => p.ProductManufacturer == _currentManufacturer);
            }

            tbCountInfo.Text = $"Показано {filteredList.Count} из {totalSameType}";
        }

        private void tbSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            _currentSearch = tbSearch.Text;
            ApplyFilters();
        }

        private void cmbManufacturer_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbManufacturer.SelectedItem != null)
            {
                _currentManufacturer = cmbManufacturer.SelectedItem.ToString();
                ApplyFilters();
            }
        }

        private void cmbSort_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbSort.SelectedItem != null)
            {
                var item = (ComboBoxItem)cmbSort.SelectedItem;
                _currentSort = int.Parse(item.Tag.ToString());
                ApplyFilters();
            }
        }

        private void listViewProducts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            Data.CurrentProduct = null;
            AddEditProduct addWindow = new AddEditProduct();
            addWindow.Owner = this;
            addWindow.ShowDialog();
            LoadData();
            LoadManufacturers();
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (listViewProducts.SelectedItem == null)
            {
                MessageBox.Show("Выберите товар для редактирования");
                return;
            }

            Data.CurrentProduct = (Product)listViewProducts.SelectedItem;
            AddEditProduct editWindow = new AddEditProduct();
            editWindow.Owner = this;
            editWindow.ShowDialog();
            LoadData();
            LoadManufacturers();
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (listViewProducts.SelectedItem == null)
            {
                MessageBox.Show("Выберите товар для удаления");
                return;
            }

            var product = (Product)listViewProducts.SelectedItem;

            // Проверка, есть ли товар в заказах
            var inOrders = _db.OrderProducts.Any(op => op.ProductArticleNumber == product.ProductArticleNumber);

            if (inOrders)
            {
                MessageBox.Show("Нельзя удалить товар, который присутствует в заказах");
                return;
            }

            var result = MessageBox.Show("Удалить товар?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    _db.Products.Remove(product);
                    _db.SaveChanges();
                    LoadData();
                    LoadManufacturers();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка удаления: {ex.Message}");
                }
            }
        }

        private void btnLogout_Click(object sender, RoutedEventArgs e)
        {
            Data.IsLoggedIn = false;
            Data.CurrentUser = null;

            Avtorizacia authWindow = new Avtorizacia();
            authWindow.Show();
            this.Close();
        }

        private void listViewProducts_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (listViewProducts.SelectedItem == null) return;

            // Проверка прав (только администратор)
            if (Data.CurrentUser != null && Data.CurrentUser.UserRoleNavigation?.RoleName == "Администратор")
            {
                Data.CurrentProduct = (Product)listViewProducts.SelectedItem;
                AddEditProduct editWindow = new AddEditProduct();
                editWindow.Owner = this;
                editWindow.ShowDialog();
                LoadData();
                LoadManufacturers();
            }
        }
    }
}