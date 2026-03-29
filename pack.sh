#!/usr/bin/env bash
set -e

[ -z $1 ] && echo "Missing project/version" && exit 1

IFS=/ read -r project version <<< $1
project=src/$project
version=${version/v}

dotnet clean -c Release
dotnet build -p:Version=${version} -c Release $project
dotnet pack $project -c Release -p:IncludeSymbols=true -p:SymbolPackageFormat=snupkg --include-source -p:PackageVersion=$version -p:Version=${version-*} -o ./artifacts
