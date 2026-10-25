using System.Globalization;

namespace DoualaClean.Services;

public record LocationResult(Location? Location, string Address, string Quartier, string Error);

public static class LocationHelper
{
    public static string FormatGps(double lat, double lon)
    {
        var ci = CultureInfo.InvariantCulture;
        var ns = lat >= 0 ? "N" : "S";
        var ew = lon >= 0 ? "E" : "W";
        return $"{Math.Abs(lat).ToString("0.0000", ci)}° {ns}, {Math.Abs(lon).ToString("0.0000", ci)}° {ew}";
    }

    public static async Task<LocationResult> GetCurrentAsync()
    {
        try
        {
            var status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            if (status != PermissionStatus.Granted)
                return new(null, "", "", "Autorisez la localisation pour utiliser cette fonction.");

            var loc = await Geolocation.Default.GetLocationAsync(
                new GeolocationRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(10)));
            if (loc is null)
                return new(null, "", "", "Position introuvable. Vérifiez que le GPS est activé.");

            string address = "", quartier = "";
            try
            {
                var p = (await Geocoding.Default.GetPlacemarksAsync(loc))?.FirstOrDefault();
                if (p is not null)
                {
                    quartier = p.SubLocality ?? p.SubAdminArea ?? p.Locality ?? "";
                    address = string.Join(", ", new[] { p.Thoroughfare, p.SubLocality ?? p.Locality, p.Locality }
                        .Where(s => !string.IsNullOrWhiteSpace(s)).Distinct());
                }
            }
            catch { /* le géocodage inverse est optionnel */ }

            if (string.IsNullOrWhiteSpace(address)) address = FormatGps(loc.Latitude, loc.Longitude);
            return new(loc, address, quartier, "");
        }
        catch (FeatureNotEnabledException) { return new(null, "", "", "Activez le GPS de votre téléphone."); }
        catch (FeatureNotSupportedException) { return new(null, "", "", "Le GPS n'est pas disponible sur cet appareil."); }
        catch (Exception) { return new(null, "", "", "Impossible d'obtenir votre position."); }
    }
}
