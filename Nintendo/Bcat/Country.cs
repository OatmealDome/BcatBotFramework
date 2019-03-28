using System;

namespace Nintendo.Bcat
{
    public enum Country
    {
        Australia,
        Austria,
        Belgium,
        Brazil,
        Canada,
        CzechRepublic,
        Denmark,
        Finland,
        France,
        Germany,
        HongKong,
        Hungary,
        Ireland,
        Italy,
        Japan,
        Mexico,
        Netherlands,
        NewZealand,
        Norway,
        Poland,
        Portugal,
        Russia,
        SouthAfrica,
        SouthKorea,
        Spain,
        Sweden,
        Switzerland,
        UnitedKingdom,
        UnitedStates,
    }

    public static class CountryExtensions
    {
        public static string ToBcatString(this Country country)
        {
            switch (country)
            {
                case Country.Australia:
                    return "AU";
                case Country.Austria:
                    return "AT";
                case Country.Belgium:
                    return "BE";
                case Country.Brazil:
                    return "BR";
                case Country.Canada:
                    return "CA";
                case Country.CzechRepublic:
                    return "CZ";
                case Country.Denmark:
                    return "DK";
                case Country.Finland:
                    return "FI";
                case Country.France:
                    return "FR";
                case Country.Germany:
                    return "DE";
                case Country.HongKong:
                    return "HK";
                case Country.Hungary:
                    return "HU";
                case Country.Ireland:
                    return "IE";
                case Country.Italy:
                    return "IT";
                case Country.Japan:
                    return "JP";
                case Country.Mexico:
                    return "MX";
                case Country.Netherlands:
                    return "NL";
                case Country.NewZealand:
                    return "NZ";
                case Country.Norway:
                    return "NO";
                case Country.Poland:
                    return "PL";
                case Country.Portugal:
                    return "PT";
                case Country.Russia:
                    return "RU";
                case Country.SouthAfrica:
                    return "ZA";
                case Country.SouthKorea:
                    return "KR";
                case Country.Spain:
                    return "ES";
                case Country.Sweden:
                    return "SE";
                case Country.Switzerland:
                    return "CH";
                case Country.UnitedKingdom:
                    return "GB";
                case Country.UnitedStates:
                    return "US";
                default:
                    throw new Exception("Unknown country");
            }
        }

        public static Country[] GetAllCountries()
        {
            return (Country[])Enum.GetValues(typeof(Country));
        }

    }
}
