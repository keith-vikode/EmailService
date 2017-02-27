#!/bin/bash
export ASPNETCORE_ENVIRONMENT=Development
export ConnectionStrings:SqlServer=Server=(local); Database=EmailService; Integrated Security=true; MultipleActiveResultSets=True
export ConnectionStrings:Storage=UseDevelopmentStorage=true

pushd ./src/EmailService.Processor.Job
dotnet watch run
cd
exit /b