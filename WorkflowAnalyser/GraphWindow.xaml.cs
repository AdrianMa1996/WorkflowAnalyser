using System.Windows;
using Microsoft.Msagl.Drawing;
using Microsoft.Msagl.GraphViewerGdi;

namespace WorkflowAnalyser
{
    public partial class GraphWindow : Window
    {
        private readonly GViewer _viewer = new();

        public GraphWindow(Graph graph)
        {
            InitializeComponent();

            Host.Child = _viewer;

            _viewer.ToolBarIsVisible = true;
            _viewer.LayoutAlgorithmSettingsButtonVisible = false;
            _viewer.MouseClick += Viewer_MouseClick;

            _viewer.Graph = graph;
        }

        private void Viewer_MouseClick(object? sender, System.Windows.Forms.MouseEventArgs e)
        {
            var obj = _viewer.ObjectUnderMouseCursor?.DrawingObject;
            if (obj is Microsoft.Msagl.Drawing.Node node && node.UserData is Workflow wf)
            {
                var details = new WorkflowDetailsWindow(wf)
                {
                    Owner = this
                };
                details.Show();
            }
        }
    }
}
