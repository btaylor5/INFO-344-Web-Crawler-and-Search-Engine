using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WikiSearchSuggestions;

namespace TestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Trie sugg = new Trie();
            TrieNode root = sugg.GetRoot();
        }
    }
}
