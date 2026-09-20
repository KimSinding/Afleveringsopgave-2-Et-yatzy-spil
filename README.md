# Yatzy

Et dansk Yatzy-spil til 1–6 lokale menneskelige spillere med en valgfri computer-modspiller. Programmet er skrevet i C# med WPF og .NET 10.

Projektets krav og designbeslutninger er beskrevet i [SPECIFIKATION.md](SPECIFIKATION.md). Specifikationen er projektets gældende SDD.

## Start spillet

Dobbelklik på `Start-Yatzy.cmd` i projektmappen. Det bygger og starter WPF-programmet.

Alternativt kan programmet startes fra en terminal i projektmappen:

```powershell
dotnet run --project src/Yatzy.App/Yatzy.App.csproj
```

## Kør testene

```powershell
dotnet test Yatzy.slnx
```

Testprojektet indeholder automatiske xUnit-tests af blandt andet scoreberegning, bonus, turregler, holdte terninger, computerstrategi og JSON-lagring.

## Funktioner

- Klassiske terninger med prikker og op til tre kast pr. tur.
- Hold og frigiv terninger efter første og andet kast.
- Dansk Yatzy-scoretavle med 15 kategorier og bonus ved mindst 63 point i øvre sektion.
- Bekræftelse, før et menneske stryger en kategori med 0 point.
- Computer-modspiller, som gennemfører sine ture automatisk og lærer af fælles spillestil og tempo.
- Automatisk lokal lagring af indstillinger, computerlæring og et igangværende spil som JSON.
- Indstillinger for rulle-animation og lyd. Animation er slået til som standard, lyd er slået fra.
- Resultatvindue med sorteret placering og håndtering af uafgjort.

## Projektstruktur

```text
src/Yatzy.Core/   Spilmotor, scoringsregler og lagring uden WPF-afhængighed
src/Yatzy.App/    WPF-brugerflade
tests/Yatzy.Tests/ xUnit-tests
```

## Lokal data

Indstillinger, computerlæring og igangværende spil gemmes lokalt i brugerens `AppData/Local/Yatzy`-mappe. Ingen data sendes til eksterne tjenester.
