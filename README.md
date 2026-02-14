# HotTub Shop 24

ASP.NET Core MVC B2C-Webshop fuer Whirlpools mit:
- Produktkatalog mit Basismodellen
- flexiblen Zusatzbausteinen je Produkt
- zweisprachigem Frontend (de/en)
- einfachem Admin-Backend fuer Produkt- und Optionspflege
- JSON-Storage in `App_Data/catalog.json` (ohne externe DB)

## Start lokal

```powershell
dotnet run --project .\HotTubShop.Web\HotTubShop.Web.csproj
```

Aufruf: `https://localhost:xxxx/?lang=de` oder `?lang=en`.

## IIS Deployment (Windows Server)

1. .NET 8 Hosting Bundle auf dem IIS-Server installieren.
2. Publish erstellen:

```powershell
dotnet publish .\HotTubShop.Web\HotTubShop.Web.csproj -c Release -o .\publish
```

3. IIS Website auf den `publish`-Ordner zeigen lassen.
4. App Pool auf `No Managed Code` setzen.
5. Schreibrechte fuer den App-Pool-User auf `App_Data` vergeben (wegen Katalogpflege).

## Admin-Bereich

- `/Admin`
- Produkte: anlegen, bearbeiten, loeschen
- Optionen pro Produkt: anlegen, bearbeiten, loeschen

Hinweis: Aktuell ohne Authentifizierung. Fuer Produktion sollte mindestens Login + Rollenmodell ergaenzt werden.
