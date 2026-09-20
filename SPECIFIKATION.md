# Specifikation – Yatzy

## Dokumentoplysninger

| Punkt | Beskrivelse |
| --- | --- |
| Type | Software Design Description (SDD) |
| Teknologi | C#, WPF og .NET 10 |
| Formål | Afleveringsopgave: et lokalt Yatzy-spil |
| Status | Klar til implementering, når specifikationen er godkendt |

## Formål

Programmet er et WPF-spil til 1–6 lokale menneskelige spillere. Computeren kan frivilligt tilføjes som modspiller og er slået til som standard. Spillerne skiftes ved samme computer, og højeste samlede score efter 15 runder vinder.

## Afgrænsning og valg

- Spillet bruger fem sekssidede terninger.
- Spillet følger de officielle danske Yatzy-regler, som de er præciseret i dette dokument. Ingen spiller, heller ikke Computeren, må bruge handlinger eller pointregler uden for disse regler.
- En tur består af op til tre kast. Før første kast må der hverken holdes terninger eller vælges scorefelt. Efter første og andet kast kan spilleren klikke på terninger for at holde eller frigive dem.
- Efter et kast vælges ét ledigt scorefelt. Et felt må kun bruges én gang.
- Spillet følger den almindelige danske Yatzy-scoretavle: enere til seksere, ét par, to par, tre ens, fire ens, lille straight, stor straight, fuldt hus, chance og yatzy.
- Bonussen er 50 point, hvis den øvre sektion (enere til seksere) giver mindst 63 point.
- En ledig kategori må altid vælges. Giver den 0 point, er det en streg, som bekræftes af spilleren. En tur kan ikke springes over.
- To par kræver to forskellige værdier. Fem ens tæller som par, tre ens og fire ens, men ikke som to par eller fuldt hus.
- Spillet er lokalt på én computer. Netværksspil, onlinekonti, reklamer og eksterne tjenester indgår ikke i opgaven.

## Begreber

| Begreb | Betydning |
| --- | --- |
| Kast | Ét kast med alle terninger, der ikke er holdt. |
| Tur | Én spillers forløb med højst tre kast og ét valgt scorefelt. |
| Runde | Når alle spillere har gennemført én tur. Der spilles 15 runder. |
| Holdt terning | En terning, som beholder sin værdi ved næste kast. |
| Streg | Et valgt scorefelt med 0 point. |
| Stabil tilstand | En tilstand efter en færdig spillerhandling, som sikkert kan gemmes og genoptages. |

## Funktionelle krav

1. Ved start vælges antal menneskelige spillere fra 1 til 6; standardvalget er 1. Derefter vises præcis det valgte antal navnefelter, hvor spillernavne kan indtastes, før spillet startes. Når brugeren forsøger at starte spillet, kontrolleres hvert menneskeligt navn; mangler et navn et synligt tegn, vises en fejlbesked, og spillet startes ikke. Brugeren vælger med en afkrydsningsboks, om Computeren skal deltage; boksen er markeret som standard. Når Computeren deltager, kan dens navn ændres.
2. Ens spillernavne er tilladt. Ved oprettelse tildeles hver spiller et unikt, internt spiller-id sammen med sit viste navn, så spilleren kan identificeres entydigt i spillet og ved en mulig senere netværksudvidelse. Spiller-id’et vises ikke i brugerfladen.
3. På hver tur kastes alle ikke-holdte terninger, og resultatet vises.
4. Spilleren kan efter kast ét og to holde terninger, kaste igen eller vælge scorefelt; efter tredje kast skal et felt vælges.
5. Programmet viser ledige scorefelter og den score, som det aktuelle kast vil give den aktuelle spiller.
6. Når et felt vælges, gemmes scoren, terninger nulstilles, og turen går til næste spiller.
7. Når alle spillere har udfyldt 15 felter, vises vinder(e), subtotaler, bonus og total.
8. Computeren gennemfører sin tur automatisk efter samme regler som et menneske: højst tre kast, kun synlige terningeværdier som grundlag for valg og ét ledigt scorefelt pr. tur.
9. Computerens tur vises trinvis med en tænkepause mellem kast, valg af holdte terninger og valg af scorefelt. Pausens varighed tilpasses menneskelige spilleres observerede beslutningstid i spillet. Menneskelige kontroller er deaktiveret, mens computerens tur afvikles.
10. Et igangværende spil gemmes lokalt, så det kan genoptages efter at programmet er lukket.
11. Hvis et gemt spil findes ved opstart, spørger programmet “Vil du fortsætte det gemte spil?” med valgene Ja og Nej. Ved Nej spørger programmet derefter “Vil du slette det gemte spil?”. Svar Ja sletter gemte spil og åbner startvinduet; svar Nej bevarer spillet og åbner startvinduet.
12. Når et spil afsluttes normalt, slettes den gemte spiltilstand automatisk. Slutskærmen med resultaterne vises fortsat.
13. Et igangværende spil gemmes automatisk efter hver færdig spilhandling: når et kast er afsluttet, når en terning holdes eller frigives, og når et scorefelt vælges. Der gemmes ikke midt i en rulle-animation eller anden overgang, så et genoptaget spil altid starter i en stabil tilstand.
14. Hvis programmet lukkes under Computerens tur, genoptages spillet fra Computerens seneste stabile handling. Computeren gennemfører derefter resten af sin tur automatisk efter de almindelige regler.
15. Spillerne tager tur i den rækkefølge, deres navne indtastes. Når Computeren deltager, placeres den sidst i turordenen.

