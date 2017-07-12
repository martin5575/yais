using GalaSoft.MvvmLight;

namespace Yais.Model
{
    public class ImpressumItem : ObservableObject
    {
        public string Name { get; set; }
        public string Street { get; set; }
        public string HouseNumber { get; set; }
        public string ZipCode { get; set; }
        public string City { get; set; }
        public string TelephoneNumber { get; set; }
        public string EMailAddress { get; set; }
        public string LegalIdentifier { get; set; }
        public string TaxIdentifier { get; set; }

    }
}