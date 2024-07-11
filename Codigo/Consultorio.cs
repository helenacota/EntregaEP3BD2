using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading;
using Microsoft.VisualBasic;
using Npgsql;

namespace MisConsultas
{
    class Consultorio
    {
        private DbConnection db = new DbConnection();

        public void IniciarConsultorio()
        {
            bool retorno = true;

            // Lê o texto digitado pelo usuário
            while (retorno)
            {
                this.imprimirMenu();
                string input = Console.ReadLine();

                retorno = processaComando(input);
                
            }

        }

        private void imprimirMenu()
        {
            Console.WriteLine("\n------ MENU ------\n");
            Console.WriteLine("[0] Exibir consultas marcadas");
            Console.WriteLine("[1] Marcar consulta");
            Console.WriteLine("[2] Alterar consulta");
            Console.WriteLine("[3] Exibir médicos");
            Console.WriteLine("[4] Exibir agenda de um médico");
            Console.WriteLine("[5] Listar funcionalidades");
            Console.WriteLine("[9] Sair\n");
            Console.Write("Digite o número do comando: ");
        }

        private bool processaComando(string input)
        {
            switch (input)
            {
                case "0": //imprimir consultas
                    imprimirConsultas();
                    return true;
                case "1": // marcar consulta
                    marcaConsulta();
                    return true;
                case "2": // alterar consulta
                    alterarConsulta();
                    return true;
                case "3": // imprimir medicos 
                    imprimirMedicos();
                    return true;
                case "4": // imprimir uma agenda
                    imprimirAgenda();
                    return true;
                case "5": // listar funcionalidades
                    listarFunc();
                    return true;
                case "9": // exit
                    Console.WriteLine("\n------ Fechando consultório... ------ \n");
                    return false;
                default:
                    Console.WriteLine("\n------ Comando inválido! Tente novamente ------ \n");
                    return true;
            }
        }

        private void listarFunc()
        {
            Console.WriteLine("\n------ Marcação de Consultas ------ ");
            Console.WriteLine("- Cadastro e agendamento de consultas.");
            Console.WriteLine("- Verificação e seleção de horários disponíveis.");
            Console.WriteLine("- Atribuição de consultas aos médicos.");
            Console.WriteLine("- Cadastro rápido de novos pacientes.");
            Console.WriteLine("\n------ Triggers ------");
            Console.WriteLine("- Verificação se o médico tem a especialidade necessária antes de inserir uma consulta.");
            Console.WriteLine("- Verificação da disponibilidade do médico na agenda antes de inserir uma consulta.");
            Console.WriteLine("\n------ Emissão de Folhas de Pagamento ------ ");
            Console.WriteLine("- Cálculo e geração de folhas de pagamento para os médicos.");
            Console.WriteLine("\n------ Histórico Clínico dos Pacientes ------ ");
            Console.WriteLine("- Registro e atualização de dados clínicos e diagnósticos.");
            Console.WriteLine("- Manutenção do histórico de doenças e tratamentos.");
            Console.WriteLine("\n------ Cadastro e Gerenciamento de Médicos ------ ");
            Console.WriteLine("- Registro de médicos e suas especialidades.");
            Console.WriteLine("- Definição de horários de atendimento.");
            Console.WriteLine("\n------ Consulta de Cronograma de Atendimentos------ ");
            Console.WriteLine("- Exibição do cronograma de atendimento diário para cada médico.");
            Console.WriteLine("- Detalhamento das consultas com informações sobre pacientes e horários.");


        }

