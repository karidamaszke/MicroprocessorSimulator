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
using System.IO;

namespace uPSimulator
{
    /// <summary>
    /// Logika interakcji dla klasy InterruptHelp.xaml
    /// </summary>
    public partial class InterruptHelp : Window
    {
        public InterruptHelp()
        {
            InitializeComponent();
            try
            {
                int_text.Text = File.ReadAllText("interrupt_help.txt", Encoding.Unicode);
            }
            catch
            {
                MessageBox.Show("Error! File couldn't be open");
            }

            
        }
    }
}
