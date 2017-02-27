@ECHO OFF
SetLocal
set ASPNETCORE_ENVIRONMENT=Development
set ConnectionStrings:SqlServer=Server=(local); Database=EmailService; Integrated Security=true; MultipleActiveResultSets=True
set ConnectionStrings:Storage=UseDevelopmentStorage=true

pushd .\src\EmailService.Processor.Job
dotnet watch run
cd
exit /b