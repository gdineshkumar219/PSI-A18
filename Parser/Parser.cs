namespace PSI;
using static Token.E;

public class Parser {
   // Interface -------------------------------------------
   public Parser (Tokenizer tokenizer) 
      => mToken = mPrevious = (mTokenizer = tokenizer).Next ();

   public NExpr Parse () {
      var node = Expression ();
      if (mToken.Kind != EOF) throw new Exception ($"Unexpected {mToken}");
      return node;
   }

   // Implementation --------------------------------------
   // expression = equality .
   NExpr Expression () 
      => Equality ();

   // equality = equality = comparison [ ("=" | "<>") comparison ] .
   NExpr Equality () {
      var expr = Comparison ();
      if (Match (EQ, NEQ))
         expr = new NBinary (expr, Prev, Comparison ());
      return expr;
   }

   // comparison = term [ ("<" | "<=" | ">" | ">=") term ] .
   NExpr Comparison () {
      var expr = Term ();
      if (Match (LT, LEQ, GT, GEQ))
         expr = new NBinary (expr, Prev, Term ());
      return expr;
   }

   // term = factor { ( "+" | "-" | "or" ) factor } .
   NExpr Term () {
      var expr = Factor ();
      while  (Match (ADD, SUB, OR)) 
         expr = new NBinary (expr, Prev, Factor ());
      return expr;
   }

   // factor = unary { ( "*" | "/" | "and" | "mod" ) unary } .
   NExpr Factor () {
      var expr = Unary ();
      while (Match (MUL, DIV, AND, MOD)) 
         expr = new NBinary (expr, Prev, Unary ());
      return expr;
   }

   // unary = ( "-" | "+" ) unary | primary .
   NExpr Unary () {
      if (Match (ADD, SUB)) 
         return new NUnary (Prev, Unary ());
      return Primary ();
   }

   // primary = IDENTIFIER | INTEGER | REAL | STRING | "(" expression ")" | "not" primary .
   NExpr Primary1 () {
      if (Match (IDENT)) return new NIdentifier (Prev);
      if (Match (INTEGER, REAL, BOOLEAN, CHAR, STRING)) return new NLiteral (Prev);
      if (Match (NOT)) return new NUnary (Prev, Primary ());
      Expect (OPEN, "Expecting identifier or literal");
      var expr = Expression ();
      Expect (CLOSE, "Expecting ')'");
      return expr;
   }
   // Modify the Primary() routine to handle function calls
   NExpr Primary () {
      if (Match (IDENT) && Peek (OPEN)) {
         Token functionName = Prev;
         Expect (OPEN, "Expecting '(' after function name");

         List<NExpr> parameters = new();
         if (!Peek (CLOSE)) {
            parameters.Add (Expression ());
            while (Match (COMMA))
               parameters.Add (Expression ());
         }

         Expect (CLOSE, "Expecting ')' after function arguments");
         return new NFnCall (functionName, parameters.ToArray());
      }
      NExpr expr = Expression ();

      // Handle other cases (variable reference, literals, etc.)
      if (Match (IDENT)) return new NIdentifier (Prev);
      if (Match (INTEGER, REAL, BOOLEAN, CHAR, STRING)) return new NLiteral (Prev);
      if (Match (NOT)) return new NUnary (Prev, Primary ());
      if (Match (OPEN)) {
         Expect (CLOSE, "Expecting ')'");
      }
         return expr;
      

      //throw new Exception ($"Unexpected token: {mToken}");
   }
   // Like match, but does not consume the token
   bool Peek (params Token.E[] kinds) => kinds.Contains (mToken.Kind);


   // Helpers ---------------------------------------------
   // Expect to find a particular token
   void Expect (Token.E kind, string message) {
      if (!Match (kind)) throw new Exception (message);
   }

   // Match and consume a token on match
   bool Match (params Token.E[] kinds) {
      if (kinds.Contains (mToken.Kind)) {
         mPrevious = mToken;
         mToken = mTokenizer.Next ();
         return true;
      }
      return false;
   }

   // The 'previous' token we found
   Token Prev => mPrevious;

   // Private data ---------------------------------------
   Token mToken, mPrevious;
   Tokenizer mTokenizer;
}