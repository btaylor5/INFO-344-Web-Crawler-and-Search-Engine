using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Controller
{
    public class HybridTrieNode
    {
        public static int ConvertAt = 9;
        public List<string> list;
        public Dictionary<char, HybridTrieNode> dict;
        public bool end;

        /// <summary>
        /// Hybrid Trie Node That supports storing strings in a list when the Trie is unnecessary.
        /// </summary>
        public HybridTrieNode()
        {
            list = new List<string>();
            dict = new Dictionary<char, HybridTrieNode>();
        }

        /// <summary>
        /// Adds a character to this nodes dictionary
        /// </summary>
        /// <param name="c"></param>
        public void AddToDict(char c) {
            dict.Add(c, new HybridTrieNode());
        }

        /// <summary>
        /// Adds a string to this nodes list
        /// </summary>
        /// <param name="str"></param>
        public void AddToList(string str)
        {
            list.Add(str);
        }

        /// <summary>
        /// Returns true if this node represents the end of a word
        /// </summary>
        /// <returns></returns>
        public bool IsEnd()
        {
            return this.end;
        }

        /// <summary>
        /// Sets the node's "end" field to whatever you boolean value you pass in
        /// </summary>
        /// <param name="end"></param>
        public void SetEnd(bool end)
        {
            this.end = end;
        }

        /// <summary>
        /// Returns whether this node's listsize is greater than or equals to 
        /// the size that you want to convert from a list to a trie
        /// </summary>
        /// <returns></returns>
        public bool ShouldConvert()
        {
            return list.Count >= ConvertAt;
        }

        public void SetAsRoot()
        {
            this.list = null;
        }
    }
}