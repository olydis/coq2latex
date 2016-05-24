Require Import Coq.Unicode.Utf8 Arith Bool Ring Setoid String.

Inductive SimpleType : Set :=
(*coq2latex: TInt := \texttt{int} *)
| TInt : SimpleType
(*coq2latex: TBool := \texttt{bool} *)
| TBool : SimpleType
(*coq2latex: TFunc #a #b := #a \rightarrow #b *)
| TFunc : SimpleType -> SimpleType -> SimpleType
.

Inductive SimpleValue : Set :=
(*coq2latex: VNum #n := #n *)
| VNum : nat -> SimpleValue
(*coq2latex: VTrue := \texttt{true} *)
| VTrue : SimpleValue
(*coq2latex: VFalse := \texttt{false} *)
| VFalse : SimpleValue
.

Definition SimpleVariable := string.

Inductive SimpleExpression : Set :=
(*coq2latex: EValue #v := #v *)
| EValue : SimpleValue -> SimpleExpression
(*coq2latex: EVariable #x := #x *)
| EVariable : SimpleVariable -> SimpleExpression
(*coq2latex: EApplication #a #b := #a~#b *)
| EApplication : SimpleExpression -> SimpleExpression -> SimpleExpression
(*coq2latex: ELambda #x #t #e := (\lambda #x : #t . #e) *)
| ELambda : SimpleVariable -> SimpleType -> SimpleExpression -> SimpleExpression
.

Definition SimpleEnvironment := SimpleVariable -> option SimpleType.

(*coq2latex: SimpleEnvironmentSet #G #x #t := #G, #x : #t *)
Definition SimpleEnvironmentSet
              (env : SimpleEnvironment)
              (x : SimpleVariable)
              (t : SimpleType)
              : SimpleEnvironment :=
                  fun y => if string_dec x y
                            then Some t
                            else env x.

(*coq2latex: SimpleExpressionHasType #G #e #t := #G \vdash #e : #t *)
Inductive SimpleExpressionHasType : SimpleEnvironment -> SimpleExpression -> SimpleType -> Prop :=
| TyNum : forall n Gamma(*\*), SimpleExpressionHasType Gamma (EValue (VNum n)) TInt
| TyTrue  : forall Gamma(*\*), SimpleExpressionHasType Gamma (EValue VTrue) TBool
| TyFalse : forall Gamma(*\*), SimpleExpressionHasType Gamma (EValue VFalse) TBool
| TyVariable : forall Gamma(*\*) tau(*\*) x,
    Gamma x = Some tau ->
    SimpleExpressionHasType Gamma (EVariable x) tau
| TyApplication : forall Gamma(*\*) e_0 e_1 tau(*\*) tau'(*\*),
    SimpleExpressionHasType Gamma e_0 (TFunc tau(*\*) tau'(*\*)) ->
    SimpleExpressionHasType Gamma e_1 tau ->
    SimpleExpressionHasType Gamma (EApplication e_0 e_1) tau'
| TyLambda : forall Gamma(*\*) x e tau(*\*) tau'(*\*),
    SimpleExpressionHasType (SimpleEnvironmentSet Gamma x tau) e tau' ->
    SimpleExpressionHasType Gamma (ELambda x tau e) (TFunc tau tau')
.


(*coq2latex: @Some #_ #x := #x *)
(*coq2latex: @eq #_ #a #b := #a = #b *)

