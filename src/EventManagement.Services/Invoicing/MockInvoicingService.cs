using System.Threading.Tasks;
using losol.EventManagement.Domain;

namespace losol.EventManagement.Services.Invoicing
{
    public class MockInvoicingService : IInvoicingService, IPowerOfficeService, IStripeInvoiceService
    {
        public async Task<bool> CreateInvoiceAsync(Order order) => await Task.FromResult(true);
    }
}