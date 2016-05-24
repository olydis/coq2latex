Require Import Coq.Unicode.Utf8 Arith Bool Ring Setoid String.

Inductive SimpleType : Set :=
| TInt : SimpleType
| TBool : SimpleType
| TFunc : SimpleType -> SimpleType -> SimpleType
.

Inductive SimpleValue : Set :=
| VNum : nat -> SimpleValue
| VTrue : SimpleValue
| VFalse : SimpleValue
.

Definition SimpleVariable := string.

Inductive SimpleExpression : Set :=
| EValue : SimpleValue -> SimpleExpression
| EVariable : SimpleVariable -> SimpleExpression
| EApplication : SimpleExpression -> SimpleExpression -> SimpleExpression
| ELambda : SimpleVariable -> SimpleType -> SimpleExpression -> SimpleExpression
.

Definition SimpleEnvironment := SimpleVariable -> option SimpleType.

Definition SimpleEnvironmentSet
              (env : SimpleEnvironment)
              (x : SimpleVariable)
              (t : SimpleType)
              : SimpleEnvironment :=
                  fun y => if string_dec x y
                            then Some t
                            else env x.

Inductive SimpleExpressionHasType : SimpleEnvironment -> SimpleExpression -> SimpleType -> Prop :=
| TyNum : forall n env, SimpleExpressionHasType env (EValue (VNum n)) TInt
| TyTrue  : forall env, SimpleExpressionHasType env (EValue VTrue) TBool
| TyFalse : forall env, SimpleExpressionHasType env (EValue VFalse) TBool
| TyVariable : forall env tau x,
    env x = Some tau ->
    SimpleExpressionHasType env (EVariable x) tau
| TyApplication : forall env e_0 e_1 tau tau',
    SimpleExpressionHasType env e_0 (TFunc tau tau') ->
    SimpleExpressionHasType env e_1 tau ->
    SimpleExpressionHasType env (EApplication e_0 e_1) tau'
| TyLambda : forall env x e tau tau',
    SimpleExpressionHasType (SimpleEnvironmentSet env x tau) e tau' ->
    SimpleExpressionHasType env (ELambda x tau e) (TFunc tau tau')
.
