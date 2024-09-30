using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
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

namespace DBMigration.WPF.Views
{
    /// <summary>
    /// Interaction logic for MigrationPage.xaml
    /// </summary>
    public partial class MigrationPage : Page
    {
        private enum RedGreen { Red, Green }

        private DbContext? _context;

        private string _connectionString = string.Empty;
        public MigrationPage()
        {
            InitializeComponent();
        }

        private void ChangeCanvaColor(RedGreen redGreen)
        {
            Canvas canvasToDisplay = redGreen == RedGreen.Green ? Cnv_Green : Cnv_Red;
            Canvas canvasToHide = redGreen == RedGreen.Green ? Cnv_Red : Cnv_Green;
            canvasToDisplay.Visibility = Visibility.Visible;
            canvasToHide.Visibility = Visibility.Collapsed;
        }
        private void CheckContext()
        {
            if (_context == null)
                throw new Exception("_context is null, please select a valid DLL");
        }
        private void CheckConnectionString()
        {
            if (string.IsNullOrEmpty(_connectionString))
                throw new Exception("The connection string is null or empty, please provide a valid connection string.");
        }

        private async Task CheckDataBaseConnectionAsync()
        {
            CheckContext();
            CheckConnectionString();

            bool connectionTest = true;

            Regex regex = new Regex(@"Initial Catalog=(\w)+;");
            string cnxStr = regex.Replace(_connectionString, "Initial Catalog=master;");
            _context!.Database.SetConnectionString(cnxStr);
            if (!await _context!.Database.CanConnectAsync())
            {
                ChangeCanvaColor(RedGreen.Red);
                connectionTest = false;
            }
            else
                ChangeCanvaColor(RedGreen.Green);

            _context.Database.SetConnectionString(_connectionString);

            if(!connectionTest)
            throw new Exception("Connection failed to the server, please check your DB server or your connection string");
        }

        private async void Btn_Migration_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await CheckDataBaseConnectionAsync();
                var response = MessageBox.Show($"Do you want to run a migration for the database?",
                    "Migration?", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (response == MessageBoxResult.Yes)
                {
                    if (Chx_LastVersion.IsChecked == true)
                        await _context!.Database.MigrateAsync();
                    else
                    {
                        var migrator = _context!.GetService<IMigrator>();
                        await migrator.MigrateAsync(Cbx_MigrationVersions.Text);
                    }
                    MessageBox.Show("Migration completed successfully");

                }
            }
            catch (Exception? ex)
            {
                string msg = string.Empty;
                while (ex != null)
                {
                    msg += $"{ex.Message} \n";
                    ex = ex.InnerException;
                }
                MessageBox.Show(msg, "Erreur", MessageBoxButton.OK, MessageBoxImage.Stop);
            }
        }

        private void Chx_LastVersion_Checked(object sender, RoutedEventArgs e)
        {
            Cbx_MigrationVersions.Visibility = Chx_LastVersion.IsChecked == true ? Visibility.Collapsed : Visibility.Visible;
        }

        private void Chx_LastVersion_Unchecked(object sender, RoutedEventArgs e)
        {
            Chx_LastVersion_Checked(sender, e);
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Chx_LastVersion.IsChecked = true;
            Btn_DataBase.IsEnabled = false;
            SetItemsEnabled(false);
            ChangeCanvaColor(RedGreen.Red);
        }

        private void SetItemsEnabled(bool enabled)
        {
            Chx_LastVersion.IsEnabled = enabled;
            Cbx_MigrationVersions.IsEnabled = enabled;
            Btn_Migration.IsEnabled = enabled;
        }

        private void Btn_Select_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = "DLL files (*.dll)|*.dll";
            fileDialog.Title = "Select DbContext Librery...";
            fileDialog.Multiselect = false; // Set to enabled or never mention this line for single file select

            var success = fileDialog.ShowDialog();
            if (success ?? false)
            {
                Tbx_Path.Text = fileDialog.FileName;        // Use fileDialog.FileName for a single file - no 's' at the end
                LoadMigrations();
            }
        }

        private void LoadMigrations()
        {
            // Get the DLL path from the TextBox
            string dllPath = Tbx_Path.Text;
            if (string.IsNullOrEmpty(dllPath) || !File.Exists(dllPath))
            {
                MessageBox.Show("Please select a valid DbContext DLL.");
                return;
            }

            // Load the assembly from the selected DLL file
            Assembly dbAssembly = Assembly.LoadFrom(dllPath);

            // Find the DbContext type in the assembly
            Type? dbContextType = dbAssembly.GetTypes().FirstOrDefault(t => typeof(DbContext).IsAssignableFrom(t));

            if (dbContextType == null)
            {
                MessageBox.Show("No DbContext found in the selected DLL.");
                Tbx_Path.Text = null;
                return;
            }

            _context = (DbContext)Activator.CreateInstance(dbContextType)!;

            Btn_DataBase.IsEnabled = true;
        }

        private async void Btn_DataBase_Click(object sender, RoutedEventArgs e)
        {
            if (_context == null)
            {
                MessageBox.Show("_context is null, please select a valid DLL");
                return;
            }

            _connectionString = ConnectionInfo.GetConnectionString();

            if (!_connectionString.IsNullOrEmpty())
            {
                try
                {
                    _context.Database.SetConnectionString(_connectionString);
                    await CheckDataBaseConnectionAsync();

                    var migrations = _context.Database.GetMigrations();

                    // Bind migrations to the ComboBox
                    if (migrations != null)
                    {
                        SetItemsEnabled(true);
                        Cbx_MigrationVersions.ItemsSource = migrations;
                        Cbx_MigrationVersions.SelectedItem = migrations.LastOrDefault();
                    }
                    else
                    {
                        MessageBox.Show("No migrations found.");
                    }
                }
                catch (Exception? ex)
                {
                    string msg = string.Empty;
                    while (ex != null)
                    {
                        msg += $"{ex.Message} \n";
                        ex = ex.InnerException;
                    }
                    MessageBox.Show(msg, "Erreur", MessageBoxButton.OK, MessageBoxImage.Stop);
                }
            }
        }
    }
}
