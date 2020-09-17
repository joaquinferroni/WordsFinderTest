using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TestWordFinder
{
    public class WordFinder
    {
        private const int ROW_SIZE = 64;
        private const int COLUMN_SIZE = 64;
        private string[,] _matrix = new string[ROW_SIZE, COLUMN_SIZE];
        private Dictionary<string, int> WordsCounter = new Dictionary<string, int>();

        public WordFinder(IEnumerable<string> matrix)
        {
            FillMatrix(matrix);
            PrintMatrix();
        }

        /// <summary>
        /// Iterate the words in parallel in order to check the time of words finding.
        /// Problem: Not thread safe, if the list of words has repited words, it will throw an exception because try to add twice inside the dictionary at the same time with the same key
        /// </summary>
        /// <param name="wordsStream"></param>
        /// <returns></returns>
        public IEnumerable<string> FindParallel(IEnumerable<string> wordsStream)
        {
            Console.WriteLine($"Total words to find: {wordsStream.Count()}");
            var rowSize = _matrix.GetLength(0);
            var columnSize = _matrix.GetLength(1);
            // First iterate the matrix and after the words in order to be more performant
            for (var i = 0; i < rowSize; i++)
            {
                for (var j = 0; j < columnSize; j++)
                {
                    Parallel.ForEach(wordsStream,(word)=>{
                        bool wordExists = ExistsHorizontal(i, j, 0, word.ToLower())
                                    || ExistsVertical(i,j,0, word.ToLower());
                        if(wordExists){
                            if(WordsCounter.TryGetValue(word, out var counter))
                                WordsCounter[word] = counter+1;
                            else
                                WordsCounter.Add(word,1);
                        }
                    });
                }
            }
            return WordsCounter.OrderByDescending(wc => wc.Value).Select(wc => wc.Key);
        }
        /// <summary>
        /// Iterates the words not in parallel, in order to compare with the parallel way   
        /// </summary>
        /// <param name="wordsStream"></param>
        /// <returns></returns>
        public IEnumerable<string> Find(IEnumerable<string> wordsStream)
        {
            Console.WriteLine($"Total words to find: {wordsStream.Count()}");
            var rowSize = _matrix.GetLength(0);
            var columnSize = _matrix.GetLength(1);
            // First iterate the matrix and after the words in order to be more performant
            for (var i = 0; i < rowSize; i++)
            {
                for (var j = 0; j < columnSize; j++)
                {
                    foreach (var word in wordsStream)
                    {
                        bool wordExists = ExistsHorizontal(i, j, 0, word.ToLower())
                                    || ExistsVertical(i,j,0,word.ToLower());
                        if(wordExists){
                            if(WordsCounter.TryGetValue(word, out var counter))
                                WordsCounter[word] = counter+1;
                            else
                                WordsCounter.Add(word,1);
                        }
                    }
                }
            }
            return WordsCounter.OrderByDescending(wc => wc.Value).Select(wc => wc.Key);
        }

        /// <summary>
        /// Check if the word exist from left ot right
        /// </summary>
        /// <param name="i">row to ckeck (never change)</param>
        /// <param name="j">column to ckeck</param>
        /// <param name="indexWordToCompare">index of the letter to compare</param>
        /// <param name="word">word to find</param>
        /// <returns></returns>
        private bool ExistsHorizontal(int i, int j, int indexWordToCompare, string word)
        {
            var letter = word[indexWordToCompare]; //take the char to compare
            //if the char is not equal or if the matrix length is finished, return false, the word is not in the horizontal
            if (j == _matrix.GetLength(1)
                || letter != _matrix[i, j][0]
            )
            {
                return false;
            }
            //if the letter is in the index of the matrix and the word is fully compared, the word exists horizontally
            if (letter == _matrix[i, j][0]
                && indexWordToCompare == word.Length - 1
                )
            {
                return true;
            }
            // if the letter exists in the current row-column matrix but is not fully compared, recall the method to continue comparing
            return ExistsHorizontal(i, j + 1, indexWordToCompare + 1, word);
        }

        /// <summary>
        /// Check if the word exists from top to bottom
        /// </summary>
        /// <param name="i">row to ckeck</param>
        /// <param name="j">column to ckeck (never change)</param>
        /// <param name="indexWordToCompare">index of the letter to compare</param>
        /// <param name="word">word to find</param>
        /// <returns></returns>
        private bool ExistsVertical(int i, int j, int indexWordToCompare, string word)
        {
            var letter = word[indexWordToCompare]; 
            if (i == _matrix.GetLength(0)
                || letter != _matrix[i, j][0]
            )
            {
                return false;
            }
            if (letter == _matrix[i, j][0]
                && indexWordToCompare == word.Length - 1
                )
            {
                return true;
            }
            return ExistsVertical(i+1, j , indexWordToCompare + 1, word);
        }

        /// <summary>
        /// Take the enumerable and fill a matrix in order to be more readable and iterable
        /// </summary>
        /// <param name="matrix"></param>
        private void FillMatrix(IEnumerable<string> matrix)
        {
            for (var i = 0; i < _matrix.GetLength(0); i++)
            {
                string[] newLine = matrix.Skip(i * ROW_SIZE).Take(ROW_SIZE).ToArray();

                for (var j = 0; j < newLine.Length; j++)
                {
                    _matrix[i, j] = newLine[j].ToLower();
                }
            }
        }

        /// <summary>
        /// Print the matrix in Console
        /// </summary>
        private void PrintMatrix()
        {
            for (int i = 0; i < _matrix.GetLength(0); i++)
            {
                for (int j = 0; j < _matrix.GetLength(1); j++)
                {
                    Console.Write($"{ _matrix[i, j]} ");
                }
                Console.WriteLine();
            }
        }

    }
}