using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using Microsoft.EntityFrameworkCore;
using Zadanie3TovarV1.ModelsDB;

namespace Zadanie3TovarV1
{
    public partial class Avtorizacia : Window
    {
        private int _attemptCount = 1;
        private DispatcherTimer _timer;
        private string _currentCaptcha;

        public Avtorizacia()
        {
            InitializeComponent();
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(10);
            _timer.Tick += Timer_Tick;
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            tbLogin.Focus();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            stackPanel.IsEnabled = true;
            _timer.Stop();
        }

        private void GenerateCaptcha()
        {
            string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghjkmnpqrs"+"tuvwxyz123456789";

            Random rnd = new Random();
            _currentCaptcha = "";

            for (int i = 0; i < 4; i++)
            {
                _currentCaptcha += chars[rnd.Next(chars.Length)];
            }

            txtCaptcha.Text = _currentCaptcha;
            gridCaptcha.Visibility = Visibility.Visible;
            tbCaptchaInput.Text = "";
            txtCaptcha.LayoutTransform = new RotateTransform(rnd.Next(-15, 15));
            lineCapcha.X1 = 10;
            lineCapcha.Y1 = rnd.Next(10,40); 
            lineCapcha.X2 = 280;
            lineCapcha.Y2 = rnd.Next(10, 40);
        }

        private void btnEnter_Click(object sender, RoutedEventArgs e)
        {
            using (Trade1Context _db = new Trade1Context())
            {
                var user = _db.Users
                    .Include(u => u.UserRoleNavigation)
                    .FirstOrDefault(u => u.UserLogin == tbLogin.Text
                                      && u.UserPassword == tbPassword.Password);

                // Проверка каптчи если нужно
                if (gridCaptcha.Visibility == Visibility.Visible)
                {
                    if (tbCaptchaInput.Text != _currentCaptcha)
                    {
                        MessageBox.Show("Неверная каптча!");
                        stackPanel.IsEnabled = false;
                        _timer.Start();
                        return;
                    }
                }

                if (user != null)
                {
                    Data.CurrentUser = user;
                    Data.IsLoggedIn = true;

                    MainWindow mainWindow = new MainWindow();
                    mainWindow.Show();
                    this.Close();
                }
                else
                {
                    if (_attemptCount >= 1)
                    {
                        MessageBox.Show("Неверный логин или пароль!");
                        GenerateCaptcha();

                        if (_attemptCount >= 2)
                        {
                            stackPanel.IsEnabled = false;
                            _timer.Start();
                        }
                        _attemptCount++;
                    }
                }
            }
        }

        private void btnGuest_Click(object sender, RoutedEventArgs e)
        {
            Data.IsLoggedIn = true;
            Data.CurrentUser = null;

            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }
    }
}