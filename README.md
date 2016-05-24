# coq2latex

> **tl;wr** - clone repo, goto `examples`, launch `simplyTyped.[sh/bat]`, see how it works, adjust to your satisfaction

## What is it?
A simple tool to auto-generate LaTeX inference rules from inductive propositions in your Coq file.
![Figure](https://raw.githubusercontent.com/olydis/coq2latex/master/doc/readmeFigure.png)

## Why would I need that?
- You're working on a paper/thesis/presentation/... and are sick of **doing the same work twice**
- Like your fellow humans, you tend to **misspell** variable names every once in a while. Coq will complain, LaTeX won't.
- You adjusted some rules and are now getting a headache finding the corresponding spots in that LaTeX-spaghetti code.
- Or you just hate writing LaTeX.

## Walkthrough
...of the example you can find at `examples/simplyTyped`.
#### Step 1: Locate an inductive proposition
``` Coq
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
```
#### Step 2: Run the tool (`coq2latex.[sh/bat]`)
Run 

    <tool> <coq-file (without .v extension)> <name of inductive proposition>
    
**from the directory containing the coq-file**.
In the above example, this would be 

    <tool> simplyTyped SimpleExpressionHasType
    
LaTeX should be thrown at you (meant to be redirected to a `tex`-file). 

If not, make sure that `coqtop` is in your path (or modify `coq2latex.[sh/bat]` so it uses the absolute path) and that the `coq2latex` binary is compiled and found.

#### Step 3: Compile the LaTeX
What it should look like:

![Figure](https://raw.githubusercontent.com/olydis/coq2latex/master/doc/pdfAnnot0.png)

> But wait, that looks horrible!

> Indeed!

Reason: `coq2latex` won't guess what an `ELambda`, `env`, `TInt` or `SimpleExpressionHasType` is supposed to look like.
Instead, it assumes that everything is a predicate/function and renders it accordingly (this is a nice fallback as we will see later).

Let's fix that.

#### Step 4: Provide translation hints
``` Coq
(*coq2latex: SimpleExpressionHasType #G #e #t := #G \vdash #e : #t *)
Inductive SimpleExpressionHasType : SimpleEnvironment -> SimpleExpression -> SimpleType -> Prop :=
| TyNum : forall n env(*\Gamma*), SimpleExpressionHasType env (EValue (VNum n)) TInt
| TyTrue  : forall env(*\Gamma*), SimpleExpressionHasType env (EValue VTrue) TBool
| TyFalse : forall env(*\Gamma*), SimpleExpressionHasType env (EValue VFalse) TBool
| TyVariable : forall env(*\Gamma*) tau(*\*) x,
    env x = Some tau ->
    SimpleExpressionHasType env (EVariable x) tau
| TyApplication : forall env(*\Gamma*) e_0 e_1 tau(*\*) tau'(*\*),
    SimpleExpressionHasType env e_0 (TFunc tau(*\*) tau'(*\*)) ->
    SimpleExpressionHasType env e_1 tau ->
    SimpleExpressionHasType env (EApplication e_0 e_1) tau'
| TyLambda : forall env(*\Gamma*) x e tau(*\*) tau'(*\*),
    SimpleExpressionHasType (SimpleEnvironmentSet env x tau) e tau' ->
    SimpleExpressionHasType env (ELambda x tau e) (TFunc tau tau')
.
```

There are two kinds of translation hint you can provide (see below for more detailed documentation):

1. rewrite rules for relations: `(* coq2latex: <relation name> <parameters> := <LaTeX pattern> *)`
2. alternative names for bound variables: `<original variable name>(*<LaTeX alternative>*)`
   
   - giving just `\` as the LaTeX alternative will simply treat the variable name as a LaTeX command, see `tau(*\*)`

#### Step 5: Redo from **Step 2** until satisfied
Until it looks something like this:

![Figure](https://raw.githubusercontent.com/olydis/coq2latex/master/doc/pdfAnnot1.png)

Note that in rule `TyVariable` it is still the "fallback" that renders `env x` as `\Gamma(x)`.

### Documentation
#### TODO
   Each parameter is either
   - a variable `#<some name>` (that matches anything and can be used in the LaTeX pattern) or
   - a Coq expression (that must be matched in order for the rewrite rule to apply)
      - Example: 