        private void alterarConsulta()
        {
            // Passo 1 - Exibir consultas existentes
            string sSqlConsultas = "select a.idcon, c.nomep, b.nomem, a.data, a.diasemana,a.horainiccon ";
            sSqlConsultas += " from clinicamedica.consulta a ";
            sSqlConsultas += " inner join clinicamedica.medico b on a.crm = b.crm  ";
            sSqlConsultas += " inner join clinicamedica.paciente c on a.idpac = c.idpac ";
            var readerConsultas = db.getReader(sSqlConsultas);
            List<Consulta> consultas = new List<Consulta>();
            int index = 0;

            Console.WriteLine("\n------ Consultas existentes: ------ \n");
            
            Console.Write("     | Paciente | Médico | Data | Dia da Semana | Horário | \n");
            while (readerConsultas.Read())
            {
                Consulta consulta = this.preencherConsultaMenu(readerConsultas);
                consultas.Add(consulta);

                Console.Write("\n[" + index + "] ");
                //Console.Write(consulta.IdCon + " | ");
                Console.Write(consulta.NomeP + " | ");
                Console.Write(consulta.NomeM + " | ");
                Console.Write(consulta.Data.ToString("dd/MM/yyyy") + " | ");
                Console.Write(consulta.DiaSemana + " | ");
                Console.Write(consulta.HoraInicCon + "\n");
                index++;
            }
            readerConsultas.Close();

            if (consultas.Count == 0)
            {
                Console.WriteLine("\n------ Nenhuma consulta encontrada para alterar ------ \n");
                return;
            }

            // Passo 2 - Pegar o número da consulta que o usuário deseja alterar
            Console.Write("\nDigite o índice da consulta que deseja alterar: ");
            int Indice = parseInt(Console.ReadLine());
            if (Indice < 0 || Indice >= consultas.Count)
            {
                Console.WriteLine("\nÍndice inválido.\n");
                return;
            }
            int IdCon = consultas[Indice].IdCon;

            // Objeto para armazenar as alterações da consulta
            Consulta consultaAlterada = new Consulta();

            // Passo 1 - Pegar especialidades disponíveis
            string sSqlEspecialidades = "select idesp, nomee from clinicamedica.especialidade";
            var readerEspecialidades = db.getReader(sSqlEspecialidades);
            List<(int IdEsp, string NomeE)> especialidades = new List<(int, string)>();
            index = 0;

            Console.WriteLine("\n------ Especialidades disponíveis: ------ \n");
            while (readerEspecialidades.Read())
            {
                especialidades.Add((readerEspecialidades.GetInt32(0), readerEspecialidades.GetString(1)));
                Console.WriteLine("[" + index + "] " + readerEspecialidades.GetString(1));
                index++;
            }
            readerEspecialidades.Close();

            Console.Write("\nDigite o número da especialidade desejada: ");
            int selectedEspIndex = Convert.ToInt32(parseInt(Console.ReadLine()));
            if (selectedEspIndex < 0 || selectedEspIndex >= especialidades.Count)
            {
                Console.WriteLine("\nÍndice inválido.\n");
                return;
            }
            int selectedIdEsp = especialidades[selectedEspIndex].IdEsp;

            // Passo 2 - Buscar médicos com a especialidade selecionada
            string sSqlMedicosEspecialidade = $"select m.crm, m.nomem from clinicamedica.exerceesp e inner join clinicamedica.medico m on e.crm = m.crm where e.idesp = {selectedIdEsp}";
            var readerMedicos = db.getReader(sSqlMedicosEspecialidade);
            List<(int CRM, string NomeM)> medicos = new List<(int, string)>();
            index = 0;
            Console.Write("\n------ Médicos disponíveis: ------ \n");

            while (readerMedicos.Read())
            {
                medicos.Add((readerMedicos.GetInt32(0), readerMedicos.GetString(1)));
                Console.WriteLine("[" + index + "] " + readerMedicos.GetString(1));
                index++;
            }
            readerMedicos.Close();

            if (medicos.Count == 0)
            {
                Console.WriteLine("\nNenhum médico encontrado com a especialidade selecionada.\n");
                return;
            }

            Console.Write("\nDigite o número do médico desejado: ");
            int selectedMedicoIndex = Convert.ToInt32(parseInt(Console.ReadLine()));
            if (selectedMedicoIndex < 0 || selectedMedicoIndex >= medicos.Count)
            {
                Console.WriteLine("\nÍndice inválido.\n");
                return;
            }
            var selectedMedico = medicos[selectedMedicoIndex];

            // Passo 3 - Pegar dias e horários disponíveis do médico selecionado
            string sSqlDiasHorariosDisponiveis = $"select idagenda, diasemana, horainicio, horafim from clinicamedica.agenda where crm = {selectedMedico.CRM}";
            var readerDiasHorarios = db.getReader(sSqlDiasHorariosDisponiveis);
            List<(int IdAgenda, string DiaSemana, TimeSpan HoraInicio, TimeSpan HoraFim)> diasHorarios = new List<(int, string, TimeSpan, TimeSpan)>();
            index = 0;

            Console.WriteLine("\n------ Dias e horários disponíveis: ------ ");
            while (readerDiasHorarios.Read())
            {
                diasHorarios.Add((readerDiasHorarios.GetInt32(0), readerDiasHorarios.GetString(1), readerDiasHorarios.GetTimeSpan(2), readerDiasHorarios.GetTimeSpan(3)));
                Console.WriteLine("[" + index + "] " + readerDiasHorarios.GetString(1) + " " + readerDiasHorarios.GetTimeSpan(2) + " - " + readerDiasHorarios.GetTimeSpan(3));
                index++;
            }
            readerDiasHorarios.Close();

            if (diasHorarios.Count == 0)
            {
                Console.WriteLine("\nNenhum dia e horário disponível encontrado para o médico selecionado.\n");
                return;
            }

            Console.Write("\nDigite o número do dia e horário desejados: ");
            int selectedDiaHorarioIndex = Convert.ToInt32(parseInt(Console.ReadLine()));
            if (selectedDiaHorarioIndex < 0 || selectedDiaHorarioIndex >= diasHorarios.Count)
            {
                Console.WriteLine("\nÍndice inválido.\n");
                return;
            }
            var selectedDiaHorario = diasHorarios[selectedDiaHorarioIndex];

            // Passo 4 - Exibir pacientes registrados e escolher um
            string sSqlPacientes = "\nselect idpac, nomep from clinicamedica.paciente\n";
            var readerPacientes = db.getReader(sSqlPacientes);
            List<(int IdPac, string NomeP)> pacientes = new List<(int, string)>();
            index = 0;

            Console.WriteLine("\n------ Pacientes cadastrados: ------ \n");
            while (readerPacientes.Read())
            {
                pacientes.Add((readerPacientes.GetInt32(0), readerPacientes.GetString(1)));
                Console.WriteLine("[" + index + "] " + readerPacientes.GetString(1));
                index++;
            }
            readerPacientes.Close();

            Console.Write("\nDigite o número do paciente desejado: ");
            int selectedPacienteIndex = Convert.ToInt32(parseInt(Console.ReadLine()));
            if (selectedPacienteIndex < 0 || selectedPacienteIndex >= pacientes.Count)
            {
                Console.WriteLine("\nÍndice inválido.\n");
                return;
            }
            var selectedPaciente = pacientes[selectedPacienteIndex];

            // Passo 6 - Coletar informações adicionais da consulta
            Console.WriteLine("\n------ O paciente pagou pela consulta? ------ \n");
            Console.WriteLine("[0] Não");
            Console.WriteLine("[1] Sim\n");
            Console.Write("Digite sua resposta: ");
            int pagouOpcao = Convert.ToInt32(parseInt(Console.ReadLine()));
            bool pagou = pagouOpcao == 1;

            
            Console.WriteLine("\n------ Valor da consulta ------ \n");
            Console.Write("\nDigite o valor da consulta (somente o número): ");
            decimal valorPago = Convert.ToDecimal(Console.ReadLine());

            Console.WriteLine("\n------ Formas de pagamento:------ \n");
            Console.WriteLine("[0] Cartão");
            Console.WriteLine("[1] Dinheiro");
            Console.Write("\nDigite a forma de pagamento: ");
            int formaPagamentoIndex = Convert.ToInt32(parseInt(Console.ReadLine()));
            string formaPagamento = formaPagamentoIndex == 0 ? "Cartão" : "Dinheiro";

            // Passo 7 - Criar a nova consulta
            Consulta novaConsulta = new Consulta
            {
                IdCon = IdCon,
                CRM = selectedMedico.CRM,
                IdEsp = selectedIdEsp,
                IdPac = selectedPaciente.IdPac,
                Data = DateTime.Now.Date,
                DiaSemana = selectedDiaHorario.DiaSemana,
                HoraInicCon = selectedDiaHorario.HoraInicio,
                HoraFimCon = selectedDiaHorario.HoraFim,
                Pagou = pagou,
                ValorPago = valorPago,
                FormaPagamento = formaPagamento
            };

            // Passo 8 - Marcar consulta no banco
            string sSqlMarcarConsulta = "update clinicamedica.consulta set crm = @crm, idesp = @idesp, idpac = @idpac, data = @data, diasemana = @diasemana, horainiccon = @horainiccon, horafimcon = @horafimcon, pagou = @pagou, valorpago = @valorpago, formapagamento = @formapagamento  " +
                                        "where idcon = @idcon";

            using (var cmd = new NpgsqlCommand(sSqlMarcarConsulta, db.Connection))
            {
                cmd.Parameters.AddWithValue("idcon", novaConsulta.IdCon);
                cmd.Parameters.AddWithValue("crm", novaConsulta.CRM);
                cmd.Parameters.AddWithValue("idesp", novaConsulta.IdEsp);
                cmd.Parameters.AddWithValue("idpac", novaConsulta.IdPac);
                cmd.Parameters.AddWithValue("data", novaConsulta.Data);
                cmd.Parameters.AddWithValue("diasemana", novaConsulta.DiaSemana);
                cmd.Parameters.AddWithValue("horainiccon", novaConsulta.HoraInicCon);
                cmd.Parameters.AddWithValue("horafimcon", novaConsulta.HoraFimCon);
                cmd.Parameters.AddWithValue("pagou", novaConsulta.Pagou);
                cmd.Parameters.AddWithValue("valorpago", novaConsulta.ValorPago);
                cmd.Parameters.AddWithValue("formapagamento", novaConsulta.FormaPagamento);

                int result = cmd.ExecuteNonQuery();
                if (result > 0)
                {
                    
                    Console.WriteLine("\n------ Consulta alterada com sucesso! ------ \n");
                    imprimirDetalhesConsulta(novaConsulta, selectedMedico.NomeM, especialidades[selectedEspIndex].NomeE, selectedPaciente.NomeP);
                }
                else
                {
                    Console.WriteLine("\n------ Falha ao alterar consulta. Por favor, tente novamente ------ \n");
                }
            }
            
        }


