using System;
using System.Windows.Forms;


namespace DotNetInjector
{
    public partial class PayloadCreator : Form
    {
        private AssemblyHandler assemblyToInject;
        private String typeNameToInject;
        private String methodNameToInject;


        /* CONSTRUCTOR ***************************************************************************/
        public PayloadCreator(AssemblyHandler assemblyHandlerToInject, String FormParentName, String FormMethodName)
        {
            InitializeComponent();
            assemblyToInject = assemblyHandlerToInject;
            typeNameToInject = FormParentName;
            methodNameToInject = FormMethodName;

            if (assemblyToInject.CheckIfMethodIsInjectable(typeNameToInject, methodNameToInject) == false)
                throw new Exception("Can't inject in this method");
        }


        /* UTILS *********************************************************************************/
        private void SaveAssembly()
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Title = "Save to :",
                Filter = "Executable | *.exe | DLL | *.dll"
            })
            {
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    assemblyToInject.SaveAssembly(saveFileDialog.FileName);
                }
            }
        }

        /* PAYLOAD CREATION AND INJECTION *******************************************************/
        private void CompileAndInject()
        {
            Compiler compiler = new Compiler();

            string[] references = { "System.dll", "System.Core.dll", "mscorlib.dll", "System.Windows.Forms.dll" };
            compiler.SetReferences(references);

            String outputFile = "tmpres.exe";
            compiler.SetOutput(outputFile);

            /* compile the user's code */
            try
            {
                compiler.Compile(richTextBox1.Text);

            }
            catch (Exception e)
            {
                richTextBox2.Text = e.ToString();
                return;
            }
            
            richTextBox2.Text = "Compilation OK!";

            /* create a new temporary assembly for our compilation result */
            AssemblyHandler tmpAssembly = new AssemblyHandler();
            tmpAssembly.LoadAssembly(outputFile);

            /* copy the method to the main assembly */
            tmpAssembly.CopyMethodToAssembly(assemblyToInject, typeNameToInject, methodNameToInject, "das", "boot");
            SaveAssembly();

            this.Close();
        }


        /* FORM OBJECTS **************************************************************************/
        private void button1_Click(object sender, EventArgs e)
        {
            CompileAndInject();
        }
    }
}