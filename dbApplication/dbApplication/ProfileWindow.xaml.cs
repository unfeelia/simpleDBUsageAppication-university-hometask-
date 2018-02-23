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
    /// Логика взаимодействия для ProfileWindow.xaml
    /// </summary>
    public partial class ProfileWindow : Window
    {
        List<User> friends;
        List<User> requesters;
        List<User> allUsers;

        private void refresh()
        {
            currnetEmailBox.Text = dbRequestsHandler.selectCurrentUser().email_address.ToString();
        }
        public ProfileWindow()
        {
            InitializeComponent();
            dbRequestsHandler.selectAllFriends(out friends);
            dbRequestsHandler.selectAllRequesters(out requesters);
            dbRequestsHandler.selectAllUsers(out allUsers);

            friendsList.ItemsSource = allUsers;
            refresh();

        }

        private void submitChangesPassword_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (passwordBox.Text == repeatedPasswordBox.Text)
                {
                    dbRequestsHandler.verifyUser(passwordOldBox.Text);
                    dbRequestsHandler.updatePassword(passwordBox.Text, passwordOldBox.Text);
                }
                else
                {
                    throw new Exception("Passwords dont match");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void submitChangesEmail_Click(object sender, RoutedEventArgs e)
        {
            dbRequestsHandler.updateEmail(emailBox.Text);
            refresh();
        }
    }
}
