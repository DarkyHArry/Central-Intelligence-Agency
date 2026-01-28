using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Runtime.InteropServices;
using CIA.HelpDesk.Models;
using CIA.HelpDesk.Services;
using CIA.HelpDesk.Storage;
using CIA.HelpDesk.Utils;

var persistence = new PersistenceService();
var repo = new InMemoryChamadoRepository(persistence);
var service = new ChamadoService(repo);
var criminosos = persistence.LoadCriminosos();
var operacoes = persistence.LoadOperacoes();
var supports = persistence.LoadSupports();

// Gera um código administrativo único simples para oficiais
var adminCode = GenerateAdminCode();

ColorConsole.PrintMenu("═══════════════════════════════════════════════════════════════");
ColorConsole.PrintMenu("                   CIA DESKOPT EMERGENCY                       ");
ColorConsole.PrintMenu("═══════════════════════════════════════════════════════════════");
Console.WriteLine();
ColorConsole.WriteMenu("Você é: ");
ColorConsole.WriteCorporation("1) Oficial");
Console.Write("  ");
ColorConsole.WriteCivil("2) Civil");
Console.WriteLine();
var role = Console.ReadLine();

if (role == "2")
{
	ColorConsole.PrintCivil("╔════════════════════════════════════════════════════════════╗");
	ColorConsole.PrintCivil("║              SISTEMA DE ATENDIMENTO AO CIVIL               ║");
	ColorConsole.PrintCivil("╚════════════════════════════════════════════════════════════╝");
	Console.WriteLine();
	ColorConsole.WriteCivil("Seu Nome completo: ");
	var nome = Console.ReadLine() ?? string.Empty;
	if (!IsValidName(nome)) { ColorConsole.PrintError("Nome inválido. Use apenas letras e espaços."); return; }

	// Busca por nome existente (persistente)
	var clients = persistence.LoadClients();
	var cliente = clients.Find(c => string.Equals(c.Nome, nome, StringComparison.InvariantCultureIgnoreCase));

	if (cliente == null)
	{
		ColorConsole.WriteCivil("Documento (somente números - CPF 11 dígitos): ");
		var documento = Console.ReadLine() ?? string.Empty;
		if (!IsValidDocument(documento)) { ColorConsole.PrintError("Documento inválido. Deve ser número com 11 dígitos."); return; }
		
		ColorConsole.WriteCivil("Sua Localização: ");
		var endereco = Console.ReadLine() ?? string.Empty;
		
		cliente = new Cliente(clients.Count + 1, nome, "Civil", documento, endereco);
		clients.Add(cliente);
		persistence.SaveClients(clients);
		ColorConsole.PrintSuccess("✓ Cadastro criado e salvo.");
	}
	else
	{
		// Atualiza endereço se for um cliente existente
		ColorConsole.WriteCivil("Endereço do incidente: ");
		var endereco = Console.ReadLine() ?? string.Empty;
		if (!string.IsNullOrEmpty(endereco))
		{
			cliente.Endereco = endereco;
			persistence.SaveClients(clients);
		}
	}

	ColorConsole.WriteCivil("Descreva o incidente: ");
	var descricao = Console.ReadLine() ?? string.Empty;
	var unidade = AnalyzeIncident(descricao);
	ColorConsole.PrintInfo($"➜ Detectado envio para: {unidade}");

	service.CriarChamado(descricao, cliente, Categoria.Segurança, cliente.Endereco);
	persistence.SaveChamados(service.ListarChamados());
	ColorConsole.PrintSuccess("✓ Chamado criado e registrado.");

	// Tocar áudio (apenas para civil) — tenta arquivo específico por unidade
	PlayAlertForUnit(unidade);

	// salva cliente atual
	persistence.SaveClients(service.ListarChamados().Select(c => c.Solicitante).Distinct().ToList());
}
else if (role == "1")
{
	ColorConsole.PrintCorporation("╔════════════════════════════════════════════════════════════╗");
	ColorConsole.PrintCorporation("║              ÁREA ADMINISTRATIVA RESTRITA                 ║");
	ColorConsole.PrintCorporation("╚════════════════════════════════════════════════════════════╝");
	Console.WriteLine();
	ColorConsole.WriteInfo($"Código administrativo padrão (guarde): ");
	ColorConsole.PrintInfo(adminCode);
	ColorConsole.WriteCorporation("Digite o código para entrar na área administrativa: ");
	var input = Console.ReadLine() ?? string.Empty;

	if (input != adminCode)
	{
		ColorConsole.PrintError("✗ Código inválido. Saindo.");
		return;
	}

	ColorConsole.PrintSuccess("✓ Acesso administrativo concedido.");
	bool running = true;
	while (running)
	{
		Console.WriteLine();
		ColorConsole.PrintMenu("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
		ColorConsole.PrintMenu("MENU ADMINISTRATIVO");
		ColorConsole.PrintMenu("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
		Console.Write("  ");
		ColorConsole.WriteCorporation("1) Cadastrar criminoso");
		Console.Write("      ");
		ColorConsole.WriteCorporation("2) Cadastrar operação");
		Console.WriteLine();
		Console.Write("  ");
		ColorConsole.WriteCorporation("3) Cadastro apoio governamental");
		Console.Write("  ");
		ColorConsole.WriteMenu("4) Listar tudo");
		Console.Write("  ");
		ColorConsole.WriteMenu("0) Sair");
		Console.WriteLine();
		ColorConsole.PrintMenu("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
		ColorConsole.WriteMenu("Escolha: ");
		var opt = Console.ReadLine();
		switch (opt)
		{
			case "1":
				Console.WriteLine();
				ColorConsole.PrintCorporation("═ CADASTRO DE CRIMINOSO ═");
				ColorConsole.WriteCorporation("Nome do criminoso: ");
				var nome = Console.ReadLine() ?? string.Empty;
				ColorConsole.WriteCorporation("Descrição / observações: ");
				var desc = Console.ReadLine() ?? string.Empty;
				criminosos.Add(new Criminoso { Nome = nome, Detalhes = desc, DataRegistro = DateTime.UtcNow, Origem = "Oficial" });
				persistence.SaveCriminosos(criminosos);
				ColorConsole.PrintSuccess("✓ Criminoso registrado.");
				break;
			case "2":
				Console.WriteLine();
				ColorConsole.PrintCorporation("═ CADASTRO DE OPERAÇÃO ═");
				ColorConsole.WriteCorporation("Nome da operação: ");
				var op = Console.ReadLine() ?? string.Empty;
				ColorConsole.WriteCorporation("Descrição: ");
				var dop = Console.ReadLine() ?? string.Empty;
				operacoes.Add(new Operacao { Titulo = op, Descricao = dop, DataAgendada = DateTime.UtcNow, Origem = "Oficial" });
				persistence.SaveOperacoes(operacoes);
				ColorConsole.PrintSuccess("✓ Operação registrada.");
				break;
			case "3":
				Console.WriteLine();
				ColorConsole.PrintCorporation("═ CADASTRO DE APOIO GOVERNAMENTAL ═");
				ColorConsole.WriteCorporation("Descrição do apoio: ");
				var sup = Console.ReadLine() ?? string.Empty;
				supports.Add(new GovernmentSupport { Descricao = sup, Data = DateTime.UtcNow, Origem = "Oficial" });
				persistence.SaveSupports(supports);
				ColorConsole.PrintSuccess("✓ Apoio registrado.");
				break;
			case "4":
				Console.WriteLine();
				ColorConsole.PrintMenu("═══════════════════════════════════════════════════════════");
				ColorConsole.PrintCorporation("■ CRIMINOSOS CADASTRADOS");
				ColorConsole.PrintMenu("═══════════════════════════════════════════════════════════");
				if (criminosos.Count == 0)
					ColorConsole.PrintInfo("  (Nenhum criminoso registrado)");
				else
					foreach (var c in criminosos) 
						ColorConsole.PrintCorporation($"  ► {c.Nome} - {c.Detalhes} ({c.DataRegistro:dd/MM/yyyy HH:mm})");
				
				Console.WriteLine();
				ColorConsole.PrintMenu("═══════════════════════════════════════════════════════════");
				ColorConsole.PrintCorporation("■ OPERAÇÕES CADASTRADAS");
				ColorConsole.PrintMenu("═══════════════════════════════════════════════════════════");
				if (operacoes.Count == 0)
					ColorConsole.PrintInfo("  (Nenhuma operação registrada)");
				else
					foreach (var o in operacoes) 
						ColorConsole.PrintCorporation($"  ► {o.Titulo} - {o.Descricao} ({o.DataAgendada:dd/MM/yyyy HH:mm})");
				
				Console.WriteLine();
				ColorConsole.PrintMenu("═══════════════════════════════════════════════════════════");
				ColorConsole.PrintCorporation("■ APOIOS GOVERNAMENTAIS");
				ColorConsole.PrintMenu("═══════════════════════════════════════════════════════════");
				if (supports.Count == 0)
					ColorConsole.PrintInfo("  (Nenhum apoio registrado)");
				else
					foreach (var s in supports) 
						ColorConsole.PrintCorporation($"  ► {s.Descricao} ({s.Data:dd/MM/yyyy HH:mm})");
				break;
			case "0":
				running = false; 
				ColorConsole.PrintSuccess("✓ Encerrando sessão...");
				break;
			default:
				ColorConsole.PrintError("✗ Opção inválida."); 
				break;
		}
	}
}
else
{
	ColorConsole.PrintError("✗ Opção inválida. Encerrando.");
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

		if (lower.Contains("ambul")) filename = "Ambulance.mp3";
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

		Console.WriteLine("\t\nEstamos enviando a nossa unidade mais próxima.");
		
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