## Scoreregler

| Felt | Point |
| --- | --- |
| Enere–seksere | Summen af terninger med den valgte værdi |
| Ét par | Summen af det højeste mulige par; fx giver 3-3-3-2-2 seks point |
| To par | Summen af to forskellige par; ellers 0 |
| Tre/fire ens | Summen af tre/fire ens af samme værdi |
| Lille/stor straight | 15 for 1-2-3-4-5 / 20 for 2-3-4-5-6, ellers 0 |
| Fuldt hus | Summen af et par og tre ens, ellers 0 |
| Chance | Summen af alle terninger |
| Yatzy | 50 for fem ens, ellers 0 |

## Ikke-funktionelle krav

- Selve programmet skal være dansk, bruge WPF og køre med `.NET 10` uden eksterne pakker. Testprojektet må bruge xUnit.
- Scoringslogik og spilmotor skal være uafhængige af brugerfladen. Tilfældighed abstraheres via `IDiceRoller`, så et helt spil kan testes med forudbestemte kast.
- Tilstanden må ikke kunne indeholde et scorefelt flere gange.
- Computerens fælles læringsmønstre, brugerindstillinger for lyd og rulle-animation samt et eventuelt igangværende spil gemmes lokalt i JSON-filer. Filerne læses ved opstart og opdateres ved ændringer med .NETs indbyggede JSON-understøttelse; de kræver hverken database eller eksterne pakker.
- Hvis JSON-filen mangler, er tom eller ikke kan læses, starter programmet med standardindstillinger og tom læring uden at gå ned.

## Acceptkriterier

- Et fuldt spil kan gennemføres i 15 ture pr. spiller uden nedbrud.
- Et scorefelt kan ikke vælges to gange.
- Kendte kast scorer korrekt, fx giver `6,6,6,6,6` 50 i Yatzy og `1,2,3,4,5` 15 i lille straight.
- Spillet kan ikke oprettes uden 1–6 spillere, og kastknappen er deaktiveret efter tredje kast.
- Holdte terninger ændrer ikke værdi ved næste kast, og et 0-points felt kan vælges som streg.

## Brugerflade

