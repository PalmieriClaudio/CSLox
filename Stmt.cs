using System.Collections.Generic;
namespace CSLox;

public abstract class Stmt
{

 public interface IVisitor<T>
 {
     T VisitBlockStmt(Block stmt);
     T VisitExpressionStmt(Expression stmt);
     T VisitPrintStmt(Print stmt);
     T VisitVarStmt(Var stmt);
 }

 public abstract T Accept<T>(IVisitor<T> visitor);

 public class Block:Stmt
 {
     public readonly List<Stmt> statements;

     public Block(List<Stmt> statements)
     {
         this.statements = statements;
     }

     public override T Accept<T>(IVisitor<T> visitor)
     {
         return visitor.VisitBlockStmt(this);
     }
 }
 public class Expression:Stmt
 {
     public readonly Expr expression;

     public Expression(Expr expression)
     {
         this.expression = expression;
     }

     public override T Accept<T>(IVisitor<T> visitor)
     {
         return visitor.VisitExpressionStmt(this);
     }
 }
 public class Print:Stmt
 {
     public readonly Expr expression;

     public Print(Expr expression)
     {
         this.expression = expression;
     }

     public override T Accept<T>(IVisitor<T> visitor)
     {
         return visitor.VisitPrintStmt(this);
     }
 }
 public class Var:Stmt
 {
     public readonly Token name;
     public readonly Expr? initializer;

     public Var(Token name, Expr? initializer)
     {
         this.name = name;
         this.initializer = initializer;
     }

     public override T Accept<T>(IVisitor<T> visitor)
     {
         return visitor.VisitVarStmt(this);
     }
 }
}
