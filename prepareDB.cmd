@echo
dotnet ef database update &^
dotnet ef migrations add initialDB &^ 
dotnet ef database update &^
pause