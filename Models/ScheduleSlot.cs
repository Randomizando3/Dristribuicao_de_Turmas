namespace DistribuicaoTurmas.Models
{
    // Horários possíveis: Segunda/…/Sexta x (M1, M2, N1, N2)
    public enum ScheduleSlot
    {
        Segunda_M1, Segunda_M2, Segunda_N1, Segunda_N2,
        Terca_M1, Terca_M2, Terca_N1, Terca_N2,
        Quarta_M1, Quarta_M2, Quarta_N1, Quarta_N2,
        Quinta_M1, Quinta_M2, Quinta_N1, Quinta_N2,
        Sexta_M1, Sexta_M2, Sexta_N1, Sexta_N2
    }
}
