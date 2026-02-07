using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using CIA.HelpDesk.Models;

namespace CIA.HelpDesk.Services
{
    public class PersistenceService
    {
        private readonly string _dataDir;
        private readonly JsonSerializerOptions _options = new JsonSerializerOptions { WriteIndented = true };

        public PersistenceService(string dataDir = "data")
        {
            _dataDir = dataDir;
            if (!Directory.Exists(_dataDir)) Directory.CreateDirectory(_dataDir);
        }

        private string PathFor(string name) => System.IO.Path.Combine(_dataDir, name + ".json");

        public List<Cliente> LoadClients()
        {
            var p = PathFor("clients");
            if (!File.Exists(p)) return new List<Cliente>();
            var json = File.ReadAllText(p);
            return JsonSerializer.Deserialize<List<Cliente>>(json, _options) ?? new List<Cliente>();
        }

        public void SaveClients(IEnumerable<Cliente> clients)
        {
            var p = PathFor("clients");
            var json = JsonSerializer.Serialize(clients, _options);
            File.WriteAllText(p, json);
        }

        public List<Models.Chamado> LoadChamados()
        {
            var p = PathFor("chamados");
            if (!File.Exists(p)) return new List<Models.Chamado>();
            var json = File.ReadAllText(p);
            return JsonSerializer.Deserialize<List<Models.Chamado>>(json, _options) ?? new List<Models.Chamado>();
        }

        public void SaveChamados(IEnumerable<Models.Chamado> chamados)
        {
            var p = PathFor("chamados");
            var json = JsonSerializer.Serialize(chamados, _options);
            File.WriteAllText(p, json);
        }
    }
}
