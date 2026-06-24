namespace CSLox;

public abstract class Expr
{

 public interface IVisitor<T>
 {
     T VisitBinaryExpr(Binary expr);
     T VisitGroupingExpr(Grouping expr);
     T VisitLiteralExpr(Literal expr);
     T VisitUnaryExpr(Unary expr);
 }

 public abstract T Accept<T>(IVisitor<T> visitor);

 public class Binary:Expr
 {
     public readonly Expr left;
     public readonly Token opr;
     public readonly Expr right;

     public Binary(Expr left, Token opr, Expr right)
     {
         this.left = left;
         this.opr = opr;
         this.right = right;
     }

     public override T Accept<T>(IVisitor<T> visitor)
     {
         return visitor.VisitBinaryExpr(this);
     }
 }
 public class Grouping:Expr
 {
     public readonly Expr expression;

     public Grouping(Expr expression)
     {
         this.expression = expression;
     }

     public override T Accept<T>(IVisitor<T> visitor)
     {
         return visitor.VisitGroupingExpr(this);
     }
 }
 public class Literal:Expr
 {
     public readonly object? value;

     public Literal(object? value)
     {
         this.value = value;
     }

     public override T Accept<T>(IVisitor<T> visitor)
     {
         return visitor.VisitLiteralExpr(this);
     }
 }
 public class Unary:Expr
 {
     public readonly Token opr;
     public readonly Expr right;

     public Unary(Token opr, Expr right)
     {
         this.opr = opr;
         this.right = right;
     }

     public override T Accept<T>(IVisitor<T> visitor)
     {
         return visitor.VisitUnaryExpr(this);
     }
 }
}
