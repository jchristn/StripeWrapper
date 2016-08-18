# Stripe Wrapper

[![][nuget-img]][nuget]

[nuget]:     https://www.nuget.org/packages/StripeWrapper/
[nuget-img]: https://badge.fury.io/nu/Object.svg

A simple C# class library for performing Stripe charges and refunds.  This class isn't meant to be a complete SDK for Stripe but rather a simple example to help you construct and manage your own requests and responses.

## Test App
A test project is included which will help you exercise the class library.

## Example
```
using StripeWrapper;
using RestWrapper;
using Newtonsoft.Json.Linq;   // for JObject response body

StripeWrapper wrapper = new StripeWrapper("Your API key");

JObject responseBody;
string stripeChargeTxnID;
string stripeRefundTxnID;
string stripeCardID;

//
// Charge a card
//
if (wrapper.Charge(
   null,
   100, "usd", 12, 2018, "4242424242424242",
   "123 Some Street", "San Jose", "CA", "95128",
   "111", "SOME NAME ON CARD", "Test Transaction",
   out stripeCardID,
   out stripeChargeTxnID,
   out responseBody))
{
   Console.WriteLine("Success");
   Console.WriteLine("  Card ID       : " + stripeCardID);
   Console.WriteLine("  Charge Txn ID : " + stripeChargeTxnID);
   Console.WriteLine("Response Body");
   Console.WriteLine(wrapper.SerializeJson(responseBody));
}
else
{
   Console.WriteLine("Failed");
   Console.WriteLine("Response Body");
   Console.WriteLine(wrapper.SerializeJson(responseBody));
}

//
// Refund a previous charge
//
if (wrapper.Refund(
   "Charge transaction ID here",
   out stripeRefundTxnID,
   out responseBody))
{
   Console.WriteLine("Success");
   Console.WriteLine("  Refund Txn ID : " + stripeRefundTxnID);
   Console.WriteLine("Response Body");
   Console.WriteLine(wrapper.SerializeJson(responseBody));
}
else
{
   Console.WriteLine("Failed");
   Console.WriteLine("Response Body");
   Console.WriteLine(wrapper.SerializeJson(responseBody));
}
```