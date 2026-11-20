using System.Globalization;
using System.Net.Http.Headers;
using System.Text.Json;

namespace CT.Postojna.Services;

public sealed class StripeCheckoutService(
    HttpClient httpClient,
    IConfiguration configuration,
    ILogger<StripeCheckoutService> logger)
{
    private static readonly IReadOnlyDictionary<string, (string Name, long UnitAmount)> Products =
        new Dictionary<string, (string Name, long UnitAmount)>
        {
            ["burger-postojna"] = ("Postojna Burger L", 3000),
            ["burger-slovenia"] = ("Slovenia Burger XL", 5000),
            ["burger-maribor"] = ("Maribor Burger XL", 7500),
            ["burger-ljubljana"] = ("Ljubljana Burger XXL", 10000),
            ["pasta-postojna"] = ("Postojna Pasta Bolognese", 3000),
            ["pasta-slovenia"] = ("Slovenia Pasta Verdura", 5000),
            ["pasta-maribor"] = ("Maribor Pasta Arabiata", 7500),
            ["pasta-ljubljana"] = ("Ljubljana Pasta", 10000)
        };

    public async Task<Uri> CreateCheckoutSessionAsync(
        IEnumerable<(string ProductId, int Quantity)> cartItems,
        Uri appBaseUri,
        CancellationToken cancellationToken = default)
    {
        var secretKey = "sk_test_51U2S9wDyVAspCC9RJM4MlnHRzn3LIwMQTwDOpUlSYvVYuL5z1yByaejcbR0OxjeSEBjtHj0IRfC7DMbnLXWxlGi500OVqob90Q";
        //var secretKey = configuration["Stripe:SecretKey"];
        if (string.IsNullOrWhiteSpace(secretKey))
        {
            logger.LogError("Stripe checkout was requested, but Stripe:SecretKey is not configured.");
            throw new StripeCheckoutException("Stripe payments are not configured. Please contact the site administrator.");
        }

        if (!appBaseUri.IsAbsoluteUri || appBaseUri.Scheme is not ("http" or "https"))
        {
            throw new StripeCheckoutException("The checkout return address is invalid.");
        }

        var items = cartItems.ToList();
        if (items.Count is 0 or > 8 || items.Any(item =>
                !Products.ContainsKey(item.ProductId) || item.Quantity is < 1 or > 99))
        {
            throw new StripeCheckoutException("The cart contains an invalid item or quantity. Please review your cart and try again.");
        }

        var form = new List<KeyValuePair<string, string>>
        {
            new("mode", "payment"),
            new("success_url", new Uri(appBaseUri, "?checkout=success").AbsoluteUri),
            new("cancel_url", new Uri(appBaseUri, "?checkout=cancelled").AbsoluteUri)
        };

        for (var index = 0; index < items.Count; index++)
        {
            var item = items[index];
            var product = Products[item.ProductId];
            var prefix = $"line_items[{index}]";
            form.Add(new($"{prefix}[price_data][currency]", "usd"));
            form.Add(new($"{prefix}[price_data][product_data][name]", product.Name));
            form.Add(new($"{prefix}[price_data][unit_amount]", product.UnitAmount.ToString(CultureInfo.InvariantCulture)));
            form.Add(new($"{prefix}[quantity]", item.Quantity.ToString(CultureInfo.InvariantCulture)));
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.stripe.com/v1/checkout/sessions")
        {
            Content = new FormUrlEncodedContent(form)
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", secretKey);

        HttpResponseMessage response;
        try
        {
            response = await httpClient.SendAsync(request, cancellationToken);
        }
        catch (HttpRequestException exception)
        {
            logger.LogError(exception, "Unable to connect to Stripe to create a checkout session.");
            throw new StripeCheckoutException("Unable to connect to Stripe. Please try again.");
        }

        using (response)
        {
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var stripeMessage = GetStripeErrorMessage(responseBody);
                logger.LogWarning("Stripe checkout session creation failed with status {StatusCode}: {StripeMessage}", response.StatusCode, stripeMessage);
                throw new StripeCheckoutException("Stripe could not start checkout. Please try again.");
            }

            try
            {
                using var document = JsonDocument.Parse(responseBody);
                if (document.RootElement.TryGetProperty("url", out var urlElement) &&
                    Uri.TryCreate(urlElement.GetString(), UriKind.Absolute, out var checkoutUri) &&
                    checkoutUri.Scheme == Uri.UriSchemeHttps)
                {
                    return checkoutUri;
                }
            }
            catch (JsonException exception)
            {
                logger.LogError(exception, "Stripe returned an invalid checkout session response.");
            }
        }

        logger.LogError("Stripe returned a successful response without a valid checkout URL.");
        throw new StripeCheckoutException("Stripe returned an invalid checkout response. Please try again.");
    }

    private static string GetStripeErrorMessage(string responseBody)
    {
        try
        {
            using var document = JsonDocument.Parse(responseBody);
            var root = document.RootElement;
            if (root.ValueKind == JsonValueKind.Object &&
                root.TryGetProperty("error", out var error) &&
                error.ValueKind == JsonValueKind.Object &&
                error.TryGetProperty("message", out var message))
            {
                return message.GetString() ?? "No error message provided.";
            }
        }
        catch (JsonException)
        {
            return "Stripe returned an unsuccessful response.";
        }

        return "Stripe returned an unsuccessful response.";
    }
}

public sealed class StripeCheckoutException(string message) : Exception(message)
{
}