        private Consulta preencherConsultaMenu(NpgsqlDataReader reader)
        {
            // a.idcon, c.nomep, b.nomem, a.data, a.diasemana,a.horainiccon 
            Consulta consulta = new Consulta();
            consulta.IdCon = reader.GetInt32(0);
            consulta.NomeP = reader.GetString(1);
            consulta.NomeM = reader.GetString(2);
            consulta.Data = reader.GetDateTime(3);
            consulta.DiaSemana = reader.GetString(4);
            consulta.HoraInicCon = reader.GetTimeSpan(5);
            return consulta;
        }


        // Função para imprimir os detalhes da consulta 
        private void imprimirDetalhesConsulta(Consulta consulta, string medico, string especialidade, string paciente)
        {
            // Impressão dos detalhes da consulta
            Console.WriteLine($"ID da Consulta: {consulta.IdCon}");
            Console.WriteLine($"Médico: {medico}");
            Console.WriteLine($"Especialidade: {especialidade}");
            Console.WriteLine($"Paciente: {paciente}");
            Console.WriteLine($"Data: {consulta.Data:dd/MM/yyyy}");
            Console.WriteLine($"Dia da Semana: {consulta.DiaSemana}");
            Console.WriteLine($"Horário: {consulta.HoraInicCon} - {consulta.HoraFimCon}");
            Console.WriteLine($"Pagou: {(consulta.Pagou ? "Sim" : "Não")}");
            Console.WriteLine($"Valor Pago: {consulta.ValorPago}");
            Console.WriteLine($"Forma de Pagamento: {consulta.FormaPagamento}");
        }


