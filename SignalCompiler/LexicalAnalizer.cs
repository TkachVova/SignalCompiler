using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SignalCompiler.Domain;
using System.IO;

namespace SignalCompiler
{
    //Row of lexems and their codes
    struct LexicalAnalizerOutput
    {
        public int code;
        public string lexem;
        public int row;
    }
    class LexicalAnalizer
    {
        public LexicalAnalizer()
        {
            //TableManeger.SerializeAttributes();
            //TableManeger.SeriaizeKeyWords();
            attributes = TableManeger.DeserializeAttributes();
            keyWords = TableManeger.DeserializeKeyWords();
            constants = new List<Constant>();
            identifiers = new List<Identifier>();
            errors = new List<Error>();
        }

        private List<Attributes> attributes;
        public List<Identifier> identifiers;
        public List<KeyWord> keyWords;
        public List<Constant> constants;
        public List<Error> errors;

        public static string commentSymbol = "*";
        public static string endCom = ")";



        public List<LexicalAnalizerOutput> MakeLexemLine(string filepath)
        {
            List<LexicalAnalizerOutput> result = new List<LexicalAnalizerOutput>();
            string[] lines;

            if (File.Exists(filepath))
            {
                lines = File.ReadAllLines(filepath);
            }
            else
            {
                throw new FileNotFoundException();
            }

            int i = 0; // row number
            int j = 0; // symbol number
            bool lineWhiteSpaced = false; // true if whitespaced
            string buffer_for_lexems = "";
            int lexCode = 0;
            string currLexem = ""; // used only for logging

            if (attributes.Count != 0)
            {
                for (i = 0; i < lines.Count(); i++)
                {
                    string currentLine = lines[i];
                    j = 0;

                    while (j < currentLine.Length)
                    {
                        lineWhiteSpaced = false;
                        char currentSymbol = currentLine[j];
                        int symbolAttr = GetSymbolAttr(currentSymbol);
                        buffer_for_lexems = "";
                        currLexem = "";

                        if (symbolAttr == (int)attrType.whitespace) // whitespace
                        {
                            while (++j < currentLine.Length)
                            {
                                currentSymbol = currentLine[j];
                                symbolAttr = GetSymbolAttr(currentSymbol);
                                if (symbolAttr != (int)attrType.whitespace)
                                    break;
                            }
                            lineWhiteSpaced = true;
                        }
                        else if (symbolAttr == (int)attrType.constant) // constant
                        {
                            buffer_for_lexems = BuferizeLexem(currentLine, attrType.constant, ref j);
                            currLexem = buffer_for_lexems;
                            lexCode = GetConstantLexCode(buffer_for_lexems);
                        }
                        else if (symbolAttr == (int)attrType.identifier) // identifier
                        {
                            buffer_for_lexems = BuferizeLexem(currentLine, attrType.identifier, ref j);
                            currLexem = buffer_for_lexems;
                            lexCode = GetIndentifierLexCode(buffer_for_lexems);
                        }
                        else if (symbolAttr == (int)attrType.oneSymbDelimiter) // divider
                        {
                            lexCode = (int)currentSymbol;
                            currLexem = currentSymbol.ToString();
                            j++;
                        }
                        //some shit code =)
                        else if (currentSymbol == ':')
                        {
                            buffer_for_lexems = Find_equel(currentLine, ref j);
                            if (buffer_for_lexems != "")
                            {
                                currLexem = buffer_for_lexems;
                                lexCode = GetIndentifierLexCode(buffer_for_lexems);
                            }
                        }
                        else if (symbolAttr == (int)attrType.begCom) // Comment
                        {
                            int indexBeforeDelete = i;
                            DeleteComment(lines, ref i, ref j);
                            lineWhiteSpaced = true;
                            if (indexBeforeDelete != i)
                                break;
                        }
                        else
                        {
                            j++;
                            errors.Add(new Error { message = "**Error** Invalid symbol", row = i, pos = j });
                            lineWhiteSpaced = true;
                        }
                        if (!lineWhiteSpaced)
                        {
                            result.Add(new LexicalAnalizerOutput { code = lexCode, lexem = currLexem, row = i });
                        }
                    }

                }
            }

            return result;
        }

        //returns type of current symbol
        private int GetSymbolAttr(char symbol)
        {
            if (attributes.Count != 0)
                return attributes.First(x => x.symbol == symbol).type;
            return -1;
        }

