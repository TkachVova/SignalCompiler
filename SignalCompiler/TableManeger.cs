using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Xml.Serialization;
using System.IO;
using SignalCompiler.Domain;

namespace SignalCompiler
{
    //
    enum attrType { whitespace, constant, identifier, oneSymbDelimiter, manySymbDelimiter, begCom, invalid };
    class TableManeger
    {
        private static string path = System.IO.Directory.GetCurrentDirectory();
        private static string attributesTablePath = path + @"\AttributesTable.xml";
        private static string keyWordsTablePath = path + @"\KeyWordsTable.xml";
        private static string identifiersTablePath = path + @"\IdentifiersTable.xml";
        private static void Serialize(object obj, string path)
        {
            Type objectType = obj.GetType();
            XmlSerializer writer = new XmlSerializer(objectType);
            StreamWriter file = new StreamWriter(path);
            writer.Serialize(file, obj);
            file.Close();
        }

        private static void Deserialize(ref object obj, string path)
        {
            Type objectType = obj.GetType();
            XmlSerializer reader = new XmlSerializer(objectType);
            StreamReader file = new StreamReader(path);
            obj = reader.Deserialize(file);
        }
        
        private static char[] GenerateCharArray(char start, char fin)
        {
            return Enumerable.Range(start, fin - start + 1).Select(x => (char)x).ToArray();
        }

        //write attributes xml file
        public static void SerializeAttributes()
        {
            char[] whitespaces = new char[]  // 0 in attributes
            {
                '\x20', // ascii code space
                '\xD', // carriage return
                '\xA', // line feed
                '\x9', // horizontal tab
                '\xB', // vertical tab 
                '\xC' // form feed
            };

            char[] constants = GenerateCharArray('0', '9'); // numbers 0..9

            char[] letters = GenerateCharArray('A', 'Z');

            char[] delimiters = new char[] { ';', '.', '+', '-'};

            char[] begCom = new char[] { '(' };

            List<Attributes> listAttributes = new List<Attributes>();
            for (int i = 0; i <= 255; i++)
            {
                Attributes attributes = new Attributes();
                attributes.symbol = Convert.ToChar(i); // convert ascii to char

                if (whitespaces.Contains(attributes.symbol))
                    attributes.type = (int) attrType.whitespace;
                else if (constants.Contains(attributes.symbol))
                    attributes.type = (int) attrType.constant;
                else if (letters.Contains(attributes.symbol))
                    attributes.type = (int) attrType.identifier;
                else if (delimiters.Contains(attributes.symbol))
                    attributes.type = (int) attrType.oneSymbDelimiter;
                else if (begCom.Contains(attributes.symbol))
                    attributes.type = (int) attrType.begCom;
                else
                    attributes.type = (int) attrType.invalid;

                listAttributes.Add(attributes);
            }
            Serialize(listAttributes, attributesTablePath);
        }

        //gets attribute list from xml file
        public static List<Attributes> DeserializeAttributes()
        {
            List<Attributes> listAttributes = new List<Attributes>();
            object obj = (object) listAttributes;
            Deserialize(ref obj, attributesTablePath);

            Debug.Print("\nAttributes deserialized.");
            foreach (var item in (List<Attributes>) obj)
            {
                if (item.type != (int) attrType.invalid)
                    Debug.Print("symbol: {0}    type: {1}",
                        (item.type == (int) attrType.whitespace) ? "ascii " + Convert.ToInt32(item.symbol).ToString() : item.symbol.ToString(),
                        item.type);
            }

            return (List<Attributes>) obj;
        }
        //write key word xml file
        public static void SeriaizeKeyWords()
        {
            string[] words = new string[] { "PROGRAM", "BEGIN", "END", "LOOP", "ENDLOOP", "FOR", "ENDFOR", ":=", "TO", "DO" };
            int id = 301;
            List<KeyWord> keyWordsList = new List<KeyWord>();

            foreach (var item in words)
            {
                KeyWord keyWord = new KeyWord(item, id++);
                keyWordsList.Add(keyWord);
            }

            Serialize(keyWordsList, keyWordsTablePath);
        }
        //gets key word list from xml file
        public static List<KeyWord> DeserializeKeyWords()
        {
            List<KeyWord> keyWordsList = new List<KeyWord>();
            object obj = (object)keyWordsList;
            Deserialize(ref obj, keyWordsTablePath);

            Debug.Print("\nKeyWords deserialized");
            foreach (var item in (List<KeyWord>)obj)
            {
                Debug.Print("Keyword: {0}   id: {1}", item.keyWord, item.id.ToString());
            }

            return (List<KeyWord>)obj;
        }
      
    }
    
}
