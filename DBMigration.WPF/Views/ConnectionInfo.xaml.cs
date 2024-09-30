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

namespace DBMigration.WPF.Views
{
    /// <summary>
    /// Interaction logic for ConnectionInfo.xaml
    /// </summary>
    public partial class ConnectionInfo : Window
    {
        private string ConnectionString { get; set; } = string.Empty;

        public static string GetConnectionString()
        {
            ConnectionInfo connectionInfo = new ConnectionInfo();
            connectionInfo.ShowDialog();
            return connectionInfo.ConnectionString;
        }

        private ConnectionInfo()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Tbx_Server.Focus();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ConnectionString = $"Data Source={Tbx_Server.Text};Initial Catalog={Tbx_DataBase.Text};User ID={Tbx_User.Text};Password={Pbx_User.Password};Encrypt=True;TrustServerCertificate=True;";
            this.Close();
        }
    }
}