        //devides lexem (identifier or constant) from line
        private string BuferizeLexem(string currentLine, attrType type, ref int j)
        {
            string buffer = "";
            char currentSymbol = currentLine[j];
            buffer += currentSymbol.ToString();
            while (++j < currentLine.Length)
            {
                currentSymbol = currentLine[j];
                int symbolAttr = GetSymbolAttr(currentSymbol);
                // if type == constant it takes only digits, if identifier it takes letters or digits starting from second symbol
                if (symbolAttr == (int)type || symbolAttr == (int)attrType.constant)
                    buffer += currentSymbol.ToString();
                else break;
            }
            return buffer;
        }

        //devides lexem := from line
        private string Find_equel(string currentLine, ref int j)
        {
            string buffer = "";
            char currentSymbol = currentLine[j];
            if (j < currentLine.Length - 2)
            {
                if (currentLine[j + 1] == '=')
                {
                    buffer += currentSymbol.ToString();
                    buffer += currentLine[j + 1];
                    j++;
                    j++;
                }
            }

            return buffer;
        }

        // returns lexCode, if not present in constants returns new id
        private int GetConstantLexCode(string constStr)
        {
            int lexCode = 0;
            if (constants.Count() == 0)
            {
                Constant constant = new Constant(Convert.ToInt32(constStr));
                constants.Add(constant);
                lexCode = constant.id;
            }
            else
            {
                if (!constants.Any(x => x.value == Convert.ToInt32(constStr))) // if no consts has the same value
                {
                    // creates new const with id = maxId + 1
                    Constant constant = new Constant(Convert.ToInt32(constStr), constants.OrderByDescending(x => x.id).First().id + 1);
                    constants.Add(constant);
                    lexCode = constant.id;
                }
                else lexCode = constants.First(x => x.value == Convert.ToInt32(constStr)).id; // if exists get id
            }
            return lexCode;
        }

        // returns lexCode of identifier 
        private int GetIndentifierLexCode(string identifierStr)
        {
            int lexCode = 0;

            if (keyWords.Count() != 0)
            {
                if (keyWords.Any(x => x.keyWord == identifierStr))
                {
                    lexCode = keyWords.First(x => x.keyWord == identifierStr).id;
                    return lexCode;
                }
            }
            if (identifiers.Count() != 0)
            {
                if (identifiers.Any(x => x.name == identifierStr))
                {
                    lexCode = identifiers.First(x => x.name == identifierStr).id;
                    return lexCode;
                }
                else
                {
                    // creates new identifier with id = maxId + 1
                    Identifier identifier = new Identifier(identifierStr, identifierType.user, identifiers.OrderByDescending(x => x.id).First().id + 1);
                    identifiers.Add(identifier);
                    lexCode = identifier.id;
                    return lexCode;
                }
            }
            else
            {
                //first identifier to identifiers list
                Identifier identifier = new Identifier(identifierStr, identifierType.user);
                identifiers.Add(identifier);
                lexCode = identifier.id;
                return lexCode;
            }
        }

        private void DeleteComment(string[] lines, ref int i, ref int j)
        {

            int entry_i = i;
            int entry_j = j;
            string currentLine = lines[i];
            char currentSymbol = currentLine[j];

            j++;

            if (j < currentLine.Length)
            {
                currentSymbol = currentLine[j];
                entry_j = j;
            }
            else // error ??      
            {
                errors.Add(new Error { message = "**Error** BegCom symbol without '*'", row = i, pos = j });
                j++; // Analize method will iterate to next row
                //entry_i = i;
                //entry_j = j;
                return;
            }

            if (currentSymbol == (char)commentSymbol[0]) // if (*
            {
                j++;
                for (int k = i; k < lines.Count(); k++)
                {
                    currentLine = lines[k];
                    while (j < currentLine.Length - 1)
                    {
                        currentSymbol = currentLine[j];
                        char nextSymbol = currentLine[j + 1];
                        j++;
                        if (currentSymbol == commentSymbol[0] && nextSymbol == endCom[0]) // end of Comment found
                        {
                            i = k;
                            j += 1; // skip "*)"
                            return;
                        }
                    }
                    j = 0;
                }
                // ERROR end of comment not found
                i = entry_i; // skip begCom and continue parsing
                j = entry_j;
                errors.Add(new Error { message = "**Error** End of comment not found", row = i, pos = j });
                return;
            }
            else
            {
                // ERROR ("Do u mean comment? '*' missing")
                errors.Add(new Error { message = "**Error** Do u mean comment? '*' missing", row = i, pos = j });
            }

        }
    }
}
