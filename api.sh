#!/bin/bash
export ASPNETCORE_ENVIRONMENT=Development

pushd ./src/EmailService.Web.Api
dotnet watch run --server.urls http://localhost:16794;https://localhost:44321
cd
exit /b