- Startvinduet indeholder en vælger til antal menneskelige spillere (1–6), hvor 1 er forvalgt, samt den forvalgte afkrydsningsboks “Spil mod computeren”. Ved ændring af spillertallet vises præcis så mange navnefelter. Ydre mellemrum fjernes automatisk, og flere sammenhængende mellemrum inde i et navn samles til ét mellemrum, før navnet valideres og gemmes. Navne må højst indeholde 20 tegn, skal indeholde mindst ét andet tegn end mellemrum og må kun indeholde bogstaver, tal, mellemrum, bindestreg, understregning og udråbstegn. Emojis, linjeskift, tabulatorer og øvrige symboltegn afvises. Ens navne er tilladt. Ved et ugyldigt navn vises en dansk fejlbesked, når brugeren klikker “Start spil”, og hvert ugyldigt navnefelt markeres med en rød kant. Markeringen fjernes straks, når feltet igen er gyldigt. Når boksen er markeret, vises et valgfrit navnefelt til computermodspilleren; et tomt felt giver navnet “Computeren”.
- Hovedvinduet viser fem klikbare, klassiske terninger med prikker, den aktuelle spillers viste navn, runde og antal kast tilbage.
- En holdt terning har både en tydelig farvet baggrund og en tydelig ramme. Markeringen fjernes igen, når terningen frigives eller turen slutter.
- Når der kastes, vises en kort visuel rulle-animation på de terninger, der ikke er holdt. Holdte terninger står helt stille og bevarer deres viste værdi. De endelige værdier vises efter animationen. Terninger og kastknap er deaktiveret, mens animationen kører.
- Rulle-animationen er slået til som standard, men kan slås fra i indstillingerne. Når den er slået fra, vises kastets endelige værdier med det samme.
- Lyd er slået fra som standard, men kan slås til eller fra i indstillingerne. Når lyd er slået til, afspilles en kort lyd ved terningekast og ved Yatzy.
- Hovedvinduet indeholder en enkel menu eller knap med navnet “Indstillinger”. Her kan brugeren slå lyd og rulle-animation til eller fra samt vælge “Nulstil computerens læring”.
- Pointtavlen har én kolonne pr. spiller og tilpasser dynamisk kolonnebredder, tekststørrelse og layout til den aktuelle skærm- og vinduesstørrelse, så alle spillerkolonner kan ses uden vandret rulning. Kun ledige felter i den aktuelle spillers kolonne kan vælges; et klik gemmer det viste pointforslag.
- Udfyldte felter vises som faste point. Udfyldningsforslag vises kun for den aktuelle spiller.
- Bonus vises som 50 straks ved mindst 63 i øvre sektion; når den øvre sektion er færdig uden 63 point, vises 0.
- Efter et 0-points valg vises “Vil du stryge {kategori}?” med Ja/Nej for menneskelige spillere. Ved Nej gemmes intet, og terninger samt kastnummer bevares, mens spilleren vælger et andet ledigt scorefelt. Var det tredje kast, kan spilleren ikke kaste igen. Når Computeren vælger en streg, bekræfter den automatisk uden en dialog; brugeren ser kun det valgte scorefelt.
- Ved afslutning vises en slutskærm med alle spillere sorteret efter samlet score, tydeligt markerede vindere og den komplette pointtavle for alle spillere. “Nyt spil” er altid tilgængeligt. Mens et spil er i gang, vises ved klik dialogen “Er du sikker på, at du vil afbryde spillet og starte et nyt?”. Først ved bekræftelse afbrydes en eventuel animation eller computerhandling, den gamle gemte spiltilstand slettes, og der vendes tilbage til startvinduet; ellers fortsætter det igangværende spil uændret. Efter et afsluttet spil åbner “Nyt spil” startvinduet direkte uden en bekræftelsesdialog.
- Hvis brugeren lukker programvinduet, mens et spil er i gang, vises dialogen “Er du sikker på, at du vil afslutte? Det igangværende spil gemmes og kan fortsættes senere.” Ved Nej forbliver programmet åbent.
- Computerens kast, holdte terninger og valgte scorefelt vises i hovedvinduet, så brugeren kan følge dens tur.
- Under computerens tænkepause vises en tydelig status, fx “Computeren tænker…”.
- Computeren forklarer ikke sine strategiske valg i tekst. Brugeren ser alene kast, holdte terninger og det valgte scorefelt, på samme måde som ved en menneskelig modspiller.

## Computerstrategi

