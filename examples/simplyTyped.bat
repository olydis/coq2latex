@echo off
call ..\coq2latex.bat simplyTyped SimpleExpressionHasType > SimpleExpressionHasType.tex
pdflatex simplyTyped.tex
start simplyTyped.pdf