using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SignalCompiler.SyntaxTree;
using SignalCompiler;

namespace SignalCompiler
{
    class AssemblerCodeGenerator
    {
        public AssemblerCodeGenerator()
        {
            XMLSyntaxTree = TableManeger.DeseriaizeNode();
            resultAsmCode = "";
            posInResultAsmCode = 0;
            dataSegmentPos = 0;
            codeSegmentPos = 0;
            labelNumber = 0;
        }
        private XMLNode XMLSyntaxTree;
        private string resultAsmCode;
        private int posInResultAsmCode;
        private int dataSegmentPos;
        private int codeSegmentPos;
        private static int labelNumber;

        //public delegate void WorkDoneHandler(string output);
        //public event WorkDoneHandler WorkDone;

        private string generateLabel()
        {
            labelNumber++;
            return String.Format("L{0}", labelNumber);
        }

        private void WriteHeader(string idn)
        {
            string header = ".386\n.MODEL\tsmall\n.STACK\t256\n";
            string codeSeg = String.Format(".CODE\n{0}\tPROC\n", idn);
            string endProg = String.Format("mov\tah,4Ch\nmov\tal,0\nint\t21h\n{0}\tENDP\nEND\t{0}", idn);
            resultAsmCode = resultAsmCode.Insert(posInResultAsmCode, header); // insert header
            dataSegmentPos = resultAsmCode.Length; // set pos to continue writing declar if needed
            posInResultAsmCode = dataSegmentPos;
            resultAsmCode = resultAsmCode.Insert(posInResultAsmCode, codeSeg); // insert code start point
            codeSegmentPos = resultAsmCode.Length;
            resultAsmCode = resultAsmCode.Insert(codeSegmentPos, endProg); // insert end of prog
        }

      
    //    private string WriteCondExpr(List<XMLNode> expressions) // returns label
    //    {
    //        var operands = expressions.ToArray();
    //        string label = generateLabel();
    //        string condition = String.Format("mov\teax, {0}\nmov\tebx, {1}\ncmp\teax, ebx\njne\t{2}\n", operands[0].value, operands[1].value, label);
    //        resultAsmCode = resultAsmCode.Insert(codeSegmentPos, condition);
    //        codeSegmentPos += condition.Length;
    //        return label;
    //    }

    //    private string WriteJumpToEndif()
    //    {
    //        string label = generateLabel();
    //        string jmp = String.Format("jmp\t{0} \n", label);
    //        resultAsmCode = resultAsmCode.Insert(codeSegmentPos, jmp);
    //        codeSegmentPos += jmp.Length;
    //        return label;
    //    }

        private void WriteLabel(string label)
        {
            string writeLabel = String.Format("{0}:\n", label);
            resultAsmCode = resultAsmCode.Insert(codeSegmentPos, writeLabel);
            codeSegmentPos += writeLabel.Length;
        }

        private void WriteJmp(string label)
        {
            string writeJmp = String.Format("jmp {0}\n", label);
            resultAsmCode = resultAsmCode.Insert(codeSegmentPos, writeJmp);
            codeSegmentPos += writeJmp.Length;
        }

        private void WriteNewExpression()
        {
            string writeNewExpression = "xor ax, ax\n";
            resultAsmCode = resultAsmCode.Insert(codeSegmentPos, writeNewExpression);
            codeSegmentPos += writeNewExpression.Length;
        }

        private void WriteMinusSummand(string summand)
        {
            string writeMinusSummand = String.Format("sub ax, {0}\n", summand);
            resultAsmCode = resultAsmCode.Insert(codeSegmentPos, writeMinusSummand);
            codeSegmentPos += writeMinusSummand.Length;
        }

        private void WritePlusSummand(string summand)
        {
            string writePlusSummand = String.Format("add ax, {0}\n", summand);
            resultAsmCode = resultAsmCode.Insert(codeSegmentPos, writePlusSummand);
            codeSegmentPos += writePlusSummand.Length;
        }

        private void WriteForInitialization(string var_idn)
        {
            string writeForInitialization = String.Format("mov {0}, ax\n", var_idn);
            resultAsmCode = resultAsmCode.Insert(codeSegmentPos, writeForInitialization);
            codeSegmentPos += writeForInitialization.Length;
        }

