using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WikiSearchSuggestions
{
    public class TrieNode
    {
        private char value;
        private Dictionary<char, TrieNode> dict;
        private bool end;

        public TrieNode(char value)
        {
            this.value = value;
            this.dict = new Dictionary<char, TrieNode>();
            this.end = false;
        }

        public TrieNode() 
            : this('\0') {
        }

        public char GetChar()
        {
            return this.value;
        }


        // Takes in a character
        // Will add the reference to the new TrieNode in this's dictionary, 
        // while creating the actual TrieNode for which it will reference
        public void AddTrieNode(char next) {
            this.dict.Add(next, new TrieNode(next));
        }

        public TrieNode GetNode(char nodeValue)
        {
            return this.dict[nodeValue];
        }

        public override string ToString()
        {
            return "" + this.value ;
        }

        public bool CharPresent(char lookFor)
        {
            return this.dict.ContainsKey(lookFor);
        }

        public void AddNode(char value)
        {
            this.dict.Add(value, new TrieNode(value));
        }

        public Dictionary<char, TrieNode> GetDict()
        {
            return this.dict;
        }

        public void SetEndOfTitle(bool status)
        {
            this.end = status;
        }

        public bool isEndOfTitle()
        {
            return this.end;
        }
    }
}