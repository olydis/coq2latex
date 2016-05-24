@echo off
((echo Set Printing Depth 2. Load simplyTyped. Set Printing All. Set Printing Width 2097151. Set Printing Depth 100. Print SimpleExpressionHasType. Quit.^
    | coqtop.exe) && type simplyTyped.v)^
    | ..\project\coq2latex\bin\Debug\coq2latex.exe SimpleExpressionHasType^
    > SimpleExpressionHasType.tex
pdflatex simplyTyped.tex
start simplyTyped.pdf