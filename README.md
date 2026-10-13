# Mai26ma-Grupp1-BlackJack

## Vad ni ska bygga

Ett spelbart tjugoett i konsolen, för en till fem spelare mot en givare.

Programmet ska gå att spela igenom från start till slut utan att krascha. Spelarnas resultat ska
sparas till fil så att de finns kvar nästa gång programmet startas.

Reglerna står nedan, fullständigt. Det är de reglerna som gäller — inte den variant ni spelat någon
annanstans.

## Ordlista

Klasser, metoder och variabler skrivs på engelska. Allt användaren ser skrivs på svenska.

| Engelska | Svenska | Noteringar |
|---|---|---|
| suit | färg | Hjärter, ruter, klöver, spader |
| rank | valör | Två till ess |
| deck | kortlek | |
| to shuffle | blanda | |
| to deal | ge, dela ut | |
| hand | hand | |
| dealer | givaren | |
| hole card | hålkortet | Givarens dolda kort |
| upcard | öppet kort | Givarens synliga kort |
| face card | klätt kort | Knekt, dam, kung |
| hit | köpa | |
| stand | stanna | |
| bust | spräcka | |
| push | lika | |
| soft / hard hand | mjuk / hård hand | |
| blackjack, natural | naturlig tjugoett | |
| chips | marker | Tillval 4 |
| bet | insats | Tillval 4 |
| payout | utbetalning | Tillval 4 |
| double down | dubbla | Överkurs |
| split | dela par | Överkurs |
| insurance | försäkring | Tillval 4, om tid finns |
| shoe | sko | Tillval 5 |
| cut card | skiljekort | Tillval 5 |

## Spelets regler

### Mål

Få en hand vars värde kommer så nära 21 som möjligt utan att överstiga 21, och som är högre än
givarens hand.

### Kortens värde

| Kort | Värde |
|---|---|
| 2–10 | Sitt eget värde |
| Knekt, dam, kung | 10 |
| Ess | 1 eller 11 — det värde som är bäst för handen utan att spräcka |

En hand där ett ess räknas som 11 kallas mjuk. ♥A + ♠6 är en mjuk 17. Drar man ♦9 till den blir
handen A + 6 + 9, och nu måste esset räknas som 1: en hård 16. Esset byter alltså värde under
handens gång. Det är den detalj som ställer till det i koden om man räknar värdet en gång och sparar
det.

### Giv

1. Varje spelare får två kort, öppet.
2. Givaren får två kort: ett öppet och ett dolt. Det dolda kortet kallas hålkortet.
3. Har en spelare 21 på sina två första kort — ess plus ett tiokort — har hen naturlig tjugoett.

### Spelarnas turer

Spelarna spelar en i taget, i tur och ordning. Varje spelare väljer upprepade gånger mellan:

- **Köpa** – ta ett kort till.
- **Stanna** – avsluta sin tur.

Går handen över 21 har spelaren spräckt. Handen är förlorad omedelbart, oavsett vad givaren sedan
gör.

### Givarens tur

Givaren spelar efter att alla spelare är klara och följer en fast regel — givaren väljer aldrig:

1. Vänd hålkortet.
2. Köp så länge handens värde är 16 eller lägre.
3. Stanna på 17 eller högre, även om det är en mjuk 17.

Givaren tittar alltså inte på hålkortet i förväg, inte ens när det öppna kortet är ett ess eller ett
tiokort. Rundan spelas alltid färdigt.

### Avgörande

Varje spelare jämförs med givaren för sig:

| Situation | Utfall |
|---|---|
| Spelaren har spräckt | Förlust, även om givaren också spräcker |
| Givaren har spräckt, spelaren har inte | Vinst |
| Ingen har spräckt | Högst hand vinner |
| Lika värde | Lika — rundan räknas varken som vinst eller förlust |
| Naturlig tjugoett mot en hand på 21 av tre kort eller fler | Den naturliga vinner |
| Naturlig tjugoett på båda sidor | Lika |

### Kortleken

Grundversionen använder en kortlek om 52 kort. När färre än 15 kort är kvar tas en ny, blandad lek
fram inför nästa runda.

Blandningen ska vara riktig. Använd Fisher–Yates: gå baklänges genom leken och byt plats på varje
kort med ett slumpmässigt valt kort bland dem som ligger före. Skapa en `Random` som delas av hela
programmet — skapar ni en ny varje gång ni blandar får ni samma lek om och om igen, eftersom den
såddas från klockan.

### Vad grundversionen inte innehåller

Inga marker och inga insatser — programmet räknar bara vunna, förlorade och lika rundor per
spelare. Inga satsningar, ingen försäkring, inget dubblande och ingen delning av par. Flera av de
sakerna finns som tillval.