        private void marcaConsulta()
        {
            // Passo 1 - Pegar especialidades disponíveis
            string sSqlEspecialidades = "select idesp, nomee from clinicamedica.especialidade";
            var readerEspecialidades = db.getReader(sSqlEspecialidades);
            List<(int IdEsp, string NomeE)> especialidades = new List<(int, string)>();
            int index = 0;

            Console.WriteLine("\n------ Especialidades disponíveis: ------ \n");
            while (readerEspecialidades.Read())
            {
                especialidades.Add((readerEspecialidades.GetInt32(0), readerEspecialidades.GetString(1)));
                Console.WriteLine("[" + index + "] " + readerEspecialidades.GetString(1));
                index++;
            }
            readerEspecialidades.Close();

            Console.Write("\nDigite o número da especialidade desejada: ");
            int selectedEspIndex = Convert.ToInt32(parseInt(Console.ReadLine()));
            if (selectedEspIndex < 0 || selectedEspIndex >= especialidades.Count)
            {
                Console.WriteLine("\nÍndice inválido.\n");
                return;
            }
            int selectedIdEsp = especialidades[selectedEspIndex].IdEsp;

            // Passo 2 - Buscar médicos com a especialidade selecionada
            string sSqlMedicosEspecialidade = $"select m.crm, m.nomem from clinicamedica.exerceesp e inner join clinicamedica.medico m on e.crm = m.crm where e.idesp = {selectedIdEsp}";
            var readerMedicos = db.getReader(sSqlMedicosEspecialidade);
            List<(int CRM, string NomeM)> medicos = new List<(int, string)>();
            index = 0;
            Console.WriteLine("\n------ Médicos disponíveis: ------ \n");

            while (readerMedicos.Read())
            {
                medicos.Add((readerMedicos.GetInt32(0), readerMedicos.GetString(1)));
                Console.WriteLine("[" + index + "] " + readerMedicos.GetString(1));
                index++;
            }
            readerMedicos.Close();

            if (medicos.Count == 0)
            {
                Console.WriteLine("\nNenhum médico encontrado com a especialidade selecionada.\n");
                return;
            }

            Console.Write("\nDigite o número do médico desejado: ");
            int selectedMedicoIndex = Convert.ToInt32(parseInt(Console.ReadLine()));
            if (selectedMedicoIndex < 0 || selectedMedicoIndex >= medicos.Count)
            {
                Console.WriteLine("\nÍndice inválido.\n");
                return;
            }
            var selectedMedico = medicos[selectedMedicoIndex];

            // Passo 3 - Pegar dias e horários disponíveis do médico selecionado
            string sSqlDiasHorariosDisponiveis = $"select idagenda, diasemana, horainicio, horafim from clinicamedica.agenda where crm = {selectedMedico.CRM}";
            var readerDiasHorarios = db.getReader(sSqlDiasHorariosDisponiveis);
            List<(int IdAgenda, string DiaSemana, TimeSpan HoraInicio, TimeSpan HoraFim)> diasHorarios = new List<(int, string, TimeSpan, TimeSpan)>();
            index = 0;

            Console.WriteLine("\n------ Dias e horários disponíveis: ------ \n");
            while (readerDiasHorarios.Read())
            {
                diasHorarios.Add((readerDiasHorarios.GetInt32(0), readerDiasHorarios.GetString(1), readerDiasHorarios.GetTimeSpan(2), readerDiasHorarios.GetTimeSpan(3)));
                Console.WriteLine("[" + index + "] " + readerDiasHorarios.GetString(1) + " " + readerDiasHorarios.GetTimeSpan(2) + " - " + readerDiasHorarios.GetTimeSpan(3));
                index++;
            }
            readerDiasHorarios.Close();

            if (diasHorarios.Count == 0)
            {
                Console.WriteLine("\nNenhum dia e horário disponível encontrado para o médico selecionado.\n");
                return;
            }

            Console.Write("\nDigite o número do dia e horário desejados: ");
            int selectedDiaHorarioIndex = Convert.ToInt32(parseInt(Console.ReadLine()));
            if (selectedDiaHorarioIndex < 0 || selectedDiaHorarioIndex >= diasHorarios.Count)
            {
                Console.WriteLine("\nÍndice inválido.\n");
                return;
            }
            var selectedDiaHorario = diasHorarios[selectedDiaHorarioIndex];

            // Passo 4 - Exibir pacientes registrados e escolher um
            string sSqlPacientes = "select idpac, nomep from clinicamedica.paciente";
            var readerPacientes = db.getReader(sSqlPacientes);
            List<(int IdPac, string NomeP)> pacientes = new List<(int, string)>();
            index = 0;

            Console.WriteLine("\n------ Pacientes cadastrados: ------ \n");
            while (readerPacientes.Read())
            {
                pacientes.Add((readerPacientes.GetInt32(0), readerPacientes.GetString(1)));
                Console.WriteLine("[" + index + "] " + readerPacientes.GetString(1));
                index++;
            }
            readerPacientes.Close();

            Console.Write("\nDigite o número do paciente desejado: ");
            int selectedPacienteIndex = Convert.ToInt32(parseInt(Console.ReadLine()));
            if (selectedPacienteIndex < 0 || selectedPacienteIndex >= pacientes.Count)
            {
                Console.WriteLine("\nÍndice inválido.\n");
                return;
            }
            var selectedPaciente = pacientes[selectedPacienteIndex];

            // Passo 5 - Gerar um novo ID para a consulta
            string sSqlIdConsulta = "select coalesce(max(idcon), 0) + 1 from clinicamedica.consulta";
            var readerIdConsulta = db.getReader(sSqlIdConsulta);
            readerIdConsulta.Read();
            int newIdConsulta = readerIdConsulta.GetInt32(0);
            readerIdConsulta.Close();

            // Passo 6 - Coletar informações adicionais da consulta
            Console.WriteLine("\n------ O paciente pagou pela consulta? ------ \n");
            Console.WriteLine("[0] Não");
            Console.WriteLine("[1] Sim\n");
            Console.Write("Digite sua resposta: ");
            int pagouOpcao = Convert.ToInt32(parseInt(Console.ReadLine()));
            bool pagou = pagouOpcao == 1;

            Console.WriteLine("\n------ Valor da consulta ------ \n");
            Console.Write("\nDigite o valor da consulta (somente o número): ");
            decimal valorPago = Convert.ToDecimal(Console.ReadLine());

            Console.WriteLine("\n------ Formas de pagamento: ------ \n");
            Console.WriteLine("[0] Cartão");
            Console.WriteLine("[1] Dinheiro");
            Console.Write("\nDigite a forma de pagamento: ");
            int formaPagamentoIndex = Convert.ToInt32(parseInt(Console.ReadLine()));
            string formaPagamento = formaPagamentoIndex == 0 ? "Cartão" : "Dinheiro";

            // Passo 7 - Criar a nova consulta
            Consulta novaConsulta = new Consulta
            {
                IdCon = newIdConsulta,
                CRM = selectedMedico.CRM,
                IdEsp = selectedIdEsp,
                IdPac = selectedPaciente.IdPac,
                Data = DateTime.Now.Date,
                DiaSemana = selectedDiaHorario.DiaSemana,
                HoraInicCon = selectedDiaHorario.HoraInicio,
                HoraFimCon = selectedDiaHorario.HoraFim,
                Pagou = pagou,
                ValorPago = valorPago,
                FormaPagamento = formaPagamento
            };

            // Passo 8 - Marcar consulta no banco
            string sSqlMarcarConsulta = "insert into clinicamedica.consulta (idcon, crm, idesp, idpac, data, diasemana, horainiccon, horafimcon, pagou, valorpago, formapagamento) " +
                                        "values (@idcon, @crm, @idesp, @idpac, @data, @diasemana, @horainiccon, @horafimcon, @pagou, @valorpago, @formapagamento)";

            using (var cmd = new NpgsqlCommand(sSqlMarcarConsulta, db.Connection))
            {
                cmd.Parameters.AddWithValue("idcon", novaConsulta.IdCon);
                cmd.Parameters.AddWithValue("crm", novaConsulta.CRM);
                cmd.Parameters.AddWithValue("idesp", novaConsulta.IdEsp);
                cmd.Parameters.AddWithValue("idpac", novaConsulta.IdPac);
                cmd.Parameters.AddWithValue("data", novaConsulta.Data);
                cmd.Parameters.AddWithValue("diasemana", novaConsulta.DiaSemana);
                cmd.Parameters.AddWithValue("horainiccon", novaConsulta.HoraInicCon);
                cmd.Parameters.AddWithValue("horafimcon", novaConsulta.HoraFimCon);
                cmd.Parameters.AddWithValue("pagou", novaConsulta.Pagou);
                cmd.Parameters.AddWithValue("valorpago", novaConsulta.ValorPago);
                cmd.Parameters.AddWithValue("formapagamento", novaConsulta.FormaPagamento);

                int result = cmd.ExecuteNonQuery();
                if (result > 0)
                {
                    Console.WriteLine("\n------ Consulta marcada com sucesso! ------ \n");
                    imprimirDetalhesConsulta(novaConsulta, selectedMedico.NomeM, especialidades[selectedEspIndex].NomeE, selectedPaciente.NomeP);
                }
                else
                {
                    Console.WriteLine("\n------ Falha ao marcar consulta. Por favor, tente novamente ------ \n");
                }
            }
        }


