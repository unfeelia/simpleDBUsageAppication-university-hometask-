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
using System.Windows.Shapes;

namespace dbApplication
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void logInButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                dbRequestsHandler.verifyUser(loginNameBox.Text, loginPasswordBox.Text);
                UserWindow userWindow = new UserWindow();
                userWindow.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void registrationButton_Click(object sender, RoutedEventArgs e)
        {
            if (registrationPasswordBox.Text == registrationRepeatedPasswordBox.Text)
            {
                dbRequestsHandler.addUser(registrationNameBox.Text, registrationPasswordBox.Text, registrationEmailBox.Text);
            }
            else
            {
                MessageBox.Show("Passwords don't match");
            }
        }
    }
}
