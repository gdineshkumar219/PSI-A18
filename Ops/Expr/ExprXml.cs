using PSI;
using System.Xml.Linq;

namespace Ops.Expr {
   public class ExprXml : Visitor<XElement> {

      public ExprXml (string expression) => mExpression = expression;

      public override XElement Visit (NLiteral lit) => CreateNode ("Literal", ("value", lit.Value), ("type", lit.Type));

      public override XElement Visit (NIdentifier identifier) => CreateNode ("Identifier", ("value", identifier.Name.Text), ("type", identifier.Type));

      public override XElement Visit (NUnary unary) => CreateNode ("Unary", ("Operator", unary.Op.Kind), ("Type", unary.Type), unary.Expr.Accept (this));

      public override XElement Visit (NBinary binary) => CreateNode ("Binary", ("Operator", binary.Op.Kind), ("Type", binary.Type), binary.Left.Accept (this), binary.Right.Accept (this));

      public void SaveTo (string file, XElement n) => File.WriteAllText (file, new XElement ("Expression", n).ToString ());

      XElement CreateNode (string nodeName, params object[] attributes) {
         var node = new XElement (nodeName);
         foreach (var value in attributes)
            switch (value) {
               case XElement child: node.Add (child); break;
               case (string Name, object Value): node.SetAttributeValue (Name, Value); break;
               default: node.SetValue (value); break;
            }
         return node;
      }
      string mExpression;
   }
}