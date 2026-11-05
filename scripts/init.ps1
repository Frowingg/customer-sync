# Generates the solution and projects with the official dotnet templates.
# Run once, from the repository root:  .\scripts\init.ps1
# Requires: .NET 10 SDK, git.

$ErrorActionPreference = "Stop"

function Run($cmd) {
    Write-Host ">> $cmd" -ForegroundColor Cyan
    Invoke-Expression $cmd
    if ($LASTEXITCODE -ne 0) { throw "Command failed: $cmd" }
}

Run "dotnet --list-sdks"

# Solution and repo hygiene
Run "dotnet new sln -n CustomerSync"
Run "dotnet new gitignore"
Run "dotnet new editorconfig"
Add-Content .gitignore "`n# Local secrets`n.env"

# Source projects
Run "dotnet new web       -n Legacy.CustomerService       -o src/Legacy.CustomerService       -f net10.0"
Run "dotnet new classlib  -n CustomerSync.Core            -o src/CustomerSync.Core            -f net10.0"
Run "dotnet new classlib  -n CustomerSync.Infrastructure  -o src/CustomerSync.Infrastructure  -f net10.0"
Run "dotnet new webapi    -n CustomerSync.Api             -o src/CustomerSync.Api             -f net10.0 --use-controllers"

# Test projects
Run "dotnet new xunit     -n CustomerSync.Core.Tests      -o tests/CustomerSync.Core.Tests    -f net10.0"
Run "dotnet new xunit     -n CustomerSync.Api.Tests       -o tests/CustomerSync.Api.Tests     -f net10.0"

# Add everything to the solution
Get-ChildItem -Recurse src,tests -Filter *.csproj | ForEach-Object { Run "dotnet sln add `"$($_.FullName)`"" }

# Project references (dependencies point inward)
Run "dotnet add src/CustomerSync.Infrastructure reference src/CustomerSync.Core"
Run "dotnet add src/CustomerSync.Api reference src/CustomerSync.Core src/CustomerSync.Infrastructure"
Run "dotnet add tests/CustomerSync.Core.Tests reference src/CustomerSync.Core"
Run "dotnet add tests/CustomerSync.Api.Tests reference src/CustomerSync.Api src/CustomerSync.Core"

# NuGet packages
Run "dotnet add src/Legacy.CustomerService package CoreWCF.Http"
Run "dotnet add src/Legacy.CustomerService package CoreWCF.Primitives"
Run "dotnet add src/CustomerSync.Infrastructure package Microsoft.EntityFrameworkCore.SqlServer --version 10.*"
Run "dotnet add src/CustomerSync.Infrastructure package System.ServiceModel.Http"
Run "dotnet add src/CustomerSync.Api package Microsoft.EntityFrameworkCore.Design --version 10.*"
Run "dotnet add tests/CustomerSync.Api.Tests package Microsoft.AspNetCore.Mvc.Testing --version 10.*"

Run "dotnet build"

Write-Host ""
Write-Host "Done. Next: git init, first commit, push to GitHub." -ForegroundColor Green
