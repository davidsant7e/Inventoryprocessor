using System.Text.Json;
using System.Text.RegularExpressions;
using InventoryProcessor.Models;

namespace InventoryProcessor.Services;

public class FileService : IFileService
{
    private static readonly JsonSerializerOptions _readOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private static readonly JsonSerializerOptions _writeOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public InventoryFile LoadFromFile(string path)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException($"Archivo no encontrado: {path}");

        string json = File.ReadAllText(path);
        json = Regex.Replace(json, @"(?<=[:,\[]\s*)0+(\d)", "$1");

        try
        {
            var result = JsonSerializer.Deserialize<InventoryFile>(json, _readOptions);
            if (result is null)
                throw new InvalidDataException("El archivo JSON no pudo deserializarse correctamente.");
            return result;
        }
        catch (JsonException ex)
        {
            throw new InvalidDataException($"JSON inválido: {ex.Message}", ex);
        }
    }

    public void SaveToFile(InventoryFile inventory, string path)
    {
        string json = JsonSerializer.Serialize(inventory, _writeOptions);
        File.WriteAllText(path, json);
    }
}
