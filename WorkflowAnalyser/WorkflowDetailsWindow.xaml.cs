using System.Windows;

namespace WorkflowAnalyser
{
    public partial class WorkflowDetailsWindow : Window
    {
        public WorkflowDetailsWindow(Workflow wf)
        {
            InitializeComponent();
            DataContext = new WorkflowDetailsVm(wf);
        }
    }

    public sealed class WorkflowDetailsVm
    {
        public int Index { get; }
        public List<KeyValuePair<string, string>> Rows { get; } = new();

        public WorkflowDetailsVm(Workflow wf)
        {
            Index = wf.Index;

            Add(wf.Index.ToString(), "Index");
            Add(wf.Name, "Name");
            Add(wf.Type, "Type");
            Add(wf.Category, "Category");
            Add(wf.NextWorkflows, "NextWorkflows");
            Add(wf.FunctionIndex.ToString(), "FunctionIndex");
            Add(wf.FunctionParameters, "FunctionParameters");
            Add(wf.Changed, "Changed");
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
