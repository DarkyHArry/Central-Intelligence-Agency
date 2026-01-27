using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Runtime.InteropServices;
using CIA.HelpDesk.Models;
using CIA.HelpDesk.Services;
using CIA.HelpDesk.Storage;

var persistence = new PersistenceService();
var repo = new InMemoryChamadoRepository(persistence);
var service = new ChamadoService(repo);
var central = persistence.LoadCentral();

// Gera um código administrativo único simples para oficiais
var adminCode = GenerateAdminCode();

Console.WriteLine("CIA Deskopt Emergency");
Console.WriteLine("Você é: 1) Oficial  2) Civil");
var role = Console.ReadLine();

if (role == "2")
{
	Console.Write("Seu Nome completo: ");
	var nome = Console.ReadLine() ?? string.Empty;
	if (!IsValidName(nome)) { Console.WriteLine("Nome inválido. Use apenas letras e espaços."); return; }

	// Busca por nome existente (persistente)
	var clients = persistence.LoadClients();
	var cliente = clients.Find(c => string.Equals(c.Nome, nome, StringComparison.InvariantCultureIgnoreCase));

	if (cliente == null)
	{
		Console.Write("Documento (somente números - CPF 11 dígitos): ");
		var documento = Console.ReadLine() ?? string.Empty;
		if (!IsValidDocument(documento)) { Console.WriteLine("Documento inválido. Deve ser número com 11 dígitos."); return; }
		
		Console.Write("Sua Localização: : ");
		var endereco = Console.ReadLine() ?? string.Empty;
		
		cliente = new Cliente(clients.Count + 1, nome, "Civil", documento, endereco);
		clients.Add(cliente);
		persistence.SaveClients(clients);
		Console.WriteLine("Cadastro criado e salvo.");
	}
	else
	{
		// Atualiza endereço se for um cliente existente
		Console.Write("Endereço do incidente: ");
		var endereco = Console.ReadLine() ?? string.Empty;
		if (!string.IsNullOrEmpty(endereco))
		{
			cliente.Endereco = endereco;
			persistence.SaveClients(clients);
		}
	}

	Console.Write("Descreva o incidente: ");
	var descricao = Console.ReadLine() ?? string.Empty;
	var unidade = AnalyzeIncident(descricao);
	Console.WriteLine($"Detectado envio para: {unidade}");

	service.CriarChamado(descricao, cliente, Categoria.Segurança, cliente.Endereco);
	persistence.SaveChamados(service.ListarChamados());
	Console.WriteLine("Chamado criado e registrado.");

	// Tocar áudio (apenas para civil) — tenta arquivo específico por unidade
	PlayAlertForUnit(unidade);

	// salva central atual
	persistence.SaveCentral(central);
}
else if (role == "1")
{
	Console.WriteLine($"Código administrativo padrão (guarde): {adminCode}");
	Console.Write("Digite o código para entrar na área administrativa: ");
	var input = Console.ReadLine() ?? string.Empty;

	if (input != adminCode)
	{
		Console.WriteLine("Código inválido. Saindo.");
		return;
	}

	Console.WriteLine("Acesso administrativo concedido.");
	bool running = true;
	while (running)
	{
		Console.WriteLine("Admin Menu: 1) Cadastrar criminoso 2) Cadastrar operação 3) Cadastro apoio governamental 4) Listar tudo 0) Sair");
		var opt = Console.ReadLine();
		switch (opt)
		{
			case "1":
				Console.Write("Nome do criminoso: ");
				var nome = Console.ReadLine() ?? string.Empty;
				Console.Write("Descrição / observações: ");
				var desc = Console.ReadLine() ?? string.Empty;
				central.Criminosos.Add(new Criminoso { Nome = nome, Detalhes = desc, DataRegistro = DateTime.UtcNow, Origem = "Oficial" });
				persistence.SaveCentral(central);
				Console.WriteLine("Criminoso registrado.");
				break;
			case "2":
				Console.Write("Nome da operação: ");
				var op = Console.ReadLine() ?? string.Empty;
				Console.Write("Descrição: ");
				var dop = Console.ReadLine() ?? string.Empty;
				central.Operacoes.Add(new Operacao { Titulo = op, Descricao = dop, DataAgendada = DateTime.UtcNow, Origem = "Oficial" });
				persistence.SaveCentral(central);
				Console.WriteLine("Operação registrada.");
				break;
			case "3":
				Console.Write("Descrição do apoio: ");
				var sup = Console.ReadLine() ?? string.Empty;
				central.Supports.Add(new GovernmentSupport { Descricao = sup, Data = DateTime.UtcNow, Origem = "Oficial" });
				persistence.SaveCentral(central);
				Console.WriteLine("Apoio registrado.");
				break;
			case "4":
				Console.WriteLine("-- Criminosos --");
				foreach (var c in central.Criminosos) Console.WriteLine($"{c.Nome} - {c.Detalhes} - {c.DataRegistro}");
				Console.WriteLine("-- Operações --");
				foreach (var o in central.Operacoes) Console.WriteLine($"{o.Titulo} - {o.Descricao} - {o.DataAgendada}");
				Console.WriteLine("-- Apoios Governamentais --");
				foreach (var s in central.Supports) Console.WriteLine($"{s.Descricao} - {s.Data}");
				break;
			case "0":
				running = false; break;
			default:
				Console.WriteLine("Opção inválida."); break;
		}
	}
}
else
{
	Console.WriteLine("Opção inválida. Encerrando.");
}

