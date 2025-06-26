using GH_Metodos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MauiApp1
{
    /// <summary>
    /// Classe auxiliar para integração com o serviço GetTurnosCalendario
    /// </summary>
    public class CalendarioHelper
    {
        private readonly Service6SoapClient _service;
        private readonly int _idColaborador;
        private readonly string _token;

        public CalendarioHelper(Service6SoapClient service, int idColaborador, string token)
        {
            _service = service;
            _idColaborador = idColaborador;
            _token = token;
        }

        public async Task<List<TurnoCalendarioInfo>> ObterTurnosCalendarioAsync(DateTime dataInicio, DateTime dataFim)
        {
            try
            {
                string dataInicioStr = dataInicio.ToString("dd/MM/yyyy");
                string dataFimStr = dataFim.ToString("dd/MM/yyyy");

                var resposta = await _service.GetTurnosCalendarioAsync(_idColaborador, _token, dataInicioStr, dataFimStr);
                var resultado = resposta?.Body?.GetTurnosCalendarioResult;

                if (resultado?._erro?.erro != 0)
                {
              
                    throw new Exception($"Erro do serviço: {resultado?._erro?.erroMensagem}");
                }

                var turnos = resultado?.aTurnosCalendario?.ToList() ?? new List<TurnoCalendario>();

          
                if (!turnos.Any())
                {
                   
                    return new List<TurnoCalendarioInfo>();
                }

                return turnos.Select(t => new TurnoCalendarioInfo
                {
                    Tipo = t.tipo,
                    Titulo = t.titulo,
                    Descricao = t.descricao,
                    Cor = t.cor,
                    IdCorGoogle = t.idCorGoogle,
                    CodigoCorGoogle = t.codigoCorGoogle,
                    Localizacao = t.localizacao,
                    TipoEvento = DeterminarTipoEvento(t),
                    DataHoraInicio = ParseDateTime(t.dataHoraInicio),
                    DataHoraFim = ParseDateTime(t.dataHoraFim)
                }).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao obter turnos do calendário: {ex.Message}", ex);
            }
        }

        public async Task<List<TurnoCalendarioInfo>> ObterFeriasAsync(DateTime dataInicio, DateTime dataFim)
        {
            var todosTurnos = await ObterTurnosCalendarioAsync(dataInicio, dataFim);
            return todosTurnos.Where(t => t.TipoEvento == TipoEventoCalendario.Ferias).ToList();
        }

        public async Task<List<TurnoCalendarioInfo>> ObterAbstinenciasAsync(DateTime dataInicio, DateTime dataFim)
        {
            var todosTurnos = await ObterTurnosCalendarioAsync(dataInicio, dataFim);
            return todosTurnos.Where(t => t.TipoEvento == TipoEventoCalendario.Abstinencia).ToList();
        }

        public async Task<List<TurnoCalendarioInfo>> ObterTurnosTrabalhoAsync(DateTime dataInicio, DateTime dataFim)
        {
            var todosTurnos = await ObterTurnosCalendarioAsync(dataInicio, dataFim);
            return todosTurnos.Where(t => t.TipoEvento == TipoEventoCalendario.Turno).ToList();
        }

        private TipoEventoCalendario DeterminarTipoEvento(TurnoCalendario turno)
        {
            var tipo = turno.tipo?.ToLower() ?? "";
            var titulo = turno.titulo?.ToLower() ?? "";
            var descricao = turno.descricao?.ToLower() ?? "";

            if (tipo.Contains("ferias") || titulo.Contains("ferias") || descricao.Contains("ferias"))
            {
                return TipoEventoCalendario.Ferias;
            }

            if (tipo.Contains("abstinencia") || tipo.Contains("falta") ||
                titulo.Contains("abstinencia") || titulo.Contains("falta") ||
                descricao.Contains("abstinencia") || descricao.Contains("falta"))
            {
                return TipoEventoCalendario.Abstinencia;
            }

            if (tipo.Contains("ausencia") || titulo.Contains("ausencia") || descricao.Contains("ausencia") ||
                tipo.Contains("licenca") || titulo.Contains("licenca") || descricao.Contains("licenca"))
            {
                return TipoEventoCalendario.Abstinencia;
            }

            return TipoEventoCalendario.Turno;
        }

        private DateTime? ParseDateTime(string dateTimeString)
        {
            if (string.IsNullOrEmpty(dateTimeString))
                return null;

            if (DateTime.TryParse(dateTimeString, out DateTime result))
                return result;

            return null;
        }

        public async Task<EstatisticasCalendario> ObterEstatisticasAsync(DateTime dataInicio, DateTime dataFim)
        {
            var todosTurnos = await ObterTurnosCalendarioAsync(dataInicio, dataFim);

            return new EstatisticasCalendario
            {
                TotalEventos = todosTurnos.Count,
                TotalFerias = todosTurnos.Count(t => t.TipoEvento == TipoEventoCalendario.Ferias),
                TotalAbstinencias = todosTurnos.Count(t => t.TipoEvento == TipoEventoCalendario.Abstinencia),
                TotalTurnos = todosTurnos.Count(t => t.TipoEvento == TipoEventoCalendario.Turno),
                PeriodoInicio = dataInicio,
                PeriodoFim = dataFim
            };
        }
    }

    public class TurnoCalendarioInfo
    {
        public string Tipo { get; set; }
        public DateTime? DataHoraInicio { get; set; }
        public DateTime? DataHoraFim { get; set; }
        public string Titulo { get; set; }
        public string Descricao { get; set; }
        public string Cor { get; set; }
        public string IdCorGoogle { get; set; }
        public string CodigoCorGoogle { get; set; }
        public string Localizacao { get; set; }
        public TipoEventoCalendario TipoEvento { get; set; }

        public string ResumoEvento => $"{Titulo} ({DataHoraInicio?.ToString("dd/MM/yyyy HH:mm")} - {DataHoraFim?.ToString("dd/MM/yyyy HH:mm")})";
    }

    public enum TipoEventoCalendario
    {
        Turno,
        Ferias,
        Abstinencia
    }

    public class EstatisticasCalendario
    {
        public int TotalEventos { get; set; }
        public int TotalFerias { get; set; }
        public int TotalAbstinencias { get; set; }
        public int TotalTurnos { get; set; }
        public DateTime PeriodoInicio { get; set; }
        public DateTime PeriodoFim { get; set; }

        public string Resumo => $"Período: {PeriodoInicio:dd/MM/yyyy} - {PeriodoFim:dd/MM/yyyy}\n" +
                                $"Total de eventos: {TotalEventos}\n" +
                                $"Férias: {TotalFerias}\n" +
                                $"Abstinências: {TotalAbstinencias}\n" +
                                $"Turnos: {TotalTurnos}";
    }
}