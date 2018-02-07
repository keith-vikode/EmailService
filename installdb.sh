#!/bin/bash
pushd ./src/EmailService.Web
dotnet ef database update
cd
exit /b