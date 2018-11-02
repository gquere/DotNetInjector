using System;
using System.Windows.Forms;


namespace DotNetInjector
{
    public partial class Form1 : Form
    {
        private AssemblyHandler assemblyToInject = new AssemblyHandler();

        public Form1()
        {
            InitializeComponent();
        }

        private void LoadAssembly(String fileName)
        {
            if (assemblyToInject.LoadAssembly(fileName) != true)
            {
                return;
            }
            assemblyToInject.ReadMethodsToTree(this.treeView1);

            richTextBox1.Clear();
            richTextBox1.AppendText("Assembly Loaded." + Environment.NewLine);
            richTextBox1.AppendText(assemblyToInject.GetRuntime() + " dependant." + Environment.NewLine);          
        }


        /* GETTERS *******************************************************************************/
        public String GetMethodParentName()
        {
            if (treeView1.SelectedNode == null)
                return null;

            if (treeView1.SelectedNode.Parent == null)
                return null;
            
            return treeView1.SelectedNode.Parent.Text;
        }

        public String GetMethodName()
        {
            if (treeView1.SelectedNode == null)
                return null;

            return treeView1.SelectedNode.Text;
        }


        /* FORM OBJECTS **************************************************************************/
        private void button1_Click(object sender, EventArgs e)
        {
            richTextBox.Clear();

            OpenFileDialog open = new OpenFileDialog
            {
                Filter = "Executable or DLL | *.exe; *.dll"
            };

            if (open.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = open.FileName;
                LoadAssembly(open.FileName);
            }
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            richTextBox.Clear();

            /* return if the node has no parent */
            if (treeView1.SelectedNode.Parent == null)
                return;

            String text = assemblyToInject.DecompileMethod(GetMethodParentName(), GetMethodName());
            if (text != null)
                richTextBox.AppendText(text);
        }

        private void treeView1_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            String parentName = GetMethodParentName();
            String methodName = GetMethodName();

            if (parentName == null || methodName == null)
                return;

            try
            {
                PayloadCreator pl = new PayloadCreator(assemblyToInject, parentName, methodName);
                pl.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
    }
}
