# FoxProApi - Batch and Blend

.NET 8 ASP.NET Core REST API built around the uploaded Visual FoxPro `batch.dbf` and `blend.dbf` structures.

## Actual tables

### batch.dbf

- FCBATCH Character(8)
- FCDESCRIPT Character(40)
- FCACTIVE Character(1)
- FCUSERLOCK Character(3)

### blend.dbf

- FCACTIVE Character(1)
- FCBLEND Character(12)
- FCCOMPATIB Character(1)
- FCDESCRIPT Character(40)
- FCGEOID Character(20)
- FCGLCODE Character(20)
- FCNAMECODE Character(8)
- FCUSERLOCK Character(3)
- FCVARIETY Character(5)
- FNVINTAGE Numeric(4)
- FCBLNDGRP Character(12)
- FCEXCEPTN Character(1)
- FMCOMMENTS Memo

## Endpoints

### Batches

GET `/api/batches`

GET `/api/batches?search=shiraz`

GET `/api/batches/SHPT`

POST `/api/batches`

PUT `/api/batches/SHPT`

DELETE `/api/batches/SHPT`

### Blends

GET `/api/blends`

GET `/api/blends?search=chardonnay`

GET `/api/blends/99LBCHR1`

POST `/api/blends`

PUT `/api/blends/99LBCHR1`

DELETE `/api/blends/99LBCHR1`

GET `/health`

## FoxPro configuration

Edit `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "FoxPro": "Provider=VFPOLEDB.1;Data Source=C:\\WMS\\Data;Collating Sequence=MACHINE;"
  }
}
```

If the tables are in a database container:

```text
Provider=VFPOLEDB.1;Data Source=C:\WMS\Data\YourDatabase.dbc;
```

## Important

The uploaded files establish the table/field structures, but the API does not assume that these files are the production database location.

Change `Data Source` to the actual directory containing your FoxPro tables.

## Memo field

`FMCOMMENTS` is handled separately as a FoxPro Memo parameter rather than treating it as a normal Character field.

## Search

Search is parameterized and currently searches:

Batch:
- FCBATCH
- FCDESCRIPT

Blend:
- FCBLEND
- FCDESCRIPT
- FCGEOID

## Running

```powershell
dotnet restore
dotnet build
dotnet run --project src/FoxProApi
```

Open Swagger:

`https://localhost:7045/swagger`

## VFPOLEDB

This API requires the Microsoft Visual FoxPro OLE DB provider and therefore is intended for Windows hosting.

Check the installed provider bitness before IIS deployment. If VFPOLEDB is 32-bit, the IIS application pool may need `Enable 32-Bit Applications = True`.

## Production

Before exposing write endpoints in production:

- add authentication/authorization
- add audit logging
- verify FoxPro file locking/concurrency behavior
- back up the DBF/CDX/FPT files
- test INSERT/UPDATE/DELETE against a copy first
- keep the API process on the same Windows environment as the FoxPro provider
