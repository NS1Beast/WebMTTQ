namespace WebDoAn.Services
{
    public interface ILuotTruyCapService
    {
        Task<int> DemLuotTruyCapAsync();
        Task<int> DemLuotTruyCapHomNayAsync();
        Task<List<int>> ThongKe7NgayAsync();
        Task<List<string>> Lay7NgayGanNhatAsync();
    }
}