#!/bin/bash
cd src/dotnet
export EF_MIGRATION=1

dotnet ef migrations remove --startup-project ./Server/eConnect.Identities.Server --project ./Server/eConnect.Identities.Server.EntityFramework

unset EF_MIGRATION
