using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace PL.Services
{
    public class FileService
    {
        public IEnumerable<string> GetFileSystemEntries(string path)
        {
            var entries = new List<string>();

            try
            {
                // Listar directorios
                var directories = Directory.GetDirectories(path);
                entries.AddRange(directories);

                // Listar archivos
                var files = Directory.GetFiles(path);
                entries.AddRange(files);
            }
            catch (UnauthorizedAccessException ex)
            {
                entries.Add($"Acceso denegado: {ex.Message}");
            }
            catch (Exception ex)
            {
                entries.Add($"Error: {ex.Message}");
            }

            return entries;
        }
    }
}