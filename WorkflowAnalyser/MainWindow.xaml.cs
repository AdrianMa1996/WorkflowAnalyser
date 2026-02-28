
using Microsoft.Win32;
using System.IO;
using System.Windows;

namespace WorkflowAnalyser
{
    public partial class MainWindow : Window
    {
        public string jsonWorkflowsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "workflows.json");
        public MainWindow()
        {
            InitializeComponent();
        }

        private void OpenGraph_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!int.TryParse(StartIndexBox.Text.Trim(), out var startIndex))
                    throw new Exception("Start Index ist keine Zahl.");

                if (!int.TryParse(DepthBox.Text.Trim(), out var depth) || depth < 1)
                    throw new Exception("Depth muss >= 1 sein.");

                var (graph, _) = WorkflowGraphBuilder.BuildSubgraphFromJsonFile(jsonWorkflowsPath, startIndex, depth);

                var win = new GraphWindow(graph)
                {
                    Title = $"Workflow Graph {startIndex} (Depth {depth})"
                };
                win.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}