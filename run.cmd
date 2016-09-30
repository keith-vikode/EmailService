@ECHO OFF
SetLocal
set ASPNETCORE_ENVIRONMENT=Development

pushd .\src\%1
dotnet watch run %2
cd
exit /b