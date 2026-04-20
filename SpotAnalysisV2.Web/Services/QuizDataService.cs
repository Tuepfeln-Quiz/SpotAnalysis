using System.Text.Json;
using SpotAnalysisV2.Web.Models;

namespace SpotAnalysisV2.Web.Services;

public class QuizDataService
{
    private readonly QuizData _data;
    private readonly string _uebungenPath;
    private readonly object _sync = new();
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };
    private List<UebungItem>? _cachedUebungen;

    public QuizDataService(IWebHostEnvironment env)
    {
        var path = Path.Combine(env.ContentRootPath, "Data", "quizdata.json");
        _uebungenPath = Path.Combine(env.ContentRootPath, "Data", "uebungen.json");
        var json = File.ReadAllText(path);
        _data = JsonSerializer.Deserialize<QuizData>(json, _jsonOptions) ?? new();
    }

    public List<LightQuizData> GetLightQuizzes() =>
        _data.Light.OrderBy(q => q.Id).ToList();

    public List<TuepfelnQuizData> GetTuepfelnQuizzes() =>
        _data.Tuepfeln.OrderBy(q => q.Id).ToList();

    public TuepfelnQuizData? GetTuepfelnQuiz(int id) =>
        _data.Tuepfeln.FirstOrDefault(q => q.Id == id);

    public LightQuizData? GetLightQuiz(int id) =>
        _data.Light.FirstOrDefault(q => q.Id == id);

    public AufgabeInfo? GetAufgabe(int uebungId, int aufgabeId) =>
        GetUebungen().FirstOrDefault(u => u.Id == uebungId)
                     ?.Aufgaben.FirstOrDefault(a => a.AufgabeId == aufgabeId);

    public int GetAufgabenCount(int uebungId) =>
        GetUebungen().FirstOrDefault(u => u.Id == uebungId)?.AufgabenCount ?? 1;

    public List<UebungItem> GetUebungen()
    {
        lock (_sync)
        {
            _cachedUebungen ??= LoadUebungen();
            return _cachedUebungen.Select(CloneUebung).ToList();
        }
    }

    public UebungItem CreateUebung(string title, string? description, IEnumerable<AufgabeInfo> aufgaben)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);

        var normalizedAufgaben = aufgaben
            .Select((aufgabe, index) => NormalizeAufgabe(aufgabe, index + 1))
            .ToList();

        if (normalizedAufgaben.Count == 0)
            throw new InvalidOperationException("Bitte mindestens ein Quiz auswählen.");

        lock (_sync)
        {
            _cachedUebungen ??= LoadUebungen();

            var newUebung = new UebungItem
            {
                Id = _cachedUebungen.Count == 0 ? 1 : _cachedUebungen.Max(u => u.Id) + 1,
                Title = title.Trim(),
                Description = string.IsNullOrWhiteSpace(description)
                    ? BuildDescription(normalizedAufgaben)
                    : description.Trim(),
                Aufgaben = normalizedAufgaben
            };

            _cachedUebungen.Add(newUebung);
            SaveUebungen(_cachedUebungen);
            return CloneUebung(newUebung);
        }
    }

    public bool DeleteUebung(int uebungId)
    {
        lock (_sync)
        {
            _cachedUebungen ??= LoadUebungen();
            var uebung = _cachedUebungen.FirstOrDefault(u => u.Id == uebungId);
            if (uebung == null) return false;
            _cachedUebungen.Remove(uebung);
            SaveUebungen(_cachedUebungen);
            return true;
        }
    }

    private List<UebungItem> LoadUebungen()
    {
        if (!File.Exists(_uebungenPath))
            return new();

        var json = File.ReadAllText(_uebungenPath);
        var uebungen = JsonSerializer.Deserialize<List<UebungItem>>(json, _jsonOptions) ?? new();

        return uebungen
            .OrderBy(u => u.Id)
            .Select(NormalizeUebung)
            .ToList();
    }

    private void SaveUebungen(IEnumerable<UebungItem> uebungen)
    {
        var normalized = uebungen
            .OrderBy(u => u.Id)
            .Select(NormalizeUebung)
            .ToList();

        Directory.CreateDirectory(Path.GetDirectoryName(_uebungenPath)!);
        var json = JsonSerializer.Serialize(normalized, _jsonOptions);
        File.WriteAllText(_uebungenPath, json);
        _cachedUebungen = normalized;
    }

    private UebungItem NormalizeUebung(UebungItem uebung)
    {
        return new UebungItem
        {
            Id = uebung.Id,
            Title = uebung.Title?.Trim() ?? string.Empty,
            Description = uebung.Description?.Trim() ?? string.Empty,
            Aufgaben = uebung.Aufgaben
                .Select((aufgabe, index) => NormalizeAufgabe(aufgabe, index + 1))
                .ToList()
        };
    }

    private AufgabeInfo NormalizeAufgabe(AufgabeInfo aufgabe, int aufgabeId)
    {
        var exists = aufgabe.Typ == AufgabeTyp.Light
            ? GetLightQuiz(aufgabe.QuizId) != null
            : GetTuepfelnQuiz(aufgabe.QuizId) != null;

        if (!exists)
            throw new InvalidOperationException($"Quiz {aufgabe.QuizId} vom Typ {aufgabe.Typ} wurde nicht gefunden.");

        return new AufgabeInfo
        {
            AufgabeId = aufgabeId,
            Typ = aufgabe.Typ,
            QuizId = aufgabe.QuizId
        };
    }

    private string BuildDescription(IEnumerable<AufgabeInfo> aufgaben)
    {
        var titles = aufgaben
            .Take(2)
            .Select(GetQuizTitle)
            .Where(title => !string.IsNullOrWhiteSpace(title))
            .ToList();

        return titles.Count == 0 ? "Individuell erstellt" : string.Join(" • ", titles);
    }

    private string GetQuizTitle(AufgabeInfo aufgabe) =>
        aufgabe.Typ == AufgabeTyp.Light
            ? GetLightQuiz(aufgabe.QuizId)?.Title ?? $"Light {aufgabe.QuizId}"
            : GetTuepfelnQuiz(aufgabe.QuizId)?.Title ?? $"Tüpfeln {aufgabe.QuizId}";

    private static UebungItem CloneUebung(UebungItem uebung)
    {
        return new UebungItem
        {
            Id = uebung.Id,
            Title = uebung.Title,
            Description = uebung.Description,
            Aufgaben = uebung.Aufgaben
                .Select(a => new AufgabeInfo
                {
                    AufgabeId = a.AufgabeId,
                    Typ = a.Typ,
                    QuizId = a.QuizId
                })
                .ToList()
        };
    }
}
