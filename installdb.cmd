@ECHO OFF
SetLocal

pushd .\src\EmailService.Web
dotnet ef database update
cd
exit /b