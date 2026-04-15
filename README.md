
# MeteorApp

App Meteo è un'applicazione multipiattaforma sviluppata con .NET MAUI che permette di consultare le condizioni meteo attuali e le previsioni per 5 giorni di una o più città, utilizzando le API di Open-Meteo.

## Funzionalità

- Ricerca meteo per una o più città (separate da virgola)
- Visualizzazione delle condizioni meteo attuali (temperatura, vento, info)
- Visualizzazione delle previsioni per i prossimi 5 giorni (min/max, pioggia)
- Supporto per Android, iOS, Windows e Mac
- Icone meteo dinamiche in base alla temperatura
- Gestione errori (città non trovata, problemi di rete)
- Cache locale delle richieste per migliorare le prestazioni

## Requisiti

- .NET 10 SDK o superiore
- Connessione Internet
- Visual Studio 2022/2025 con workload MAUI installato (per compilare su desktop/mobile)

## Installazione e Avvio

1. Clona o scarica il repository.
2. Apri la soluzione `App Meteo.sln` con Visual Studio.
3. Seleziona la piattaforma di destinazione (Android, Windows, ecc.).
4. Ripristina i pacchetti NuGet:
	```
	dotnet restore
	```
5. Premi "Esegui" oppure usa i comandi:
	```
	dotnet build
	dotnet run --project AppMeteoMAUI
	```
6. Inserisci una o più città e premi cerca.

> **Nota:** Tutte le librerie necessarie (incluso .NET MAUI e le sue dipendenze) vengono scaricate automaticamente tramite NuGet quando esegui `dotnet restore`. Non è necessario includerle manualmente nel repository o nella cartella del progetto.

## Risoluzione dei Problemi

- **Errore città non trovata:** Verifica la correttezza del nome inserito.
- **Problemi di rete:** Assicurati di avere una connessione Internet attiva.
- **Problemi di build:** Verifica di avere installato il workload MAUI e il .NET SDK richiesto.
- **API non raggiungibile:** Le API di Open-Meteo devono essere disponibili pubblicamente.

## Personalizzazione

- Puoi modificare le icone, i colori e lo stile in `Resources/` e `Styles/`.
- La logica di recupero dati si trova in `Logic/MeteoService.cs` e `Logic/PrevisioneService.cs`.
