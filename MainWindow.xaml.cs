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

namespace SplitHive
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            try
            {
                DatabaseHelper.InitializeDatabase();  // Initialize database
            }
            catch (Exception ex)
            {
                MessageBox.Show("DB Error: " + ex.Message);
            }

            // Load the LoginPage into the Frame at app start
            MainFrame.Content = new LoginPage();

        }


    }
}