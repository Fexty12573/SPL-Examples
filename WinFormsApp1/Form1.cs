using System.Runtime.Loader;

namespace WinFormsApp1;

public partial class Form1 : Form
{
    private readonly List<string> _items =
    [
        "Item 1",
        "Item 2",
        "Item 3",
        "Item 4",
        "Item 5"
    ];

    public Form1()
    {
        InitializeComponent();

        comboBox1.DataSource = _items;

        var formsAssembly = typeof(Form).Assembly;
        MessageBox.Show(formsAssembly.Location);
    }

    private void button1_Click(object sender, EventArgs e)
    {
        _items.Add($"Item {_items.Count + 1}");
        comboBox1.DataSource = null;
        comboBox1.DataSource = _items;
    }
}
