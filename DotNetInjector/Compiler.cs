using System;
using Microsoft.CSharp;
using System.CodeDom.Compiler;


namespace DotNetInjector
{
    class Compiler
    {
        private static CSharpCodeProvider codeProvider = new CSharpCodeProvider();
        private static ICodeCompiler icc;
        private static string[] references;
        private static string output;


        /* CONSTRUCTOR ***************************************************************************/
        public Compiler()
        {
            icc = codeProvider.CreateCompiler();
        }


        /* GET/SET *******************************************************************************/
        public void SetReferences(string[] refs)
        {
            references = refs;
        }

        public void SetOutput(string FileName)
        {
            output = FileName;
        }


        /* API ***********************************************************************************/
        public void Compile(string source)
        {
            CompilerParameters parameters = new CompilerParameters(references, output);
            CompilerResults results = icc.CompileAssemblyFromSource(parameters, source);

            if (results.Errors.Count > 0)
            {
                String errorText = "";
                foreach (CompilerError error in results.Errors)
                    errorText += error.Line + ": " + error.ErrorText + "\n";
                throw new Exception(errorText);
            }
        }
    }
}