﻿using DBMigration.WPF.Views;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DBMigration.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow? Current { get; private set; }
        public MainWindow()
        {
            InitializeComponent();
            Current = this;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Frm_MainFrame.Navigate(new MigrationPage());
        }
    }
}