        private void imprimirAgenda()
        {
            // Passo 1 - Imprimir médicos com índice de 0 a n
            string sSqlMedicos = "select crm, nomem, telefonem from clinicamedica.medico";
            var readerMedicos = db.getReader(sSqlMedicos);
            List<int> crms = new List<int>();
            int index = 0;

            Console.WriteLine("\n------ Médicos disponíveis: ------ ");
            Console.WriteLine("\n     | Nome | Telefone | \n");
            while (readerMedicos.Read())
            {
                crms.Add(readerMedicos.GetInt32(0));
                Console.WriteLine("[" + index + "] " + readerMedicos.GetString(1) + " | " + readerMedicos.GetString(2) + "\n");
                index++;
            }
            readerMedicos.Close();

            // Passo 2 - Pegar input do usuário
            Console.Write("Digite o número do médico desejado: ");
            int selectedIndex = Convert.ToInt32(parseInt(Console.ReadLine()));
            if (selectedIndex < 0 || selectedIndex >= crms.Count)
            {
                Console.WriteLine("\nÍndice inválido.\n");
                return;
            }
            int selectedCrm = crms[selectedIndex];

            // Passo 3 - Listar agendas do médico selecionado
            string sSqlAgendas = $"select idagenda, diasemana, horainicio, horafim from clinicamedica.agenda where crm = {selectedCrm}";
            var readerAgendas = db.getReader(sSqlAgendas);
            List<(int IdAgenda, string DiaSemana, TimeSpan HoraInicio, TimeSpan HoraFim)> agendas = new List<(int, string, TimeSpan, TimeSpan)>();

            while (readerAgendas.Read())
            {
                agendas.Add((readerAgendas.GetInt32(0), readerAgendas.GetString(1), readerAgendas.GetTimeSpan(2), readerAgendas.GetTimeSpan(3)));
            }
            readerAgendas.Close();

            // Passo 4 - Listar consultas do médico selecionado
            string sSqlConsultas = $"select idcon, diasemana, horainiccon, horafimcon, data from clinicamedica.consulta where crm = {selectedCrm}";
            var readerConsultas = db.getReader(sSqlConsultas);
            List<(string DiaSemana, TimeSpan HoraInicCon, TimeSpan HoraFimCon, DateTime Data)> consultas = new List<(string, TimeSpan, TimeSpan, DateTime)>();

            while (readerConsultas.Read())
            {
                consultas.Add((readerConsultas.GetString(1), readerConsultas.GetTimeSpan(2), readerConsultas.GetTimeSpan(3), readerConsultas.GetDateTime(4)));
            }
            readerConsultas.Close();

            // Passo 5 e 6 - Classificar horários na agenda como Vago ou Ocupado e imprimir agendas
            Console.WriteLine("\n------ Agenda do médico selecionado: ------ \n");
            Console.WriteLine(" | Dia da Semana | Horário Início | Horário Fim | Status |\n");

            foreach (var agenda in agendas)
            {
                string status = "Horário vago";
                List<string> datasOcupadas = new List<string>();

                foreach (var consulta in consultas)
                {
                    if (agenda.DiaSemana == consulta.DiaSemana &&
                        agenda.HoraInicio == consulta.HoraInicCon &&
                        agenda.HoraFim == consulta.HoraFimCon)
                    {
                        status = "Horário ocupado nos dias: ";
                        datasOcupadas.Add(consulta.Data.ToString("dd/MM/yyyy"));
                    }
                }

                if (datasOcupadas.Count > 0)
                {
                    status += string.Join(", ", datasOcupadas);
                }

                Console.WriteLine($" | {agenda.DiaSemana} | {agenda.HoraInicio} | {agenda.HoraFim} | {status} | \n");
            }
        }


