../../src/bin/debug/netcoreapp3.1/SSDTBuilder.exe `
--verbose `
--dacpac "dac.dacpac" `
--project "DemoDatabase.sqlproj" `
--profile "PublishProfile.xml" `
--output "c:\temp\output" `
--script "Scripts\00_schema.sql" `
--copy-loose `
--target-db-name "MyDatabase" `
--remove-sqlcmd-variables false
