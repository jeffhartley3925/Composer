using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace Composer.Messaging
{
    public static class Compression
    {
        //TODO: refactor. see all NOTE entries in this class.

        //NOTE: currently compressing a sample compostion with a base64 raw character count of 4.5 million to a compressed character count of 
        //389,000 - 91%. can improve this by changing substituted strings.
        private enum CompressionMode { Compress, Decompress };
        private static CompressionMode mode;

        //defaultSubstitutionChars contains no characters in the base64 charcter set. see CullSubstitutionChars below.

        //NOTE: the number of defaultSubstitutionChars must equal the number of substitutedStrings because of the array reversals in Decompress.
        //NOTE: all upper and lower case letters, plus all numbers, plus '+' and '/' are in the base64 character set. don't use them.
        //NOTE: Don't use ',' since I use join the lists with a ',' delimitter in order to pass them around. 
        //NOTE: not necessary to pass substitutionChars and substitutedStrings around anymore anymore since the number of defaultSubstitutionChars and substitutedStrings
        //are always identical.
        private static string[] defaultChars = new string[] { ".", "{", ";", "!", "$", ":", "*", "(", "<", "?", "'", "|", "}", "[", "]", "_", ">", "&", "#", "^" };
        private static ReadOnlyCollection<String> defaultSubstitutionChars = new ReadOnlyCollection<String>(defaultChars);

        private static List<string> substitutionChars = null;

        private static List<string> substitutedStrings = null;

        private static string target = "";
        public static string Target
        {
            get { return target; }
            set
            {
                target = value;
                CullSubstitutionChars();
                Initialize();
            }
        }

        private static void CullSubstitutionChars()
        {
            //can't use characters that exist in the compression target as substitution characters
            //defaultSubstitutionChars includes only non base64 characters, but I didn't know that when I wrote this code, so there really isn't a reason to cull
            //the list. left it in the flow anyway.
			substitutionChars = new List<string>();
            foreach (string character in defaultSubstitutionChars)
            {
                if (Target.IndexOf(character) < 0 || mode == CompressionMode.Decompress)
                {
                    substitutionChars.Add(character);
                }
            }
        }

        public static Message Compress(string target)
        {
            mode = CompressionMode.Compress;
            Target = target;
            Message message = new Message();
            
            string compressedBase64 = Target;
            int substitutionIndex = 0;
            if (!string.IsNullOrEmpty(Target))
            {
                foreach (string substitudedString in substitutedStrings)
                {
                    compressedBase64 = compressedBase64.Replace(substitudedString, substitutionChars[substitutionIndex]);
                    substitutionIndex++;
                }
            }
            message.Text = compressedBase64;
            return message;
        }

        public static string Decompress(string target)
        {
            mode = CompressionMode.Decompress;
            Target = target;
            string result = Target;
            //must decompress in the opposite order of the compression
            substitutionChars.Reverse();
            substitutedStrings.Reverse();
            int substitutionIndex = 0;
            if (!string.IsNullOrEmpty(Target))
            {
                foreach (string substitudedString in substitutedStrings)
                {
                    result = result.Replace(substitutionChars[substitutionIndex], substitudedString);
                    substitutionIndex++;
                }
            }
            substitutionChars.Reverse();
            substitutedStrings.Reverse();
            return result;
        }

        private static void Initialize()
        {
            //TODO: put these somewhere else and pass them in.
            //NOTE: the number of defaultSubstitutionChars must equal the number of substitutedStrings because of the array reversals in Decompress.
            //NOTE: the substitutionChars.Count is not really required. see CullSubstitutionChars() above. left in te flow anyway - can't hurt.
            substitutedStrings = new List<string>();
            if (substitutionChars.Count > 0 || mode == CompressionMode.Decompress) substitutedStrings.Add(@"/v7+//7+/v/");
            if (substitutionChars.Count > 1 || mode == CompressionMode.Decompress) substitutedStrings.Add(@"+/v7/");
            if (substitutionChars.Count > 2 || mode == CompressionMode.Decompress) substitutedStrings.Add(@".}.}.}.}.}.}.}.}.}.}.}.}.}.}.}.}.}.}.}.}.}.}.}.}.}.}.}.}.}.}.}.}.}");
            if (substitutionChars.Count > 3 || mode == CompressionMode.Decompress) substitutedStrings.Add(@"/AAAA/wAAAP8");
            if (substitutionChars.Count > 4 || mode == CompressionMode.Decompress) substitutedStrings.Add(@"AAAD");
            if (substitutionChars.Count > 5 || mode == CompressionMode.Decompress) substitutedStrings.Add(@"$!$!$!$!$!$!$!$!$!$!$!$!$!$!$!$!$!$!$!$!$!$!$!$!$!$!$!$!$!$!$!$!$!$!$!$!$!$!$!$");
            if (substitutionChars.Count > 6 || mode == CompressionMode.Decompress) substitutedStrings.Add(@"AA");
            if (substitutionChars.Count > 7 || mode == CompressionMode.Decompress) substitutedStrings.Add(@"//");
            if (substitutionChars.Count > 8 || mode == CompressionMode.Decompress) substitutedStrings.Add(@"((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((");
            if (substitutionChars.Count > 9 || mode == CompressionMode.Decompress) substitutedStrings.Add(@"*******************************************************************************************************************");
            if (substitutionChars.Count > 10 || mode == CompressionMode.Decompress) substitutedStrings.Add(@"((");
            if (substitutionChars.Count > 11 || mode == CompressionMode.Decompress) substitutedStrings.Add(@"**");
            if (substitutionChars.Count > 12 || mode == CompressionMode.Decompress) substitutedStrings.Add(@"MzMz");
            if (substitutionChars.Count > 13 || mode == CompressionMode.Decompress) substitutedStrings.Add(@"8zMzP");
            if (substitutionChars.Count > 14 || mode == CompressionMode.Decompress) substitutedStrings.Add(@"@@");
            if (substitutionChars.Count > 15 || mode == CompressionMode.Decompress) substitutedStrings.Add(@"`````");
            if (substitutionChars.Count > 16 || mode == CompressionMode.Decompress) substitutedStrings.Add(@"/zMzM/");
            if (substitutionChars.Count > 17 || mode == CompressionMode.Decompress) substitutedStrings.Add(@"????");
            if (substitutionChars.Count > 18 || mode == CompressionMode.Decompress) substitutedStrings.Add(@"||||");
            if (substitutionChars.Count > 19 || mode == CompressionMode.Decompress) substitutedStrings.Add(@"((((");
        }
    }
}