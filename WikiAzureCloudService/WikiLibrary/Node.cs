using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WikiLibrary
{
    /// <summary>
    /// One Node for the MemoryConciousTrie
    /// </summary>
    public class Node
    {
        private Node[] characters;
        private bool End;
        private const int TOTAL_CHARS = 28;
        private const int CHAR_OFFSET = 99;
        private const int CHAR_DIFFERENCE = 96;

        /// <summary>
        /// Adds a Node for the given character. Note, that you get the character by what index it is inserted into
        /// </summary>
        /// <param name="character"></param>
        public Node(char character) {
          this.characters = new Node[TOTAL_CHARS];
        }

        /// <summary>
        /// a node that is null.
        /// </summary>
        public Node()
            : this('\0')
        {

        }

        /// <summary>
        /// Returns the index that a character would be stored in
        /// </summary>
        /// <param name="character">the input character you are trying to convert to an index</param>
        /// <returns>an int that represents the index that a character would be</returns>
        private static int ConvertCharToInt(char character)
        {
            if (character == '\0')
            {
                return 0;
            }
            else if (character == ' ')
            {
                return TOTAL_CHARS - 1;
            }
            else
            {
                character = Char.ToLower(character);
                return character - CHAR_DIFFERENCE;
            }
        }

        /// <summary>
        ///     Returns whether the character is present in the given Node
        /// </summary>
        /// <param name="character"></param>
        /// <returns></returns>
        public bool CharPresent(char character) {
            return (characters[ConvertCharToInt(character)] != null);
        }

        /// <summary>
        ///     Returns a char based on what the index.
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public static char ConvertIntToChar(int code) {
            if (code == 0) {
                return '\0';
            }
            else if (code == TOTAL_CHARS - 1)
            {
                return ' ';
            }
            else
            {
                return (char)(CHAR_DIFFERENCE + code);
            }
        }

        /// <summary>
        ///  Adds a character to the given Node
        /// </summary>
        /// <param name="character"></param>
        public void AddNode(char character)
        {
            characters[ConvertCharToInt(character)] = new Node(character);
        }

        /// <summary>
        /// returns the Node (and thus its children) of the character
        /// </summary>
        /// <param name="character">the character you are searching for</param>
        /// <returns></returns>
        public Node GetNode(char character)
        {
            return this.characters[ConvertCharToInt(character)];
        }

        /// <summary>
        /// sets the boolean representing whether it is the end of a word or not
        /// </summary>
        /// <param name="isEnd"></param>
        public void SetEnd(bool isEnd)
        {
            this.End = isEnd;
        }

        /// <summary>
        /// returns whether or not the Node is the end of a title
        /// </summary>
        /// <returns></returns>
        public bool IsEnd()
        {
            return this.End;
        }

        /// <summary>
        /// returns the Node[] for this node
        /// </summary>
        /// <returns></returns>
        public Node[] GetArray()
        {
            return this.characters;
        }

        /// <summary>
        /// Converts the Node[] to a Char[]
        /// </summary>
        /// <returns></returns>
        public char[] GetChars()
        {
            char[] charBuilder = new char[TOTAL_CHARS];
            for (int i = 0; i < TOTAL_CHARS; i++)
            {
                if (characters[i] != null)
                {
                    charBuilder[i] = ConvertIntToChar(i);
                }
            }
            return charBuilder;
        }

    }
}