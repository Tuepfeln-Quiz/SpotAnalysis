# SpotAnalysis

## Projektstruktur
- `SpotAnalysisV2.Web` – aktuelle Blazor-UI (Pages, Shared Components, Layout, `wwwroot/css`)
- `SpotAnalysis.Web` – bestehende/ältere Web-Oberfläche
- `SpotAnalysis.Services` – Business-Logik und Anwendungsservices
- `SpotAnalysis.Data` – Datenmodelle und Persistenz
- `ExcelImportExport` – Import/Export-Funktionalität
- `*.Tests` – Testprojekte

## Coding-Regeln (kurz)
1. **Single Responsibility**: Komponenten und Services klein halten.
2. **Wiederverwendung vor Duplikat**: Wiederkehrende UI in `Components/Shared` auslagern.
3. **Schichten sauber trennen**:
   - UI in `*.Web`
   - Fachlogik in `SpotAnalysis.Services`
   - Datenzugriff in `SpotAnalysis.Data`
4. **Benennung klar halten**: sprechende Namen, konsistente Suffixe (`Dto`, `Service`, `List`, `Editor`, `Player`).
5. **Styling strukturieren**:
   - globale Styles: `wwwroot/css`
   - pages: `wwwroot/css/pages`
   - shared components: `wwwroot/css/components`
6. **Änderungen absichern**: nach Refactoring immer bauen und relevante Tests ausführen.

## Onboarding-Hinweis
Für neue Features zuerst prüfen, ob bereits ein Shared Component vorhanden ist (`TeacherPageHeader`, `ConfirmDialog`, `AppButton`, `TeacherFeatureCard`, `ExperimentTabs`).
