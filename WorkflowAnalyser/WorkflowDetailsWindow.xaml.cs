using System.Windows;

namespace WorkflowAnalyser
{
    public partial class WorkflowDetailsWindow : Window
    {
        private readonly WorkflowDetailsVm _vm;

        public WorkflowDetailsWindow(Workflow wf)
        {
            InitializeComponent();
            _vm = new WorkflowDetailsVm(wf);
            DataContext = _vm;
        }

        private void OpenFunctionDoc_Click(object sender, RoutedEventArgs e)
        {
            var win = new FunctionIndexDocWindow(_vm.FunctionDocHeader, _vm.FunctionBody)
            {
                Owner = this
            };
            win.Show();
        }
    }

    public sealed class WorkflowDetailsVm
    {
        public int Index { get; }
        public Workflow Workflow { get; }

        public string FunctionTitle { get; }         
        public string FunctionDocHeader { get; }     
        public string FunctionBody { get; }           
        public bool HasFunctionDoc { get; }

        public List<KeyValuePair<string, string>> Rows { get; } = new();

        public WorkflowDetailsVm(Workflow wf)
        {
            Workflow = wf;
            Index = wf.Index;

            Add(wf.Index.ToString(), "Index");
            Add(wf.Name, "Name");
            Add(wf.Type, "Type");
            Add(wf.Category, "Category");
            Add(wf.NextWorkflows, "NextWorkflows");
            Add(wf.FunctionIndex.ToString(), "FunctionIndex");
            Add(wf.FunctionParameters, "FunctionParameters");
            Add(wf.Changed, "Changed");

            var doc = FunctionIndexDocs.TryGet(wf.FunctionIndex);
            if (doc is null)
            {
                HasFunctionDoc = false;
                FunctionDocHeader = $"Case: {wf.FunctionIndex} — (keine Beschreibung vorhanden)";
                FunctionBody = "";
            }
            else
            {
                HasFunctionDoc = true;
                FunctionDocHeader = $"Case: {doc.FunctionIndex} — {doc.Title}";
                FunctionBody = doc.Body;
            }
        }

        private void Add(string? value, string key)
        {
            Rows.Add(new KeyValuePair<string, string>(
                key,
                value ?? string.Empty
            ));
        }
    }
}
