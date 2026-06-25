# Copilot Instructions

## General Guidelines
- Priorytet: stabilny, działający projekt.
- Projekt jest rozwijany długoterminowo i ma służyć jako starter na lata.
- Zmiany w konfiguracji, architekturze i refaktoryzacji wprowadzać ostrożnie, małymi krokami i z możliwością łatwego rollbacku.
- Użytkownik może tymczasowo zostawiać zakomentowany stary kod jako zabezpieczenie podczas większych zmian, dopóki nowe rozwiązanie nie zostanie potwierdzone testami.
- Złożone zagadnienia techniczne tłumaczyć prosto, praktycznie i krok po kroku.
- Odpowiedzi powinny wspierać rozwój wiedzy użytkownika w kierunku junior/mid developera: ASP.NET, React, TypeScript, C#, PostgreSQL.
- Użytkownik oczekuje ostrożnych rekomendacji przy konfiguracji i testach; gdy wcześniejsza porada okazuje się nietrafiona, należy jasno wskazać kontekst, w którym rozwiązanie jest potrzebne, zamiast sugerować globalne usuwanie.
- Nie wykonywać żadnych operacji Git bez wyraźnej, bezpośredniej prośby użytkownika. Dotyczy to w szczególności: zmiany brancha, tworzenia branchy, commitów, merge, rebase, cherry-pick, push, pull, reset oraz stash. Operacje Git użytkownik wykonuje samodzielnie.
- Preferować minimalne zmiany zamiast szerokich refaktoryzacji, jeśli nie są konieczne do rozwiązania problemu.
- Nie usuwać zakomentowanego kodu zabezpieczającego ani tymczasowych fallbacków bez wyraźnej prośby użytkownika lub bez potwierdzenia testami, że nie są już potrzebne.
- Przed uznaniem zadania za zakończone zawsze sprawdzić build oraz uruchomić testy adekwatne do zakresu zmian.
- Gdy istnieje kilka możliwych rozwiązań, wskazać krótkie plusy i minusy oraz zaznaczyć rekomendowany wariant.
- Nie dodawać nowych paczek, bibliotek ani narzędzi bez wyraźnej potrzeby i bez wyjaśnienia po co są potrzebne.
- Nie przenosić sekretów do repozytorium; preferować User Secrets, zmienne środowiskowe lub bezpieczną konfigurację lokalną.
- W zmianach konfiguracyjnych i bezpieczeństwa preferować rozwiązania stabilne, testowalne i łatwe do utrzymania długoterminowo.
- Jeśli porada dotyczy tylko testów, środowiska lokalnego albo tylko developmentu, należy to jasno zaznaczyć.
- Jeśli coś jest niepewne lub zależne od kontekstu projektu, nie zakładać na ślepo — najpierw sprawdzić kod i konfigurację.
- Nie zmieniać nazw, struktury folderów ani architektury projektu bez wyraźnej potrzeby i bez wskazania wpływu tej zmiany.