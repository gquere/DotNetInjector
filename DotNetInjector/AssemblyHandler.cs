using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Mono.Cecil;
using Mono.Cecil.Cil;


namespace DotNetInjector
{
    public class AssemblyHandler
    {
        private AssemblyDefinition assembly;

        /* READ AND WRITE ASSEMBLY ***************************************************************/
        public bool LoadAssembly(String fileName)
        {
            try
            {
                assembly = AssemblyDefinition.ReadAssembly(fileName);
                return true;
            }
            catch
            {
                MessageBox.Show("Could not read Assembly, probably non .net file or unreadable obfuscation.");
                return false;
            }
        }

        public void SaveAssembly(String outputFileName)
        {
            try
            {
                assembly.Write(outputFileName);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
                System.IO.File.Delete(outputFileName);
            }
        }


        /* GETTERS *******************************************************************************/
        public String GetRuntime()
        {
            return assembly.MainModule.Runtime.ToString();
        }

        public AssemblyDefinition GetAssembly()
        {
            return assembly;
        }


        /* DECOMPILERS **************************************************************************/
        public String DecompileMethod(String typeName, String methodName)
        {
            MethodDefinition method = FindMethodInAssembly(typeName, methodName);
            if (method.IsSetter || method.IsGetter || method.Body == null)
                return null;

            String text = "";
            ILProcessor cilWorker = method.Body.GetILProcessor();
            foreach (Instruction ins in cilWorker.Body.Instructions)
            {
                text += ins + Environment.NewLine;
            }

            return text;
        }

        private Instruction[] DecompileMethodAsInstructions(String typeName, String methodName, AssemblyDefinition dstAssembly)
        {
            MethodDefinition srcMethod = FindMethodInAssembly(typeName, methodName);
            if (srcMethod.IsSetter || srcMethod.IsGetter || srcMethod.Body == null)
                return null;

            List<Instruction> dstInstructions = new List<Instruction>();

            foreach (Instruction srcInstruction in srcMethod.Body.Instructions)
            {
                object operand = srcInstruction.Operand;

                if (operand is MethodReference)
                {
                    MethodReference mref = operand as MethodReference;
                    MethodReference newf = dstAssembly.MainModule.ImportReference(mref);
                    dstInstructions.Add(Instruction.Create(srcInstruction.OpCode, newf));
                    continue;
                }
                dstInstructions.Add(srcInstruction);
            }

            // remove last instruction which is ret
            dstInstructions.RemoveAt(dstInstructions.Count - 1);

            // reverse the list so that we can inject each instruction using InsertBefore
            dstInstructions.Reverse();

            return dstInstructions.ToArray();          
        }


        /* INJECTION ****************************************************************************/
        private void InjectInstructions(MethodDefinition methodDefinition, Instruction[] instructions)
        {
            ILProcessor cilWorker = methodDefinition.Body.GetILProcessor();

            foreach (Instruction instruction in instructions)
            {
                cilWorker.InsertBefore(methodDefinition.Body.Instructions[0], instruction);
            }
        }

        public void CopyMethodToAssembly(AssemblyHandler assemblyHandlerToInject, String typeNameToInject, String methodNameToInject,
                                         String typeNameToCopy, String methodNameToCopy)
        {
            AssemblyDefinition assemblyToInject = assemblyHandlerToInject.GetAssembly();
            Instruction[] instructions = this.DecompileMethodAsInstructions(typeNameToCopy, methodNameToCopy, assemblyToInject);
            MethodDefinition methodToInject = assemblyHandlerToInject.FindMethodInAssembly(typeNameToInject, methodNameToInject);

            /* inject the instructions in the main assembly */
            InjectInstructions(methodToInject, instructions);
        }


        /* METHOD HELPERS ***********************************************************************/
        public void ReadMethodsToTree(TreeView tree)
        {
            tree.Nodes.Clear();

            foreach (TypeDefinition type in assembly.MainModule.Types)
            {
                TreeNode node = tree.Nodes.Add(type.Name.ToString());

                foreach (MethodDefinition method in type.Methods)
                {
                    node.Nodes.Add(method.Name.ToString());
                }
            }
        }

        /**
         * Check if the method selected by the user is valid, e.g. it's found in the assmebly
         * and it has a body
         */
        public Boolean CheckIfMethodIsInjectable(String typeName, String methodName)
        {
            if (typeName == null || methodName == null)
                return false;

            foreach (TypeDefinition type in assembly.MainModule.Types)
            {
                if (type.Name != typeName)
                    continue;

                foreach (MethodDefinition method in type.Methods)
                    if (method.Name == methodName && method.Body != null)
                        return true;
            }
            return false;
        }

        private MethodDefinition FindMethodInAssembly(String typeName, String methodName)
        {
            if (typeName == null || methodName == null)
                return null;

            foreach (TypeDefinition type in assembly.MainModule.Types)
            {
                if (type.Name != typeName)
                {
                    continue;
                }

                foreach (MethodDefinition method in type.Methods)
                {
                    if (method.Name == methodName)
                        return method;
                }
            }
            return null;
        }
    }
}
