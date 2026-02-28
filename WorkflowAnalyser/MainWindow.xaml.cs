
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

            var docPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FunctionIndexDescriptions.json");

            FunctionIndexDocs.Load(docPath);
        }

        private void OpenGraph_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!int.TryParse(StartIndexBox.Text.Trim(), out var startIndex))
                    throw new Exception("Start Index ist keine Zahl.");

                if (!int.TryParse(DepthBoxIncoming.Text.Trim(), out var depthIncoming) || depthIncoming < 0)
                    throw new Exception("Depth-Incoming muss >= 0 sein.");

                if (!int.TryParse(DepthBoxOutgoing.Text.Trim(), out var depthOutgoing) || depthOutgoing < 0)
                    throw new Exception("Depth-Outgoing muss >= 0 sein.");

                if (depthIncoming == 0 && depthOutgoing == 0)
                    throw new Exception("Mindestens eine Richtung muss eine Tiefe > 0 haben.");

                var (graph, _) = WorkflowGraphBuilder.BuildBidirectionalSubgraphFromJsonFile(
                    jsonWorkflowsPath,
                    startIndex,
                    depthIncoming,
                    depthOutgoing);

                var win = new GraphWindow(graph)
                {
                    Title = $"Workflow {startIndex} (Incoming {depthIncoming}, Outgoing {depthOutgoing})"
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