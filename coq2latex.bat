@echo off
REM === Usage: coq2latex.bat <coq-file (without .v extension)> <name of inductive proposition>
(((echo Set Printing Depth 2. Load %1. Set Printing All. Set Printing Width 2097151. Set Printing Depth 100. Print %2. Quit. | coqtop.exe) && type %1.v) | %~dp0\project\coq2latex\bin\Debug\coq2latex.exe %2) 2> nul