Computeren spiller efter samme regler og med samme information som en menneskelig spiller. Alt, en menneskelig spiller lovligt kan gøre, kan Computeren også gøre — herunder at holde eller frigive terninger, afslutte en tur før tredje kast og vælge en streg på 0 point. Den må ikke se fremtidige terningekast eller manipulere tilfældigheden. Den vælger holdte terninger og scorefelt ud fra det aktuelle kast og sine ledige kategorier med målet om en høj score. Den bruger ikke nødvendigvis alle tre kast: efter første eller andet kast må den vælge et scorefelt, når det er et rimeligt menneskeligt valg; ellers kaster den videre op til tre gange.

Computeren bruger altid den lærende, menneskelignende strategi, der beskrives nedenfor.

Computeren må observere de menneskelige medspilleres afsluttede ture på samme måde som en menneskelig modspiller kan. Den registrerer derfor, hvilke terninger de holder, og hvilke kategorier de vælger. Disse observationer samles på tværs af alle menneskelige spillere og bruges kun som fælles mønstre til at justere computerens prioritering mellem ellers lige gode, lovlige valg. Computeren opretter ikke personprofiler og forsøger ikke at lære bestemte spillere at kende. Læringen gemmes lokalt mellem spil på den samme computer, men bruger ingen eksterne data eller tjenester.

Brugeren kan fra start- eller hovedvinduet vælge “Nulstil computerens læring”. Handlingen sletter kun de gemte spillestilsmønstre og kræver en bekræftelse.

Computeren lærer også gruppens tempo ved at måle tiden fra et menneskeligt valg bliver muligt, til handlingen udføres. Den bruger et løbende gennemsnit som tænkepause for tilsvarende computerhandlinger. Pausen begrænses til et rimeligt interval, så spillet hverken føles øjeblikkeligt eller unødigt langsomt.

## Brugerforløb

1. Ved opstart kan brugeren fortsætte et gemt spil eller gå til startvinduet.
2. I startvinduet vælges antal menneskelige spillere, navne og om Computeren deltager.
3. Spillet viser den aktuelle spiller. Spilleren kaster terningerne, holder eventuelt terninger og vælger et scorefelt.
4. Ved 0 point bekræfter spilleren, om feltet skal stryges. Derefter skifter turen.
5. Når Computeren har tur, vises dens kast, holdte terninger og valgte felt trinvis, mens menneskelige kontroller er deaktiveret.
6. Efter 15 udfyldte scorefelter pr. spiller vises resultatskærmen med vinder(e), pointtavle og totaler.

## Arkitektur og data

Programmet opdeles i tre lag, så spilregler kan testes uden brugerfladen:

| Lag | Ansvar |
| --- | --- |
| Yatzy.Core | Spiltilstand, turregler, terninger, scoreark, scoreberegning, computerstrategi og lagringsmodeller. |
| Yatzy.App | WPF-vinduer, visning, animation, lyd, dialoger og brugerinput. |
| Yatzy.Tests | xUnit-tests af især Core-lagets regler og forudbestemte terningekast. |

En spiller består mindst af et unikt internt spiller-id, et vist navn, typen menneske eller computer og et scoreark. Scorearket indeholder højst én score for hver kategori. Terninger har en værdi fra 1 til 6 og en holdt-status.

Indstillinger, computerens fælles læringsmønstre og et igangværende spil gemmes lokalt som JSON med .NETs indbyggede JSON-understøttelse. Et gemt spil indeholder den stabile spiltilstand, herunder spillere, scoreark, aktuelle terningeværdier, holdte terninger, tur, runde og kastnummer. Manglende, tom eller ulæselig JSON må aldrig forhindre programmet i at starte; i stedet bruges standardindstillinger og tom læring.

## Automatiske testcases

Følgende cases implementeres som xUnit-tests. Terningernes rækkefølge må ikke ændre resultatet.

