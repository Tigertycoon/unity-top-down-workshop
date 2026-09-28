# Kurzleitfaden für Betreuer

## Vorbereiten

Öffne `Workshop_Template` über **Workshop → Startszene öffnen**. Teste Bewegung, Sammeln und Ziel einmal selbst. Dupliziere anschließend die Szene über das Betreuer-Panel und speichere für jede Gruppe eine eigene Datei. Änderungen im Play Mode gehen beim Stoppen normalerweise verloren: Zum Bauen zuerst Play stoppen.

Das blaue Standardsprite steht für den Spieler, hellblaue Objekte werden gesammelt, farbige Gefahren schubsen zurück und das grüne Ziel schließt die Mission ab. Eigene Sprites lassen sich später über den jeweiligen SpriteRenderer zuweisen. Die Kamera folgt dem Spieler; die Startszene begrenzt den begehbaren Bereich.

## Ablaufvorschlag

| Zeitrahmen | Schwerpunkt |
| --- | --- |
| 60 Minuten | Mission benennen, wenige Sammelobjekte platzieren, Wege und Gefahren ausprobieren, gegenseitig testen |
| 90 Minuten | Zusätzlich einen Schalter mit einer Tür verbinden und dessen Wirkung erklären |
| 120 Minuten | Einen Checkpoint und einen zweiten Bereich ergänzen, Rückmeldungen aus einem zweiten Test einarbeiten |

Das sind Vorschläge für die Planung. Passe Umfang und Tempo an Erfahrung, Alter und Gruppe an. Die Zeitvorgaben sind keine Evaluation des Lernerfolgs.

## Bauen und prüfen

- Im **Kinder-Panel** Objekte hinzufügen und anschließend mit dem Verschiebewerkzeug platzieren.
- Missionstitel und Auftrag kurz halten: Was wird gesammelt, wo ist das Ziel?
- Bei einzeln angelegten Schaltern die gewünschten Türen im Inspector unter `Connected Doors` zuweisen. Die erweiterte Startszene enthält bereits ein verbundenes Paar.
- Für einen Schlüssel ein Sammelobjekt auf Typ `Key` setzen und beim Schalter `Requires Key` aktivieren.
- **Workshop → Level prüfen** zeigt technische Grundvoraussetzungen und häufige fehlende Komponenten. Ein grüner Status ersetzt keinen Spieltest.
- Zum Testen WASD/Pfeiltasten verwenden. Nach dem Sieg setzt **R** die ganze Mission zurück. Das Betreuer-Panel kann während des Spiels ebenfalls zurücksetzen.

## Häufige Stolperstellen

Eine Tür darf den einzigen Weg zum Schlüssel nicht versperren. Ein Checkpoint sollte außerhalb einer Gefahrenzone liegen. Größere Sammelwerte erhöhen sowohl Fortschritt als auch benötigten Gesamtwert. Auch `Bonus`-Objekte zählen derzeit zum Gesamtziel. Prüfe nach Änderungen an Sprites zusätzlich die Collider-Größe.

Der Missionsassistent legt eine neue Mission in einer leeren Szene an. Enthält die Szene bereits einen MissionManager, verhindert er versehentliches doppeltes Anlegen. Für einen weiteren Entwurf zuerst eine neue Szene erstellen.
