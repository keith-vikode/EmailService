#!/bin/bash
export ASPNETCORE_ENVIRONMENT=Development

pushd ./src/EmailService.Web
dotnet watch run --server.urls http://localhost:2411;https://localhost:44320
cd
exit /b