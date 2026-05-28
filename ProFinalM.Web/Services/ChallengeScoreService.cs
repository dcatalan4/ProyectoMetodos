using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using ProFinalM.Web.Models;

namespace ProFinalM.Web.Services;

public class ChallengeScoreService
{
    private static readonly string[] BannedWords =
    [
        "cerote", "mierda", "puta", "puto", "pendejo", "pendeja", "idiota", "imbecil",
        "maldito", "maldita", "culero", "culera", "verga", "hueco", "joder", "fuck",
        "shit", "bitch", "asshole"
    ];

    private readonly string _filePath;
    private readonly object _lock = new();

    public ChallengeScoreService(IWebHostEnvironment environment)
    {
        var dataPath = Path.Combine(environment.ContentRootPath, "App_Data");
        Directory.CreateDirectory(dataPath);
        _filePath = Path.Combine(dataPath, "scores.txt");
    }

    public IReadOnlyList<ChallengeScore> GetTopScores(int take = 10)
    {
        if (!File.Exists(_filePath))
        {
            return [];
        }

        lock (_lock)
        {
            return File.ReadAllLines(_filePath)
                .Select(Parse)
                .Where(score => score is not null)
                .Select(score => score!)
                .OrderByDescending(score => score.Score)
                .ThenBy(score => score.CreatedAt)
                .Take(take)
                .ToList();
        }
    }

    public ChallengeScore Save(SaveScoreRequest request)
    {
        var userName = NormalizeUserName(request.UserName);
        if (!IsAllowedName(userName))
        {
            throw new InvalidOperationException("Usa un nombre respetuoso para guardar tu punteo.");
        }

        if (request.Total <= 0 || request.Score < 0 || request.Score > request.Total)
        {
            throw new InvalidOperationException("El punteo recibido no es válido.");
        }

        var score = new ChallengeScore
        {
            UserName = userName,
            Score = request.Score,
            Total = request.Total,
            Challenge = NormalizeField(request.Challenge, 40),
            CreatedAt = DateTime.UtcNow
        };

        var line = string.Join('\t',
            score.CreatedAt.ToString("O", CultureInfo.InvariantCulture),
            Escape(score.UserName),
            score.Score.ToString(CultureInfo.InvariantCulture),
            score.Total.ToString(CultureInfo.InvariantCulture),
            Escape(score.Challenge));

        lock (_lock)
        {
            File.AppendAllText(_filePath, line + Environment.NewLine, Encoding.UTF8);
        }

        return score;
    }

    public static bool IsAllowedName(string userName)
    {
        if (string.IsNullOrWhiteSpace(userName))
        {
            return false;
        }

        var normalized = RemoveDiacritics(userName).ToLowerInvariant();
        normalized = Regex.Replace(normalized, @"[^a-z0-9]+", "");
        return !BannedWords.Any(word => normalized.Contains(word));
    }

    private static ChallengeScore? Parse(string line)
    {
        var parts = line.Split('\t');
        if (parts.Length < 5 ||
            !DateTime.TryParse(parts[0], null, DateTimeStyles.RoundtripKind, out var createdAt) ||
            !int.TryParse(parts[2], out var score) ||
            !int.TryParse(parts[3], out var total))
        {
            return null;
        }

        return new ChallengeScore
        {
            CreatedAt = createdAt,
            UserName = Unescape(parts[1]),
            Score = score,
            Total = total,
            Challenge = Unescape(parts[4])
        };
    }

    private static string NormalizeUserName(string value)
    {
        value = NormalizeField(value, 24);
        if (!Regex.IsMatch(value, @"^[A-Za-z0-9ÁÉÍÓÚÜÑáéíóúüñ _.-]+$"))
        {
            throw new InvalidOperationException("El nombre solo puede usar letras, numeros, espacios, punto, guion y guion bajo.");
        }

        return value;
    }

    private static string NormalizeField(string value, int maxLength)
    {
        value = Regex.Replace(value.Trim(), @"\s+", " ");
        if (value.Length > maxLength)
        {
            value = value[..maxLength];
        }

        return value;
    }

    private static string Escape(string value) => value.Replace("\\", "\\\\").Replace("\t", " ").Replace("\r", " ").Replace("\n", " ");
    private static string Unescape(string value) => value.Replace("\\\\", "\\");

    private static string RemoveDiacritics(string value)
    {
        var normalized = value.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder();
        foreach (var character in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(character);
            }
        }

        return builder.ToString().Normalize(NormalizationForm.FormC);
    }
}
