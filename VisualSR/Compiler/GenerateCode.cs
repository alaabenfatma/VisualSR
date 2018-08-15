/*Copyright 2018 ALAA BEN FATMA

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.*/
using System.Text;
using VisualSR.Core;

namespace VisualSR.Compiler
{
    public static class CodeMiner
    {
        public static string Code(Node root)
        {
            var codeBuilder = new StringBuilder();
            codeBuilder.Append(root.GenerateCode());
            codeBuilder.AppendLine();
            if (root.OutExecPorts.Count <= 0) return codeBuilder.ToString();
            if (root.OutExecPorts[0].ConnectedConnectors.Count <= 0) return codeBuilder.ToString();
            var nextNode = root.OutExecPorts[0].ConnectedConnectors[0].EndPort.ParentNode;
            var stillHasMoreNodes = true;
            while (stillHasMoreNodes)
            {
                codeBuilder.Append(nextNode.GenerateCode());
                codeBuilder.AppendLine();
                if (nextNode.OutExecPorts[0].ConnectedConnectors.Count > 0)
                    nextNode = nextNode.OutExecPorts[0].ConnectedConnectors[0].EndPort.ParentNode;
                else
                    stillHasMoreNodes = false;
            }
            return codeBuilder.ToString();
        }
    }
}