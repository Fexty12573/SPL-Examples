using SharpPluginLoader.Core.Resources;
using SharpPluginLoader.Core;
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

namespace FsmEditor;
/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public bool LoadFsm { get; set; }
    public MainWindow()
    {
        InitializeComponent();
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        var testFsm = ResourceManager.GetResource<Resource>(@"hm\wp\wp11\wp11_action", MtDti.Find("rAIFSM")!);
        Ensure.NotNull(testFsm);

        var fsm = new AIFSM(testFsm.Instance);

        ((EditorViewModel)NodifyEditor.DataContext).LoadFsm(fsm);
    }
}
