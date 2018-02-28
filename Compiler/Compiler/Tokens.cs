using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler
{
    public static class Tokens
    {
        private static int tokenIndex = 0;
        private static Token[] tokens = { null, null };
        
        public static void reset()
        {
            tokenIndex = 0;
            tokens[0] = null;
            tokens[1] = null;
        }
        public static Token GetToken()
        {
            while(tokens[0] == null)
            {
                NextToken();
            }
            return tokens[0];
        }

        public static Token PeekToken()
        {
            while(tokens[0] == null)
            {
                NextToken();
            }
            while (tokens[1] == null)
            {
                tokens[1] = RequestToken();
            }
            return tokens[1];
        }

        public static void NextToken()
        {
            if (tokens[1] == null)
            {
                tokens[1] = RequestToken();
            }
            tokens[0] = tokens[1];
            tokens[1] = null;
        }

        private static Token RequestToken()
        {
            return Scanner.GetToken(ref tokenIndex);
        }
    }
}