        private void imprimirMedicos()
        {
            string sSql = "select nomem, telefonem from clinicamedica.medico";
            var reader = db.getReader(sSql);
            Console.Write("\n------ Médicos disponíveis: ------ \n");
            Console.WriteLine("\n | Nome | Telefone | ");
            while (reader.Read())
            {
                Console.Write("\n | " + reader.GetString(0) + " | ");
                Console.Write(reader.GetString(1) + " | \n");
            }
            reader.Close();
        }


        private void imprimirConsultas(){
            string sSql = "select a.idcon, c.nomep, b.nomem, a.data, a.diasemana,a.horainiccon ";
            sSql += " from clinicamedica.consulta a ";
            sSql += " inner join clinicamedica.medico b on a.crm = b.crm  ";
            sSql += " inner join clinicamedica.paciente c on a.idpac = c.idpac ";

            var reader = db.getReader(sSql);
            Console.WriteLine("\n------ Consultas marcadas: ------ \n");
            Console.WriteLine(" | id | Paciente | Médico | Data | Dia da Semana | Horário | ");
            while (reader.Read())
            {
                Console.Write("\n | " + reader.GetInt32(0) + " | ");
                Console.Write(reader.GetString(1) + " | ");
                Console.Write(reader.GetString(2) + " | ");
                Console.Write(reader.GetDateTime(3).ToString("dd/MM/yyyy") + " | ");
                Console.Write(reader.GetString(4)+ " | ");
                Console.Write(reader.GetTimeSpan(5)+" | \n");
            }

            reader.Close();
        }


        public int parseInt(string toConvert){ 
            try{
                return Convert.ToInt32(toConvert);
            }catch(Exception err){
                return 0;
            }
        }
    }
}