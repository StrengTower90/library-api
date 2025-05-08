using Microsoft.Extensions.Options;

namespace LibraryAPI.PatternOnOptions
{
    public class ProccesingPayments
    {
        private RatesOptions _ratesOptions;

        public ProccesingPayments(IOptionsMonitor<RatesOptions> optionsMonitor)
        {
            _ratesOptions = optionsMonitor.CurrentValue;

            optionsMonitor.OnChange(newRates =>
            {
                Console.WriteLine("Rates Updates");
                _ratesOptions = newRates;
            });
        }

        public void ProcessPayment()
        {
            // Here we use the rates
        }

        public RatesOptions GetRates()
        {
            return _ratesOptions;
        }
    }
}
