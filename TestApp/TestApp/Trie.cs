using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using WikiSearchSuggestions;

namespace WikiSearchSuggestions
{

    public class Trie
    {

        private byte MIN_MEMORY = 50;
        private TrieNode root;

        public Trie()
        {
            this.root = new TrieNode();
            try
            {
                using (StreamReader sr = new StreamReader(@"C:\Users\Bryant Taylor\Source\Repos\INFO344\TestApp\TestApp\filteredWiki2.txt"))
                {
                    string title;

                    PerformanceCounter ramCounter = new PerformanceCounter("Memory", "Available MBytes");
                    long count = 1;
                    while ((title = sr.ReadLine()) != null && ramCounter.NextValue() > 50)
                    {
                        Queue<char> titleInChars = FormatTitle(title.Replace('_', ' '));
                        if (titleInChars != null)
                        {
                            AddTitle(titleInChars);
                            Console.WriteLine(count++ + " with " + ramCounter.NextValue() + "MB Left");
                        }
                    }
                    }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            string search = Console.ReadLine();
            Console.WriteLine("_______________________Start Searching_________________");
            List<string> answers = new List<string>();
            while (!search.Equals(""))
            {
                answers = GetAllSuggestions(search, 20);
                Console.WriteLine("_______________________Start Searching_________________");
                search = Console.ReadLine();
            }
        }

        public Queue<char> FormatTitle(string title)
        {
            
            Queue<char> charsToAdd = new Queue<char>();
            char[] bank = title.ToCharArray();
            foreach (char character in bank)
            {
                if ((character >= 65 && character <= 90) || (character >= 97 && character <= 122) || character == 32)
                {
                    charsToAdd.Enqueue(character);
                }
                else
                {
                    return null; //Filter out, not a-zA-Z || ' '
                }
                
            }
            return charsToAdd;
        }

        public void AddTitle(Queue<char> title)
        {
            AddCharNodes(root, title);
        }

        

        public TrieNode AddCharNodes(TrieNode current, Queue<char> titleCharacters)
        {
            if (titleCharacters.Count != 0)
            {
                char next = titleCharacters.Dequeue();
                if (!current.CharPresent(next)) //Doesn't have next character, we need to create it
                {
                    current.AddNode(next); // Add A TrieNode from current -> DEQUEUE (should be next)
                }
                current = current.GetNode(next); // move to the next node
                AddCharNodes(current, titleCharacters); // current = 
            }
            else 
            {
                current.SetEndOfTitle(true);
            }
            return current;
        }

        public TrieNode GetRoot()
        {
            return this.root;
        }

        public TrieNode SearchForPrefix(string prefix)
        {
            Queue<char> prefixCharacters = new Queue<char>();

            char[] bank = prefix.ToCharArray();
            foreach (char character in bank)
            {
                prefixCharacters.Enqueue(character);
            }
            TrieNode placeHolder = Search(root, prefixCharacters);
            return placeHolder;
        }

        public TrieNode Search(TrieNode current, Queue<char> prefixCharacters) 
        {
            if (prefixCharacters.Count > 0)
            {
                char next = prefixCharacters.Dequeue();
                if (current.CharPresent(next))
                {
                    return Search(current.GetNode(next), prefixCharacters);
                }
                else
                {
                    return null;
                    Console.WriteLine("Didn't Find Next Character: " + next);
                }
            }
            return current;
        }

        // Will currently return one result. Need to incomporate a 
        // stack to store nodes that I come by. Do we create another test case
        // in order to know when to call from it??
        public string GetSuggestions(TrieNode startingNode)
        {
            Dictionary<char, TrieNode> possibilities = startingNode.GetDict();
            if (possibilities.Count == 0)
            {
                return startingNode.ToString();
            }
            else
            {
               TrieNode next = startingNode.GetDict().Values.First();//Forces 1 result
               return startingNode.ToString() + GetSuggestions(next);
            }
        }

        public List<string> GetAllSuggestions(string prefix, int maxResults)
        {
            List<string> suggestions = new List<string>();
            TrieNode endOfPrefix = SearchForPrefix(prefix);
            GetMatches(endOfPrefix, suggestions, maxResults, prefix.TrimEnd(prefix[prefix.Length - 1]), prefix);
            foreach (string suggestion in suggestions)
            {
                Console.WriteLine(suggestion);
            }
            return suggestions;
        }

        public void GetMatches(TrieNode current, List<string> suggestions, int maxResults, string letters, string prefix)
        {
            if (current != null)
            {
                letters += current.GetChar();

                if (current.isEndOfTitle() || current.GetDict().Keys.Count == 0)
                {
                    if (!(suggestions.Count > maxResults))
                    {
                        suggestions.Add(letters);
                    }
                }

                foreach (char next in current.GetDict().Keys)
                {
                    GetMatches(current.GetNode(next), suggestions, maxResults, letters, prefix);
                }
            }
           

        }


    }
}