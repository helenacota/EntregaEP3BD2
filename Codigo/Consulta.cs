namespace MisConsultas
{
    public class Consulta
    {
        public int IdCon { get; set; }
        public int CRM { get; set; }

        public string NomeP { get; set; }
        public string NomeM { get; set; }

        public int IdEsp { get; set; }
        public int IdPac { get; set; }
        public DateTime Data { get; set; }
        public string DiaSemana { get; set; }
        public TimeSpan HoraInicCon { get; set; }
        public TimeSpan HoraFimCon { get; set; }
        public bool Pagou { get; set; }
        public decimal ValorPago { get; set; }
        public string FormaPagamento { get; set; }
    }
}