string AnalyzeIncident(string texto)
{
	var policeKeywords = new[] { "homem", "mulher", "criminoso", "ladrão", "assalto", "agressor", "criança", 
								"roubo", "arma", "tiroteio", "invasão", "sequestro", "invasão, invadiram", 
								"estrupo", "vítima", "vítimas", "atirador", "sequestraram"};

	var ambulanceKeywords = new[] { "ferido", "sangue", "machucado", "acidente", "ferimentos", "morto", "mortes",
									"desmaio", "desmaiou", "caiu", "queda", "batida", "colisão", "contusão", 
									"fratura", "fraturou", "queimadura", "queimado", "infarto", "parada cardíaca",
									"passando mal", "mal súbito"};

	var fireKeywords = new[] { "fogo", "incêndio", "queimando", "explosão", "fumaça",
								"queimado", "chamas", "labaredas", "explodiu", "explodiram", "curto-circuito",
								"curto circuito", "curto", "faísca", "faíscas",
								"combustível", "inflamável", "inflamavel",
								"derramamento", "vazamento"};

	var candidates = new List<string>();
	var lower = texto.ToLowerInvariant();
	foreach (var k in policeKeywords) if (lower.Contains(k)) candidates.Add("Polícia");
	foreach (var k in ambulanceKeywords) if (lower.Contains(k)) candidates.Add("Ambulância");
	foreach (var k in fireKeywords) if (lower.Contains(k)) candidates.Add("Bombeiros");

	if (candidates.Count == 0) return "Unidade de Resposta Geral";
	var rnd = new Random();
	return candidates[rnd.Next(candidates.Count)];
}

bool IsValidName(string nome)
{
	if (string.IsNullOrWhiteSpace(nome)) return false;
	// aceita letras (inclui acentos como ~, ´, ^, etc), apóstrofos e espaços
	return Regex.IsMatch(nome, @"^[\p{L}\p{M}\s''-]+$", RegexOptions.IgnoreCase);
}

bool IsValidDocument(string doc)
{
	if (string.IsNullOrWhiteSpace(doc)) return false;
	if (!Regex.IsMatch(doc, "^\\d+$")) return false;
	return doc.Length == 11; // exige 11 dígitos para CPF
}

void PlayAlertForUnit(string unidade)
{
	try
	{
		var audiosDir = Path.Combine(Directory.GetCurrentDirectory(), "audios");
		if (!Directory.Exists(audiosDir)) return;

		var lower = (unidade ?? string.Empty).ToLowerInvariant();
		string filename = null;

		if (lower.Contains("ambul")) filename = "fire rescue.mp3";
		else if (lower.Contains("bombeir")) filename = "fire rescue.mp3";
		else if (lower.Contains("pol")) filename = "police_siren.mp3";

		// se não mapeado, pega qualquer mp3 disponível
		if (string.IsNullOrEmpty(filename))
		{
			var any = Directory.GetFiles(audiosDir, "*.mp3");
			if (any.Length > 0) filename = Path.GetFileName(any[0]);
		}

		if (string.IsNullOrEmpty(filename)) return;

		var path = Path.Combine(audiosDir, filename);
		if (!File.Exists(path))
		{
			Console.WriteLine($"Áudio esperado não encontrado: {path}");
			return;
		}

		Console.WriteLine($"CIA Radio: {filename}");
		
		// Detecta o sistema operacional e usa o player apropriado
		ProcessStartInfo psi;
		if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux))
		{
			// Para Linux, tenta usar players disponíveis: paplay, aplay, ffplay, mpg123
			string[] players = { "paplay", "aplay", "ffplay", "mpg123" };
			string player = null;
			
			foreach (var p in players)
			{
				var checkPsi = new ProcessStartInfo("which", p) { RedirectStandardOutput = true, UseShellExecute = false };
				using (var proc = Process.Start(checkPsi))
				{
					proc.WaitForExit();
					if (proc.ExitCode == 0)
					{
						player = p;
						break;
					}
				}
			}
			
			if (string.IsNullOrEmpty(player))
			{
				Console.WriteLine("Nenhum player de áudio disponível no Linux. Instale: pulseaudio-utils (paplay) ou alsa-utils (aplay)");
				return;
			}
			
			psi = new ProcessStartInfo(player, $"\"{path}\"") { UseShellExecute = false };
		}
		else if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
		{
			// Para Windows, usa o player padrão
			psi = new ProcessStartInfo(path) { UseShellExecute = true };
		}
		else if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.OSX))
		{
			// Para macOS, usa afplay
			psi = new ProcessStartInfo("afplay", path) { UseShellExecute = false };
		}
		else
		{
			Console.WriteLine("Sistema operacional não suportado para reprodução de áudio.");
			return;
		}
		
		var audioProcess = Process.Start(psi);
		if (audioProcess != null)
		{
			// Aguarda o áudio terminar antes de continuar
			audioProcess.WaitForExit();
		}
	}
	catch (Exception ex)
	{
		Console.WriteLine($"Falha ao tocar áudio: {ex.Message}");
	}
}

string GenerateAdminCode()
{
	return Guid.NewGuid().ToString("N").Substring(0, 8).ToUpperInvariant();
}
