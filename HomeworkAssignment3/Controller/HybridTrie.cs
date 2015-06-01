using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;

namespace Controller
{
    public class HybridTrie
    {

        private HybridTrieNode root;
        public string lastWord = "";
        public int TrieCount = 0;
        public HybridTrie(string filepath)
        {
            root = new HybridTrieNode();
            root.SetAsRoot();
            try
            {
                using (StreamReader sr = new StreamReader(filepath))
                {
                    string word;

                    // PerformanceCounter ramCounter = new PerformanceCounter("Memory", "Available MBytes");
                    long count = 1;
                    while ((word = sr.ReadLine()) != null)
                    {
                            AddNode(root, word);
                            this.TrieCount++;
                            lastWord = word;
                           // Debug.WriteLine(count++ + " with " + ramCounter.NextValue() + "MB Left");
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
        }

        public void AddNode(HybridTrieNode current, string word)
        {
            if (word.Length > 0)
            {
                // get first character
                char letter = word[0];

                // if list is null
                if (current.list == null)
                {
                    // if dictionary !contain character
                    if (!current.dict.Keys.Contains(letter))
                    {
                        //add
                        current.dict.Add(letter, new HybridTrieNode());
                    } // else {already exists don't need to do anything}

                    if (word.Length > 1)
                    {
                        string rest = word.Substring(1);
                        AddNode(current.dict[letter], rest);
                    }
                    else if (word.Length == 1)
                    {
                        AddNode(current.dict[letter], "");
                    }
                    // Continuing adding word to Trie/List
                }
                else
                {
                    current.list.Add(word);

                    if (current.ShouldConvert())
                    {
                        foreach (string item in current.list)
                        {
                            if (item.Length > 1) {
                                letter = item[0];
                                if (!current.dict.Keys.Contains(letter))
                                {
                                    current.dict.Add(letter, new HybridTrieNode());
                                }

                                AddNode(current.dict[letter], item.Substring(1));
                            }
                            else if (item.Length == 1)
                            {
                                letter = item[0];
                                if (!current.dict.Keys.Contains(letter))
                                {
                                    current.dict.Add(letter, new HybridTrieNode());
                                }
                            }
                        }
                        current.list = null; //converted, get rid of list
                    }
                }
            }
            else
            {
                current.SetEnd(true);
            }
        }

        public HybridTrieNode SearchForPrefix(string prefix)
        {
            prefix = prefix.ToLowerInvariant();
            Queue<char> prefixBank = new Queue<char>();

            char[] bank = prefix.ToCharArray();
            foreach (char character in bank)
            {
                prefixBank.Enqueue(character);
            }
            Tuple<HybridTrieNode, string, string> placeHolderTuple = Search(root, prefixBank, "", -1, "");
            return placeHolderTuple.Item1;
        }

        public Tuple<HybridTrieNode, string, string> Search(HybridTrieNode current, Queue<char> queryQueue, string listMatch, int listIndex, string parent)
        {
            if (queryQueue.Count > 0)
            {
                char next = queryQueue.Dequeue();
                if (current.list == null)
                {
                    // Recurse Through Dictionary
                    //next = queryQueue.Dequeue();
                    if (current.dict.ContainsKey(next))
                    {
                        parent += next;
                        return Search(current.dict[next], queryQueue, listMatch, listIndex, parent);
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    listMatch += next;
                    for (int i = 0; i < current.list.Count; i++)
                    {
                        if (current.list[i].StartsWith(listMatch))
                        {
                            return Search(current, queryQueue, listMatch, i, parent);
                        }
                        else if (i == current.list.Count - 1)
                        {
                            return null; //end of list and not found
                        }
                    }
                    if (listIndex == -1)
                    {
                        return null; //Not Found
                    }
                }
            }
            return new Tuple<HybridTrieNode, string, string>(current, listMatch, parent);
        }

        //public string GetSuggestions(HybridTrieNode startingNode)
        //{

        //}

        public List<string> GetAllSuggestions(string prefix, int maxResults)
        {
            List<string> suggestions = new List<string>();

            Queue<char> prefixQueue = new Queue<char>();
            foreach (char character in prefix)
            {
                prefixQueue.Enqueue(character);
            }
            string listMatch = "";
            GetMatches(root, suggestions, maxResults, listMatch, prefixQueue, prefix, "");
            return suggestions;
        }

        public void GetMatches(HybridTrieNode current, List<string> suggestions, int maxResults, string listMatch, Queue<char> prefix, string original, string parent)
        {
            if (suggestions.Count >= maxResults)
            {
                return;
            }

            if (current == null)
            {
                return;
            }

            if (prefix.Count > 0)
            {
                //Get To starting point of DFS or figure out there is no suggestions
                //char nextLetter = prefix.Dequeue();
                //letters += nextLetter;
                Tuple<HybridTrieNode, string, string> searchResults = Search(current, prefix, listMatch, 0, "");
                if (searchResults != null)
                {
                    current = searchResults.Item1;
                    listMatch = searchResults.Item2;
                    parent = searchResults.Item3;
                }
                else
                {
                    current = null;
                }
               
                if (current == null)
                {
                    return; //will be no matches
                }
                else
                {
                    GetMatches(current, suggestions, maxResults, listMatch, prefix, original, parent);
                }
            }
            else
            {
                if (current.IsEnd())
                {
                    suggestions.Add(parent);
                }

                if (current.list == null)
                {
                    foreach (char next in current.dict.Keys)
                    {
                        GetMatches(current.dict[next], suggestions, maxResults, listMatch, prefix, original, parent + next);
                    }
                }
                else
                {
                    foreach (string item in current.list.Where(x => x.StartsWith(listMatch)))
                    {
                        suggestions.Add(parent + item);
                        if (suggestions.Count >= maxResults)
                        {
                            return;
                        }
                    }
                }
                
            }

        }
    }
}