# Phrazy

Phrazy is a word game inspired by classic hangman. It's built using .Net Blazor on the front end, a groovy webassembly platform that is fast and works everywhere.

## Running it locally

First you'll need to set up a SQL Server database locally. The schema for it is in the `Database.sql` file that's in the root of the `Phrazy.Server` project. Once that's done, you can run that project, which hosts both the API and the Blazor client.

The `Phrazy Functions` project contains an Azure Function called `DailyRanker` that runs once a day to calculate the rankings of the players from the previous day. It also has a manual implementation called `ManualRanker` that you can fire by hitting `http://localhost:7071/api/ManualRank` or something similar (check the output of the functions when you run the project). To run functions locally, you'll need to have an Azure Storage simulator running, like Azureite.

If you like this, check out MLocker, a Blazor-based personal cloud music player: https://github.com/POPWorldMedia/MLocker