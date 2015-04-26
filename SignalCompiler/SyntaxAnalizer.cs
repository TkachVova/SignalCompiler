using SignalCompiler;
using SignalCompiler.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lexer
{
    public enum nodesTypes
    {
        node,
        token,
        program,
        procedure_idn,
        block,
        attribute,
        var_idn,
        statement_list,
        statement,
        loop_declaration,
        expression,
        summand_list,
        add_instruction,
        summand,
        variable_identifier,
        identifier,
        unsigned_integer
    }
    class SyntaxAnalizer
    {
        public SyntaxAnalizer(List<LexicalAnalizerOutput> lexems, List<Constant> constants, List<Identifier> identifiers, List<KeyWord> keyWords)
        {
            errors = new List<Error>();
            this.lexems = lexems;
            this.constants = constants;
            this.identifiers = identifiers;
            this.identifiersExtended = new List<IdentifierExt>();
            this.keyWords = keyWords;
            graphNodes = new List<SyntaxTree.Node>();
            links = new List<SyntaxTree.Link>();

            program = new SyntaxTree.XMLNode(nodesTypes.program);
            //graphNodes.Add(new SyntaxTree.Node(nodesTypes.program));
            positionInLexems = -1; 
        }

        private List<Error> errors;
        private List<LexicalAnalizerOutput> lexems;
        private List<Constant> constants;
        private List<Identifier> identifiers;
        private List<KeyWord> keyWords;
        private SyntaxTree.XMLNode program;
        private int positionInLexems; // current pos in lexems
        private List<IdentifierExt> identifiersExtended;

        public delegate void WorkDoneHandler(List<Error> errors, List<IdentifierExt> identifiersExt);
        public event WorkDoneHandler WorkDone;

        private List<SyntaxTree.Node> graphNodes;
        private List<SyntaxTree.Link> links;

        private void AddGraphNode(SyntaxTree.Node g)
        {
            graphNodes.Add(g);
        }

        private void AddLink(SyntaxTree.Link l)
        {
            links.Add(l);
        }

        private void CreateGraphLabels()
        {
            foreach (var item in graphNodes)
            {
                if (item.Value != "")
                    item.Label = item.Id.ToString() + " " + item.Value;
                else
                    item.Label = item.Id.ToString();
            }
        }

        private LexicalAnalizerOutput GetNextToken()
        {
            positionInLexems++;
            if (positionInLexems < lexems.Count)
                return lexems[positionInLexems];
            else return new LexicalAnalizerOutput() { code = -1, row = -1, lexem = ""}; // end of program
        }

        private bool ParseProgram()
        {
            LexicalAnalizerOutput currentToken = GetNextToken();
            SyntaxTree.XMLNode currentNode = program;

            if (currentToken.lexem == "PROGRAM")
            {
                currentNode.AddNode(new SyntaxTree.XMLNode() { name = nodesTypes.token, value = currentToken.lexem});

                if (ParseProcedureIdn())
                {
                    currentToken = GetNextToken();
                    if (currentToken.lexem == ";")
                    {
                        currentNode.AddNode(new SyntaxTree.XMLNode() { name = nodesTypes.token, value = currentToken.lexem });
                        if (!ParseBlock())
                            return false;
                    }
                    else
                    {
                        errors.Add(new Error { message = "**Error** ';' expected", row = currentToken.row });
                    }

                    currentToken = GetNextToken();
                    if (currentToken.lexem == ".")
                    {
                        currentNode.AddNode(new SyntaxTree.XMLNode() { name = nodesTypes.token, value = currentToken.lexem });
                        currentToken = GetNextToken();
                        if (currentToken.code != -1) // if any lexems exists
                            errors.Add(new Error { message = "**Error** Expected end of program", row = currentToken.row });
                    }
                    else
                        errors.Add(new Error { message = "**Error** '.' expected", row = currentToken.row });
                }
                else
                {
                    errors.Add(new Error { message = "**Error** Identifier expected", row = currentToken.row });
                    return false;
                }
                return true;
            }
            else
                errors.Add(new Error { message = "**Error** PROGRAM expected", row = currentToken.row});
            return false;
        }

        private bool ParseProcedureIdn()
        {
            LexicalAnalizerOutput identifier = ParseIdentifier();
            if (identifier.lexem != "")
            {
                program.AddNode(new SyntaxTree.XMLNode() { name = nodesTypes.procedure_idn, value = identifier.lexem });
                return true;
            }
            else
            {
                errors.Add(new Error { message = "**Error** Expected user identifier", row = identifier.row });
                return false;
            }
        }

        private LexicalAnalizerOutput ParseIdentifier() // return empty string if not parsed else return value
        {
            LexicalAnalizerOutput currentToken = GetNextToken();
            if (identifiers.Find(x => x.id == currentToken.code && x.type != identifierType.system) != null)
                return currentToken;
            else
            {
                //errors.Add(new Error { message = "**Error** Expected user identifier", row = currentToken.row });
                return new LexicalAnalizerOutput() { lexem = "" };
            }
        }

        private bool ParseBlock()
        {
            SyntaxTree.XMLNode currentNode = program.AddNode(new SyntaxTree.XMLNode() { name = nodesTypes.block });

                // continue parsing BEGIN
                // decrement positionInLexems cause while parsing VAR block it stops on first not "identifier"
                //
                //positionInLexems--;
            LexicalAnalizerOutput currentToken = GetNextToken(); //BEGIN expected
            if (currentToken.lexem == "BEGIN")
            {
                currentNode.AddNode(new SyntaxTree.XMLNode() { name = nodesTypes.token, value = currentToken.lexem });
                // continue parsing statementList
                if (parseStatementList(program.nodes.Find(x => x.name == nodesTypes.block)))
                {
                    positionInLexems--;
                    int curr_row = GetNextToken().row;
                    currentToken = GetNextToken();
                    if (currentToken.lexem == "END")
                    {
                        currentNode.AddNode(new SyntaxTree.XMLNode() { name = nodesTypes.token, value = currentToken.lexem });
                        return true;
                    }
                    else
                    {
                        if (currentToken.row != -1)
                            curr_row = currentToken.row;
                        errors.Add(new Error { message = "**Error** END or statement expected", row = curr_row });
                    }
                }
                else
                    return false;
            }
            else
            {
                errors.Add(new Error { message = "**Error** BEGIN expected", row = currentToken.row });
            }   
                
            return false;

        }

        private bool parseStatementList(SyntaxTree.XMLNode curr)
        {
            SyntaxTree.XMLNode currentNode = curr.AddNode(new SyntaxTree.XMLNode() { name = nodesTypes.statement_list });

            if (parseStatement(currentNode))
                parseStatementList(curr);
            else
            {
                positionInLexems--;
                curr.nodes.Remove(currentNode);
            }
                

            //positionInLexems--;  
            return true;
        }

        private bool parseStatement(SyntaxTree.XMLNode curr)
        {
            SyntaxTree.XMLNode currentNode = curr.AddNode(new SyntaxTree.XMLNode() { name = nodesTypes.statement });

            LexicalAnalizerOutput currentToken = GetNextToken();

            if (currentToken.lexem == "LOOP")
            {
                currentNode.AddNode(new SyntaxTree.XMLNode() { name = nodesTypes.token, value = currentToken.lexem });
                if (parseStatementList(currentNode))
                {
                    currentToken = GetNextToken();
                    if (currentToken.lexem == "ENDLOOP")
                    {
                        currentNode.AddNode(new SyntaxTree.XMLNode() { name = nodesTypes.token, value = currentToken.lexem });
                        currentToken = GetNextToken();
                        if (currentToken.lexem == ";")
                        {
                            currentNode.AddNode(new SyntaxTree.XMLNode() { name = nodesTypes.token, value = currentToken.lexem });
                            return true;
                        }
                        else
                        {
                            errors.Add(new Error { message = "**Error** Expected ';'", row = currentToken.row });
                        }
                    }
                    else
                    {
                        errors.Add(new Error { message = "**Error** Expected 'ENDLOOP'", row = currentToken.row });
                    }
                }
            }
            else 
            {
                if (currentToken.lexem == "FOR")
                {
                    currentNode.AddNode(new SyntaxTree.XMLNode() { name = nodesTypes.token, value = currentToken.lexem });
                    LexicalAnalizerOutput expectedVarIdn = ParseIdentifier();
                    if (expectedVarIdn.lexem != "")
                    {
                        currentNode.AddNode(new SyntaxTree.XMLNode() { name = nodesTypes.variable_identifier })
                                   .AddNode(new SyntaxTree.XMLNode() { name = nodesTypes.identifier })
                                   .AddNode(new SyntaxTree.XMLNode() { name = nodesTypes.token, value = expectedVarIdn.lexem });
                        currentToken = GetNextToken();
                        if (currentToken.lexem == ":=")
                        {
                            currentNode.AddNode(new SyntaxTree.XMLNode() { name = nodesTypes.token, value = currentToken.lexem });
                            if (parseLoopDeclaration(currentNode))
                            {
                                currentToken = GetNextToken();
                                if (currentToken.lexem == "ENDFOR")
                                {
                                    currentNode.AddNode(new SyntaxTree.XMLNode() { name = nodesTypes.token, value = currentToken.lexem });
                                    currentToken = GetNextToken();
                                    if (currentToken.lexem == ";")
                                    {
                                        currentNode.AddNode(new SyntaxTree.XMLNode() { name = nodesTypes.token, value = currentToken.lexem });
                                        return true;
                                    }
                                    else
                                    {
                                        errors.Add(new Error { message = "**Error** Expected ';'", row = currentToken.row });
                                    }
                                }
                                else
                                {
                                    errors.Add(new Error { message = "**Error** Expected 'ENDFOR'", row = currentToken.row });
                                }

                            }
                        }
                        else
                        {
                            errors.Add(new Error { message = "**Error** Expected ':='", row = currentToken.row });
                        }
                    }
                    else
                    {
                        errors.Add(new Error { message = "**Error** Expected identifier", row = expectedVarIdn.row });
                    }
                }
            }
            return false;
        }

        private bool parseLoopDeclaration(SyntaxTree.XMLNode curr)
        {
            SyntaxTree.XMLNode currentNode = curr.AddNode(new SyntaxTree.XMLNode() { name = nodesTypes.loop_declaration });

            
            if (parseExpression(currentNode))
            {
                LexicalAnalizerOutput currentToken = GetNextToken();
                if (currentToken.lexem == "TO")
                {
                    currentNode.AddNode(new SyntaxTree.XMLNode() { name = nodesTypes.token, value = currentToken.lexem });
                    if (parseExpression(currentNode))
                    {
                        currentToken = GetNextToken();
                        if (currentToken.lexem == "DO")
                        {
                            currentNode.AddNode(new SyntaxTree.XMLNode() { name = nodesTypes.token, value = currentToken.lexem });
                            if (parseStatementList(currentNode))
                                return true;
                        }
                        else
                        {
                            errors.Add(new Error { message = "**Error** Expected 'DO'", row = currentToken.row });
                        }
                    }
                }
                else
                {
                    errors.Add(new Error { message = "**Error** Expected 'TO'", row = currentToken.row });
                }
            }
            return false;
        }

        private bool parseExpression(SyntaxTree.XMLNode curr)
        {
            SyntaxTree.XMLNode currentNode = curr.AddNode(new SyntaxTree.XMLNode() { name = nodesTypes.expression });
            
            if (parseSummand(currentNode))
            {
                if (parseSummandsList(currentNode))
                {
                    return true;
                }
            }
            else
            {
                positionInLexems--; // parseSummand will check next token, so need to decrement pos
                LexicalAnalizerOutput currentToken = GetNextToken();
                if (currentToken.lexem == "-")
                {
                    currentNode.AddNode(new SyntaxTree.XMLNode() { name = nodesTypes.token, value = currentToken.lexem });
                    if (parseSummand(currentNode))
                    {
                        if (parseSummandsList(currentNode))
                        {
                            return true;
                        }
                    }
                }
                else
                {
                    errors.Add(new Error { message = "**Error** Expected expression", row = currentToken.row });
                }
            }
            return false;
        }

        private bool parseSummandsList(SyntaxTree.XMLNode curr)
        {
            SyntaxTree.XMLNode currentNode = curr.AddNode(new SyntaxTree.XMLNode() { name = nodesTypes.summand_list });

            if (parseAddInstruction(currentNode))
            {
                if (parseSummand(currentNode))
                    parseSummandsList(currentNode);
                else
                {
                    positionInLexems--;
                    curr.nodes.Remove(currentNode);
                }
            }
            else
            {
                positionInLexems--;
                curr.nodes.Remove(currentNode);
            }
                
            
            //positionInLexems--;  
            return true;
        }

        private bool parseAddInstruction(SyntaxTree.XMLNode curr)
        {
            SyntaxTree.XMLNode currentNode = curr.AddNode(new SyntaxTree.XMLNode() { name = nodesTypes.add_instruction });

            LexicalAnalizerOutput currentToken = GetNextToken();
            if (currentToken.lexem == "-")
            {
                currentNode.AddNode(new SyntaxTree.XMLNode() { name = nodesTypes.token, value = currentToken.lexem });
                return true;
            }
            else if (currentToken.lexem == "+")
            {
                currentNode.AddNode(new SyntaxTree.XMLNode() { name = nodesTypes.token, value = currentToken.lexem });
                return true;
            }
           
            return false;
        }

        private bool parseSummand(SyntaxTree.XMLNode curr)
        {
            SyntaxTree.XMLNode currentNode = curr.AddNode(new SyntaxTree.XMLNode() { name = nodesTypes.summand });

            LexicalAnalizerOutput expectedVarIdn = ParseIdentifier();
            if (expectedVarIdn.lexem != "")
            {
                currentNode.AddNode(new SyntaxTree.XMLNode() { name = nodesTypes.variable_identifier })
                                   .AddNode(new SyntaxTree.XMLNode() { name = nodesTypes.identifier })
                                   .AddNode(new SyntaxTree.XMLNode() { name = nodesTypes.token, value = expectedVarIdn.lexem });
                return true;
            }
            else
            {
                positionInLexems--;
                LexicalAnalizerOutput currentToken = GetNextToken();
                if (constants.Find(x => x.value.ToString() == currentToken.lexem) != null)
                {
                    currentNode.AddNode(new SyntaxTree.XMLNode() { name = nodesTypes.unsigned_integer })
                                .AddNode(new SyntaxTree.XMLNode() { name = nodesTypes.token, value = currentToken.lexem });
                    return true;
                }
                else
                    errors.Add(new Error { message = "**Error** Expected summand", row = currentToken.row });
            }
            return false;
        }
       
        public void Analize()
        {
            ParseProgram();
            TableManeger.SeriaizeNode(program);

            SyntaxTree.XMLNodeToDGMLParser parser = new SyntaxTree.XMLNodeToDGMLParser();

            SyntaxTree.Graph graph = parser.GetGraph();
            
            TableManeger.SeriaizeNodeGraph(graph);
            if (WorkDone != null) WorkDone(errors, identifiersExtended);
        }
    }
}
