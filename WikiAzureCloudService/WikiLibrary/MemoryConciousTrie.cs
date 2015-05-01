using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;

namespace WikiLibrary
{
    /// <summary>
    ///     The MemoryConciousTree is an implimentation that attempts to minimize the memory usage
    /// </summary>
    public class MemoryConciousTrie
    {
        private const int MIN_MEMORY = 50;
        private Node root;
        public int Count;

        /// <summary>
        /// Builds a Trie Data Structure from a file.
        /// </summary>
        /// <param name="filepath">pass in the filename that you want to build thr Trie From</param>
        public MemoryConciousTrie(string filepath)
        {
            this.root = new Node();
            try
            {
                using (StreamReader sr = new StreamReader(filepath))
                {
                    string title;

                    PerformanceCounter ramCounter = new PerformanceCounter("Memory", "Available MBytes");
                    while ((title = sr.ReadLine()) != null)
                    {
                        if (this.Count % 1000 == 0)
                        {
                            if (ramCounter.NextValue() <= MIN_MEMORY)
                            {
                                break;
                            }
                        }
                        Queue<char> titleInChars = FormatTitle(title);
                        if (titleInChars != null)
                        {
                            AddTitle(titleInChars);
                            this.Count++;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        /// <summary>
        /// returns a Queue of chars
        /// </summary>
        /// <param name="title">pass in the title</param>
        /// <returns>Queue of the characters from the title</returns>
        public Queue<char> FormatTitle(string title)
        {
            
            Queue<char> charsToAdd = new Queue<char>();
            char[] bank = title.ToCharArray();
            foreach (char character in bank)
            {
                if ((character >= 65 && character <= 90) || (character >= 97 && character <= 122) || character == 32)
                {
                    charsToAdd.Enqueue(Char.ToLower(character));
                }
                else
                {
                    return null; //Filter out, not a-zA-Z || ' '
                }
                
            }
            return charsToAdd;
        }

        /// <summary>
        /// adds a title to the trie
        /// </summary>
        /// <param name="title">a Queue full of characters</param>
        public void AddTitle(Queue<char> title)
        {
            AddNodes(root, title);
        }

        /// <summary>
        ///     Adds a child node
        /// </summary>
        /// <param name="current">the parent</param>
        /// <param name="titleCharacters">the characters to be added</param>
        /// <returns></returns>
        public Node AddNodes(Node current, Queue<char> titleCharacters)
        {
            if (titleCharacters.Count != 0)
            {
                if (!current.CharPresent(titleCharacters.Peek())) //Doesn't have next character, we need to create it
                {
                    current.AddNode(titleCharacters.Peek()); // Add A TrieNode from current -> DEQUEUE (should be next)
                }
                current = current.GetNode(titleCharacters.Dequeue()); // move to the next node
                AddNodes(current, titleCharacters); // current = 
            }
            else
            {
                current.SetEnd(true);
            }
            return current;
        }

        /// <summary>
        /// returns a list of query suggestions (i.e. possible autocompletetions)
        /// </summary>
        /// <param name="prefix">starting word</param>
        /// <param name="maxResults">max size of list being returned</param>
        /// <returns></returns>
        public List<string> GetAllSuggestions(string prefix, int maxResults)
        {
            List<string> suggestions = new List<string>();
            // TrieNode endOfPrefix = SearchForPrefix(prefix);
            Queue<char> prefixQueue = new Queue<char>();
            foreach (char character in prefix)
            {
                prefixQueue.Enqueue(character);
            }
            GetMatches(root, suggestions, maxResults, "", prefixQueue);
            //foreach (string suggestion in suggestions)
            //{
            //    Console.WriteLine(suggestion);
            //}
            return suggestions;
        }

        /// <summary>
        ///     helper function to getallsuggestions
        /// </summary>
        /// <param name="current">current node</param>
        /// <param name="suggestions"> list that stores all the suggestions</param>
        /// <param name="maxResults">max number of results being returned</param>
        /// <param name="letters">current word being built up for a suggestion</param>
        /// <param name="prefix">a Queue of characters that must be a prefix in the suggestion</param>
        public void GetMatches(Node current, List<string> suggestions, int maxResults, string letters, Queue<char> prefix)
        {

            // if exceeded max, return
            if (suggestions.Count >= maxResults)
            {
                return;
            }

            //if current null, return

            if (current == null)
            {
                return;
            }

            //get keys of current <They are the next options>
            char[] next = current.GetChars();
            // if prefix isn't empty recurse down that path by dequeueing from the prefix
            if (prefix.Count > 0)
            {
                //Add prefix's character to the letters, then recurse down
                char nextLetter = prefix.Dequeue();
                letters += nextLetter;
                if (current.CharPresent(nextLetter))
                {
                    GetMatches(current.GetNode(nextLetter), suggestions, maxResults, letters, prefix);
                }
            }
            else
            {
                if (current.IsEnd())
                {
                    suggestions.Add(letters);
                }

                int size = next.Length;

                for(int i = 0; i < size; i++)
                {
                    if (next[i] != '\0')
                    {
                       GetMatches(current.GetNode(next[i]), suggestions, maxResults, letters + next[i], prefix);
                    }
                }
            }
        }
    }
}