| Kategori | Terninger | Forventet point |
| --- | --- | ---: |
| Enere | 1,1,2,3,1 | 3 |
| Femmere | 5,5,5,2,1 | 15 |
| Seksere | 1,2,3,4,5 | 0 |
| Ét par | 3,3,5,5,1 | 10 |
| Ét par | 2,2,2,1,1 | 4 |
| Ét par | 1,2,3,4,6 | 0 |
| To par | 3,3,5,5,1 | 16 |
| To par | 2,2,3,3,3 | 10 |
| To par | 4,4,4,4,1 | 0 |
| Tre ens | 4,4,4,1,2 | 12 |
| Tre ens | 6,6,6,6,6 | 18 |
| Tre ens | 1,2,3,4,6 | 0 |
| Fire ens | 3,3,3,3,5 | 12 |
| Fire ens | 2,2,2,2,2 | 8 |
| Fire ens | 3,3,3,5,5 | 0 |
| Lille straight | 1,2,3,4,5 | 15 |
| Lille straight | 2,3,4,5,6 | 0 |
| Stor straight | 2,3,4,5,6 | 20 |
| Stor straight | 1,2,3,4,5 | 0 |
| Fuldt hus | 2,2,3,3,3 | 13 |
| Fuldt hus | 4,4,4,4,4 | 0 |
| Fuldt hus | 2,2,3,3,4 | 0 |
| Chance | 1,2,3,5,6 | 17 |
| Yatzy | 4,4,4,4,4 | 50 |
| Yatzy | 4,4,4,4,3 | 0 |

Derudover testes, at 62 i øvre sektion giver 0 bonus, 63 giver 50, en kategori ikke kan udfyldes to gange, og at en falsk `IDiceRoller` kan drive et helt spil med holdte terninger.

## Testplan

- Alle rækker i testtabellen køres som xUnit-tests, også når terningernes rækkefølge ændres.
- Turregler testes: ingen score eller hold før første kast, højst tre kast, holdte terninger ændres ikke, og tredje kast kræver et scorevalg.
- Spiltilstanden testes for dubletter, streg og afslutning efter 15 felter pr. spiller.
- Computerens valg testes for samme lovlige handlinger og begrænsninger som et menneskes.
- Gemte spil testes for genoptagelse fra en stabil tilstand og sikker håndtering af manglende eller ugyldig JSON.
- Brugerfladen gennemgås manuelt for navnevalidering, dansk tekst, responsivt layout, animation, lydindstilling, dialoger og visning af computerens tur. Det kontrolleres også, at “Nyt spil” spørger under et igangværende spil, men åbner startvinduet direkte efter et afsluttet spil.

## Projektstruktur og faser

```text
Yatzy/
├── src/Yatzy.Core/     spillogik uden WPF-afhængighed
├── src/Yatzy.App/      WPF-brugerflade
└── tests/Yatzy.Tests/  xUnit-tests
```

1. Opret solution og projekter samt `.gitignore`; byg løsningen.
2. Implementér `ScoreCategory` og `ScoreCalculator`; tilføj hele testtabellen.
3. Implementér `ScoreSheet`, `Player`, `Die`, `IDiceRoller` og `Game`; test turregler, bonus, streg og afslutning.
4. Implementér JSON-lagring af indstillinger, læring og igangværende spil; test genoptagelse og fejlrobusthed.
5. Implementér computerstrategi, fælles læring og tænkepauser; test at den kun foretager lovlige valg.
6. Implementér WPF-startvindue, navnevalidering og valg af computer.
7. Implementér hovedvindue, klassiske terninger, hold-status, pointtavle, animation, lyd og indstillinger.
8. Implementér slutresultat, uafgjort, nyt spil og alle bekræftelsesdialoger; gennemfør manuel test.
9. Oprydning, README og endelig build/test.

Hver fase afsluttes med `dotnet build`, `dotnet test` hvor relevant, og en dansk Git-commit.

## Definition af færdig

Opgaven er færdig, når løsningen kan bygges, alle automatiske tests består, et helt spil kan gennemføres i brugerfladen, og alle krav i denne specifikation er verificeret ved test eller manuel gennemgang.

## Kravsporbarhed

| Kravområde | Verifikation |
| --- | --- |
| Scoreregler og bonus | xUnit-testtabellen samt bonustests |
| Turregler, hold og streg | xUnit-tests med falsk `IDiceRoller` |
| Navne, dialoger og dansk brugerflade | Manuel gennemgang |
| Computerens tur og lovlige valg | Enhedstests og manuel gennemgang |
| Gem, genoptag og sletning af spil | Enhedstests af JSON-lagring og manuel gennemgang |
| Responsivt layout, animation og lyd | Manuel gennemgang ved forskellige vinduesstørrelser |
