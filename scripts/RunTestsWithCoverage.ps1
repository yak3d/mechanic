dotnet test --collect:"XPlat Code Coverage" --settings coverlet.runsettings
reportgenerator -reports:".\Mechanic.Core.Tests\TestResults\*\coverage.cobertura.xml" -targetdir:"coveragereport" -reporttypes:Html
