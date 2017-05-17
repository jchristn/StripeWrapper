using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;  // also add using References > Add Reference > Assemblies
using RestWrapper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace StripeWrapper
{
    public class StripeWrapper
    {
        #region Constructor

        /// <summary>
        /// Simple wrapper for Stripe charges and refunds.
        /// </summary>
        /// <param name="apiKey">Your Stripe API key.  Use either your development or production API key.</param>
        public StripeWrapper(string apiKey)
        {
            if (String.IsNullOrEmpty(apiKey)) throw new ArgumentNullException("apiKey");
            Endpoint = "https://api.stripe.com/v1/";
            ApiKey = apiKey;
        }

        #endregion

        #region Public-Members

        /// <summary>
        /// Your development or production API key.
        /// </summary>
        public string ApiKey;
        
        #endregion

        #region Private-Members

        private string Endpoint;

        #endregion

        #region Public-Methods

        /// <summary>
        /// Process a charge against a card.
        /// </summary>
        /// <param name="metadata">A dictionary containing user-supplied key-value pairs.</param>
        /// <param name="amount">The amount in Stripe's preferred format, i.e. $5.00 should be set to 500.</param>
        /// <param name="currency">The currency associated with the charge.  Defaults to usd.</param>
        /// <param name="expMonth">The expiration month of the card.</param>
        /// <param name="expYear">The expiration year of the card.</param>
        /// <param name="cardNumber">The credit card number.</param>
        /// <param name="address1">The billing street address of the card.</param>
        /// <param name="city">The billing city of the card.</param>
        /// <param name="state">The biling state of the card.</param>
        /// <param name="zip">The billing zip code of the card.</param>
        /// <param name="cvv2">The CVV2 value on the back of the card.</param>
        /// <param name="nameOnCard">The cardholder name as it appears on the card.</param>
        /// <param name="description">The description of the transaction.</param>
        /// <param name="stripeCardID">The ID given by Stripe to represent this card.</param>
        /// <param name="stripeChargeTxnID">The ID given by Stripe to represent this charge transaction.</param>
        /// <param name="responseBody">The JObject response body of Stripe's response data, in its entirety.</param>
        /// <returns>A Boolean indicating whether or not the charge was successful.</returns>
        public bool Charge(
            Dictionary<string, string> metadata,
            int amount, // $5.00 would be 500
            string currency,
            int expMonth,
            int expYear,
            string cardNumber,
            string address1,
            string city,
            string state,
            string zip,
            string cvv2,
            string nameOnCard,
            string description,
            out string stripeCardID,
            out string stripeChargeTxnID,
            out JObject responseBody)
        {
            responseBody = new JObject();
            stripeCardID = null;
            stripeChargeTxnID = null;

            Dictionary<string, object> requestBody = new Dictionary<string, object>();

            if (amount < 50) throw new ArgumentOutOfRangeException("amount");
            if (String.IsNullOrEmpty(currency)) currency = "USD";
            if (expMonth < 1 || expMonth > 12) throw new ArgumentOutOfRangeException("expMonth");
            if (String.IsNullOrEmpty(cardNumber)) throw new ArgumentNullException("cardNumber");
            
            if (metadata != null && metadata.Count > 0)
            {
                foreach (KeyValuePair<string, string> curr in metadata)
                {
                    if (String.IsNullOrEmpty(curr.Key)) continue;
                    if (String.IsNullOrEmpty(curr.Value)) continue;
                    requestBody.Add("metadata[" + curr.Key + "]", curr.Value);
                }
            }

            requestBody.Add("amount", amount);
            requestBody.Add("currency", currency);
            requestBody.Add("capture", "true");

            requestBody.Add("source[object]", "card");
            requestBody.Add("source[exp_month]", expMonth);
            requestBody.Add("source[exp_year]", expYear);
            requestBody.Add("source[number]", cardNumber);
            if (!String.IsNullOrEmpty(address1)) requestBody.Add("source[address_line1]", address1);
            if (!String.IsNullOrEmpty(city)) requestBody.Add("source[address_city]", city);
            if (!String.IsNullOrEmpty(state)) requestBody.Add("source[address_state]", state);
            if (!String.IsNullOrEmpty(zip)) requestBody.Add("source[address_zip]", zip);
            if (!String.IsNullOrEmpty(cvv2)) requestBody.Add("source[cvc]", cvv2);
            if (!String.IsNullOrEmpty(nameOnCard)) requestBody.Add("source[name]", nameOnCard);
            if (!String.IsNullOrEmpty(description)) requestBody.Add("description", description);

            if (!StripeRequestWrapper("charges", "POST", StripeRequestBodyBuilder(requestBody), out responseBody)) return false;
            else
            {
                // Console.WriteLine("Success");
                // Console.WriteLine("  Card ID        : " + stripeCardID);
                // Console.WriteLine("  Charge Txn ID  : " + stripeChargeTxnID);
                // Console.WriteLine(SerializeJson(responseBody));
                stripeCardID = responseBody["source"]["id"].ToString();
                stripeChargeTxnID = responseBody["id"].ToString();
                return true;
            }
        }

        /// <summary>
        /// Process a refund for a previous charge.
        /// </summary>
        /// <param name="stripeChargeTxnID">The ID given by Stripe to represent the previous charge transaction you wish to void or refund.</param>
        /// <param name="stripeRefundTxnID">The ID given by Stripe to represent the refund transaction.</param>
        /// <param name="responseBody">The JObject response body of Stripe's response data, in its entirety.</param>
        /// <returns>A Boolean indicating whether or not the refund was successful.</returns>
        public bool Refund(
            string stripeChargeTxnID,
            out string stripeRefundTxnID,
            out JObject responseBody)
        {
            responseBody = new JObject();
            stripeRefundTxnID = null;

            if (String.IsNullOrEmpty(stripeChargeTxnID)) throw new ArgumentNullException("stripeChargeTxnID");

            if (!StripeRequestWrapper("refunds", "POST", "charge=" + stripeChargeTxnID, out responseBody)) return false;
            else
            {
                string status = responseBody["status"].ToString();
                stripeRefundTxnID = responseBody["id"].ToString();

                if (String.IsNullOrEmpty(status)) return false;
                if (String.Compare(status.ToLower(), "succeeded") != 0) return false;
                if (String.IsNullOrEmpty(stripeRefundTxnID)) return false;

                // Console.WriteLine("Success");
                // Console.WriteLine("  Refund Txn ID : " + stripeRefundTxnID);
                // Console.WriteLine(SerializeJson(responseBody));
                return true;
            }
        }

        /// <summary>
        /// Serialize an object to its JSON string representation.
        /// </summary>
        /// <param name="obj">The object you wish to serialize to a JSON string.</param>
        /// <returns>A JSON string.</returns>
        public string SerializeJson(object obj)
        {
            if (obj == null) return null;
            string json = JsonConvert.SerializeObject(
                obj,
                Newtonsoft.Json.Formatting.Indented,
                new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    DateTimeZoneHandling = DateTimeZoneHandling.Utc
                });

            return json;
        }

        #endregion

        #region Private-Methods

        private bool StripeRequestWrapper(string URL, string method, string requestBody, out JObject responseBody)
        {
            Console.WriteLine(method + " " + Endpoint + URL + " (API key " + ApiKey + ")");
            responseBody = null;

            RestResponse resp = RestRequest.SendRequest(
                Endpoint + URL,
                "application/x-www-form-urlencoded",
                method,
                ApiKey, null, true, false,
                null,
                Encoding.UTF8.GetBytes(requestBody));

            if (resp == null) return false;

            if (resp.StatusCode == 200 || resp.StatusCode == 201)
            {
                if (resp.Data != null && resp.Data.Length > 0)
                {
                    // Console.WriteLine("200/201 success status from " + metod + " " + URL + " (" + resp.Data.Length + " bytes)");
                    responseBody = JObject.Parse(Encoding.UTF8.GetString(resp.Data));
                    return true;
                }
                else
                {
                    // Console.WriteLine("200/201 success status from " + method + " " + URL + " (no data)");
                    responseBody = null;
                    return true;
                }
            }
            else
            {
                if (resp.Data != null && resp.Data.Length > 0)
                {
                    // Console.WriteLine(resp.StatusCode + " failure status from " + method + " " + URL + " (" + resp.Data.Length + " bytes)");
                    responseBody = JObject.Parse(Encoding.UTF8.GetString(resp.Data));
                    return false;
                }
                else
                {
                    // Console.WriteLine(resp.StatusCode + " failure status from " + method + " " + URL + " (no data)");
                    responseBody = null;
                    return false;
                }
            }
        }

        private string StripeRequestBodyBuilder(Dictionary<string, object> requestData)
        {
            if (requestData == null || requestData.Count < 1) return null;
            string ret = "";

            foreach (KeyValuePair<string, object> curr in requestData)
            {
                if (String.IsNullOrEmpty(ret))
                {
                    if (String.IsNullOrEmpty(curr.Key)) continue;
                    if (curr.Value == null) continue;
                    ret += HttpUtility.UrlEncode(curr.Key) + "=" + HttpUtility.UrlEncode(curr.Value.ToString());
                }
                else
                {
                    if (String.IsNullOrEmpty(curr.Key)) continue;
                    if (curr.Value == null) continue;
                    ret += "&" + HttpUtility.UrlEncode(curr.Key) + "=" + HttpUtility.UrlEncode(curr.Value.ToString());
                }
            }

            return ret;
        }

        #endregion

        #region Public-Static-Methods

        #endregion

        #region Private-Static-Methods

        #endregion
    }
}