        private void ParseNode(XMLNode parentNode)
        {
            bool parseStatement = false; // if true then do custom parse not just straight going throw the tree
            foreach (var item in parentNode.nodes)
            {
                if (item.name == nodesTypes.procedure_idn)
                    WriteHeader(item.value);

                if (item.name == nodesTypes.summand_list)
                {
                    parseStatement = true;

                    string summand = "";
                    XMLNode summandNode = item.nodes.First(x => x.name == nodesTypes.summand);
                    if (summandNode.nodes.Exists(x => x.name == nodesTypes.variable_identifier))
                    {
                        summand = summandNode.nodes.First(x => x.name == nodesTypes.variable_identifier)
                                        .nodes.First(x => x.name == nodesTypes.identifier)
                                        .nodes.First(x => x.name == nodesTypes.token).value;
                    }
                    else if (summandNode.nodes.Exists(x => x.name == nodesTypes.unsigned_integer))
                    {
                        summand = summandNode.nodes.First(x => x.name == nodesTypes.unsigned_integer)
                                        .nodes.First(x => x.name == nodesTypes.token).value;
                    }

                    if (item.nodes.First(x => x.name == nodesTypes.add_instruction)
                            .nodes.First(x => x.name == nodesTypes.token).value == "-")
                    {
                        WriteMinusSummand(summand);
                    }
                    else
                    {
                        WritePlusSummand(summand);
                    }
                    if (item.nodes.Exists(x => x.name == nodesTypes.summand_list))
                        ParseNode(item);
                }

                if (item.name == nodesTypes.statement_list)
                {
                    parseStatement = true;
                    List<XMLNode> statement = item.nodes
                                        .First(x => x.name == nodesTypes.statement)
                                        .nodes; // get nodes of if cond statement
                    string statementType = statement.First(x => x.name == nodesTypes.token).value;
                    if (statementType == "FOR")
                    {
                        string var_idn = statement.First(x => x.name == nodesTypes.variable_identifier)
                                                  .nodes.First(x => x.name == nodesTypes.identifier)
                                                  .nodes.First(x => x.name == nodesTypes.token).value;
                        List<XMLNode> loop_declaration_expressions = statement.First(x => x.name == nodesTypes.loop_declaration)
                                                                        .nodes.FindAll(x => x.name == nodesTypes.expression);
                        ParseExpression(loop_declaration_expressions[0]);
                        WriteForInitialization(var_idn); // here in ax we have calculated expression
                        ParseExpression(loop_declaration_expressions[1]);
                        WriteCMP("ax", var_idn);
                    }
                    else 
                    {
                        if (statementType == "LOOP")
                        {
                            labelNumber++;
                            string l = "l" + labelNumber.ToString();
                            WriteLabel(l);
                            ParseNode(item.nodes.First(x => x.name == nodesTypes.statement));
                            WriteJmp(l);
                        }
                    }
                    
                }
                if (!parseStatement)
                    ParseNode(item);
                else
                    continue;
            }
        }

        private void WriteCMP(string p, string var_idn)
        {
            throw new NotImplementedException();
        }

        private void ParseExpression(XMLNode expression)
        {
            WriteNewExpression();
            string summand = "";
            XMLNode summandNode = expression.nodes.First(x => x.name == nodesTypes.summand);
            if (summandNode.nodes.Exists(x => x.name == nodesTypes.variable_identifier))
            {
                summand = summandNode.nodes.First(x => x.name == nodesTypes.variable_identifier)
                                .nodes.First(x => x.name == nodesTypes.identifier)
                                .nodes.First(x => x.name == nodesTypes.token).value;
            }
            else if (summandNode.nodes.Exists(x => x.name == nodesTypes.unsigned_integer))
            {
                summand = summandNode.nodes.First(x => x.name == nodesTypes.unsigned_integer)
                                .nodes.First(x => x.name == nodesTypes.token).value;
            }
            if (expression.nodes.Exists(x => x.name == nodesTypes.token))
            {
                WriteMinusSummand(summand);
            }
            else
            {
                WritePlusSummand(summand);
            }
            if (expression.nodes.Exists(x => x.name == nodesTypes.summand_list))
            {
                ParseNode(expression);
            }
        }

        public string GenerateCode()
        {
            ParseNode(XMLSyntaxTree);
            return resultAsmCode;
            //if (WorkDone != null) WorkDone(resultAsmCode);
        }
    }

}

