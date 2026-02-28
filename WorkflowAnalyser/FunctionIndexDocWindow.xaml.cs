using System.Windows;

namespace WorkflowAnalyser
{
    public partial class FunctionIndexDocWindow : Window
    {
        public FunctionIndexDocWindow(string header, string body)
        {
            InitializeComponent();
            DataContext = new FunctionDocVm(header, body);
        }
    }

    public sealed class FunctionDocVm
    {
        public string Header { get; }
        public string Body { get; }

        public FunctionDocVm(string header, string body)
        {
            Header = header;
            Body = body;
        }
    }
}
