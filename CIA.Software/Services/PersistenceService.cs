using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using CIA.HelpDesk.Models;
using CIA.HelpDesk.Storage;

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

        public List<Storage.Criminoso> LoadCriminosos()
        {
            var p = PathFor("criminosos");
            if (!File.Exists(p)) return new List<Storage.Criminoso>();
            var json = File.ReadAllText(p);
            return JsonSerializer.Deserialize<List<Storage.Criminoso>>(json, _options) ?? new List<Storage.Criminoso>();
        }

        public void SaveCriminosos(IEnumerable<Storage.Criminoso> criminosos)
        {
            var p = PathFor("criminosos");
            var json = JsonSerializer.Serialize(criminosos, _options);
            File.WriteAllText(p, json);
        }

        public List<Storage.Operacao> LoadOperacoes()
        {
            var p = PathFor("operacoes");
            if (!File.Exists(p)) return new List<Storage.Operacao>();
            var json = File.ReadAllText(p);
            return JsonSerializer.Deserialize<List<Storage.Operacao>>(json, _options) ?? new List<Storage.Operacao>();
        }

        public void SaveOperacoes(IEnumerable<Storage.Operacao> operacoes)
        {
            var p = PathFor("operacoes");
            var json = JsonSerializer.Serialize(operacoes, _options);
            File.WriteAllText(p, json);
        }

        public List<Storage.GovernmentSupport> LoadSupports()
        {
            var p = PathFor("supports");
            if (!File.Exists(p)) return new List<Storage.GovernmentSupport>();
            var json = File.ReadAllText(p);
            return JsonSerializer.Deserialize<List<Storage.GovernmentSupport>>(json, _options) ?? new List<Storage.GovernmentSupport>();
        }

        public void SaveSupports(IEnumerable<Storage.GovernmentSupport> supports)
        {
            var p = PathFor("supports");
            var json = JsonSerializer.Serialize(supports, _options);
            File.WriteAllText(p, json);
        }

        [Obsolete("Use LoadCriminosos, LoadOperacoes, and LoadSupports instead")]
        public void LoadCentralLegacy()
        {
            // Este método é mantido apenas para compatibilidade com código legado
            // Migre para LoadCriminosos(), LoadOperacoes() e LoadSupports()
        }

        [Obsolete("Use SaveCriminosos, SaveOperacoes, and SaveSupports instead")]
        public void SaveCentralLegacy()
        {
            // Este método é mantido apenas para compatibilidade com código legado
            // Migre para SaveCriminosos(), SaveOperacoes() e SaveSupports()
        }
    }
}
