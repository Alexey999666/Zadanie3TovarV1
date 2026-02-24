using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using Zadanie3TovarV1.ModelsDB;

namespace Zadanie3TovarV1
{
    public partial class AddEditProduct : Window
    {
        private Trade1Context _db;
        private Product _currentProduct;

        public AddEditProduct()
        {
            InitializeComponent();
            _db = new Trade1Context();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadComboBoxes();

            if (Data.CurrentProduct == null) // Добавление
            {
                this.Title = "Добавление товара";
                btnSave.Content = "Добавить";

                // Генерация нового артикула
                var lastProduct = _db.Products.OrderBy(p => p.ProductArticleNumber).LastOrDefault();
                if (lastProduct != null)
                {
                    int lastNumber = 0;
                    if (int.TryParse(lastProduct.ProductArticleNumber, out lastNumber))
                    {
                        txtArticleNumber.Text = (lastNumber + 1).ToString();
                    }
                    else
                    {
                        txtArticleNumber.Text = "1";
                    }
                }
                else
                {
                    txtArticleNumber.Text = "1";
                }

                _currentProduct = new Product();
            }
            else // Редактирование
            {
                this.Title = "Редактирование товара";
                btnSave.Content = "Сохранить";

                _currentProduct = _db.Products.Find(Data.CurrentProduct.ProductArticleNumber);

                if (_currentProduct != null)
                {
                    txtArticleNumber.Text = _currentProduct.ProductArticleNumber;
                    txtName.Text = _currentProduct.ProductName;
                    cmbCategory.Text = _currentProduct.ProductCategory;
                    txtQuantity.Text = _currentProduct.ProductQuantityInStock.ToString();
                    txtUnit.Text = _currentProduct.ProductUnitMeasurement;
                    txtSupplier.Text = _currentProduct.ProductSupplier;
                    cmbManufacturer.Text = _currentProduct.ProductManufacturer;
                    txtCost.Text = _currentProduct.ProductCost.ToString("F2");
                    txtDiscountMax.Text = _currentProduct.ProductDiscountAmountMax.ToString();
                    txtDiscount.Text = _currentProduct.ProductDiscountAmount?.ToString() ?? "";
                    cmbStatus.Text = _currentProduct.ProductStatus;
                    txtDescription.Text = _currentProduct.ProductDescription;
                }
            }

            this.DataContext = _currentProduct;
        }

        private void LoadComboBoxes()
        {
            // Категории
            var categories = _db.Products
                .Select(p => p.ProductCategory)
                .Distinct()
                .OrderBy(c => c)
                .ToList();

            foreach (var cat in categories)
            {
                cmbCategory.Items.Add(cat);
            }

            // Производители
            var manufacturers = _db.Products
                .Select(p => p.ProductManufacturer)
                .Distinct()
                .OrderBy(m => m)
                .ToList();

            foreach (var man in manufacturers)
            {
                cmbManufacturer.Items.Add(man);
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder errors = new StringBuilder();

            if (string.IsNullOrWhiteSpace(txtName.Text))
                errors.AppendLine("Введите наименование");
            if (string.IsNullOrWhiteSpace(cmbCategory.Text))
                errors.AppendLine("Введите категорию");
            if (string.IsNullOrWhiteSpace(txtQuantity.Text) || !int.TryParse(txtQuantity.Text, out int quantity) || quantity < 0)
                errors.AppendLine("Количество должно быть целым неотрицательным числом");
            if (string.IsNullOrWhiteSpace(txtUnit.Text))
                errors.AppendLine("Введите единицу измерения");
            if (string.IsNullOrWhiteSpace(txtSupplier.Text))
                errors.AppendLine("Введите поставщика");
            if (string.IsNullOrWhiteSpace(cmbManufacturer.Text))
                errors.AppendLine("Введите производителя");
            if (string.IsNullOrWhiteSpace(txtCost.Text) || !decimal.TryParse(txtCost.Text.Replace('.', ','), out decimal cost) || cost < 0)
                errors.AppendLine("Стоимость должна быть неотрицательным числом");
            if (string.IsNullOrWhiteSpace(txtDiscountMax.Text) || !byte.TryParse(txtDiscountMax.Text, out byte maxDiscount) || maxDiscount < 0 || maxDiscount > 100)
                errors.AppendLine("Максимальная скидка от 0 до 100");
            if (!string.IsNullOrWhiteSpace(txtDiscount.Text))
            {
                if (!byte.TryParse(txtDiscount.Text, out byte discount) || discount < 0 || discount > 100)
                    errors.AppendLine("Текущая скидка от 0 до 100");
            }
            if (string.IsNullOrWhiteSpace(cmbStatus.Text))
                errors.AppendLine("Введите статус");
            if (string.IsNullOrWhiteSpace(txtDescription.Text))
                errors.AppendLine("Введите описание");

            if (errors.Length > 0)
            {
                MessageBox.Show(errors.ToString());
                return;
            }

            try
            {
                // Заполнение полей
                _currentProduct.ProductArticleNumber = txtArticleNumber.Text;
                _currentProduct.ProductName = txtName.Text;
                _currentProduct.ProductCategory = cmbCategory.Text;
                _currentProduct.ProductQuantityInStock = int.Parse(txtQuantity.Text);
                _currentProduct.ProductUnitMeasurement = txtUnit.Text;
                _currentProduct.ProductSupplier = txtSupplier.Text;
                _currentProduct.ProductManufacturer = cmbManufacturer.Text;
                _currentProduct.ProductCost = decimal.Parse(txtCost.Text.Replace('.', ','));
                _currentProduct.ProductDiscountAmountMax = byte.Parse(txtDiscountMax.Text);
                _currentProduct.ProductDiscountAmount = string.IsNullOrWhiteSpace(txtDiscount.Text) ? null : (byte?)byte.Parse(txtDiscount.Text);
                _currentProduct.ProductStatus = cmbStatus.Text;
                _currentProduct.ProductDescription = txtDescription.Text;
                _currentProduct.ProductPhoto = null; // Фото не реализуем для простоты

                if (Data.CurrentProduct == null) // Добавление
                {
                    _db.Products.Add(_currentProduct);
                }
                else // Редактирование
                {
                    _db.Entry(_currentProduct).State = EntityState.Modified;
                }

                _db.SaveChanges();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}");
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        // Валидация ввода чисел
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void DecimalValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9.,]+");
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}