﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StripeWrapper;
using RestWrapper;
using Newtonsoft.Json.Linq;

namespace StripeWrapper
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                string apiKey = "";
                while (String.IsNullOrEmpty(apiKey))
                {
                    Console.Write("Stripe API key: ");
                    apiKey = Console.ReadLine();
                }

                StripeWrapper wrapper = new StripeWrapper(apiKey);

                bool runForever = true;
                while (runForever)
                {
                    JObject responseBody;
                    string userInput = "";
                    while (String.IsNullOrEmpty(userInput))
                    {
                        Console.Write("Command [charge refund quit]: ");
                        userInput = Console.ReadLine();
                    }

                    switch (userInput)
                    {
                        case "charge":
                            string stripeChargeTxnID = "";
                            string stripeCardID = "";

                            if (wrapper.Charge(
                                null,
                                UserInputInt("Amount", 100, true, false),
                                UserInputString("Currency", "usd", false),
                                UserInputInt("Expiration Month", 1, true, false),
                                UserInputInt("Expiration Year", 2017, true, false),
                                UserInputString("Card Number", "4242424242424242", false),
                                UserInputString("Street Address", "123 Some Street", true),
                                UserInputString("City", "San Jose", true),
                                UserInputString("State", "CA", true),
                                UserInputString("Zip Code", "95128", true),
                                UserInputString("CVV2", "111", true),
                                UserInputString("Name On Card", "SOME PERSON", true),
                                UserInputString("Description", "Test Transaction", true),
                                out stripeCardID,
                                out stripeChargeTxnID,
                                out responseBody))
                            {
                                Console.WriteLine("Success");
                                Console.WriteLine("  Card ID        : " + stripeCardID);
                                Console.WriteLine("  Charge Txn ID  : " + stripeChargeTxnID);
                                Console.WriteLine("");
                                Console.WriteLine("Response Body");
                                Console.WriteLine(wrapper.SerializeJson(responseBody));
                                Console.WriteLine("");
                            }
                            else
                            {
                                Console.WriteLine("Failed");
                                Console.WriteLine("");
                                Console.WriteLine("Response Body");
                                Console.WriteLine(wrapper.SerializeJson(responseBody));
                                Console.WriteLine("");
                            }
                            break;

                        case "refund":
                            string stripeRefundTxnID = "";

                            if (wrapper.Refund(
                                UserInputString("Charge Transaction ID", null, false),
                                out stripeRefundTxnID,
                                out responseBody))
                            {
                                Console.WriteLine("Success");
                                Console.WriteLine("  Refund Txn ID : " + stripeRefundTxnID);
                                Console.WriteLine("");
                                Console.WriteLine("Response Body");
                                Console.WriteLine(wrapper.SerializeJson(responseBody));
                                Console.WriteLine("");
                            }
                            else
                            {
                                Console.WriteLine("Failed");
                                Console.WriteLine("");
                                Console.WriteLine("Response Body");
                                Console.WriteLine(wrapper.SerializeJson(responseBody));
                                Console.WriteLine("");
                            }
                            break;

                        case "quit":
                        case "q":
                            runForever = false;
                            break;

                        case "cls":
                            Console.Clear();
                            break;

                        default:
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                ExceptionConsole("Main", "Outer exception", e);
            }
        }

        static string StackToString()
        {
            string ret = "";

            StackTrace t = new StackTrace();
            for (int i = 0; i < t.FrameCount; i++)
            {
                if (i == 0)
                {
                    ret += t.GetFrame(i).GetMethod().Name;
                }
                else
                {
                    ret += " <= " + t.GetFrame(i).GetMethod().Name;
                }
            }

            return ret;
        }

        static void ExceptionConsole(string method, string text, Exception e)
        {
            var st = new StackTrace(e, true);
            var frame = st.GetFrame(0);
            int line = frame.GetFileLineNumber();
            string filename = frame.GetFileName();

            Console.WriteLine("---");
            Console.WriteLine("An exception was encountered which triggered this message.");
            Console.WriteLine("  Method: " + method);
            Console.WriteLine("  Text: " + text);
            Console.WriteLine("  Type: " + e.GetType().ToString());
            Console.WriteLine("  Data: " + e.Data);
            Console.WriteLine("  Inner: " + e.InnerException);
            Console.WriteLine("  Message: " + e.Message);
            Console.WriteLine("  Source: " + e.Source);
            Console.WriteLine("  StackTrace: " + e.StackTrace);
            Console.WriteLine("  Stack: " + StackToString());
            Console.WriteLine("  Line: " + line);
            Console.WriteLine("  File: " + filename);
            Console.WriteLine("  ToString: " + e.ToString());
            Console.WriteLine("---");

            return;
        }

        static string UserInputString(string question, string defaultAnswer, bool allowNull)
        {
            while (true)
            {
                Console.Write(question);

                if (!String.IsNullOrEmpty(defaultAnswer))
                {
                    Console.Write(" [" + defaultAnswer + "]");
                }

                Console.Write(" ");

                string userInput = Console.ReadLine();

                if (String.IsNullOrEmpty(userInput))
                {
                    if (!String.IsNullOrEmpty(defaultAnswer)) return defaultAnswer;
                    if (allowNull) return null;
                    else continue;
                }

                return userInput;
            }
        }

        static int UserInputInt(string question, int defaultAnswer, bool positiveOnly, bool allowZero)
        {
            while (true)
            {
                Console.Write(question);
                Console.Write(" [" + defaultAnswer + "] ");

                string userInput = Console.ReadLine();

                if (String.IsNullOrEmpty(userInput))
                {
                    return defaultAnswer;
                }

                int ret = 0;
                if (!Int32.TryParse(userInput, out ret))
                {
                    Console.WriteLine("Please enter a valid integer.");
                    continue;
                }

                if (ret == 0)
                {
                    if (allowZero)
                    {
                        return 0;
                    }
                }

                if (ret < 0)
                {
                    if (positiveOnly)
                    {
                        Console.WriteLine("Please enter a value greater than zero.");
                        continue;
                    }
                }

                return ret;
            }
        }
    }
}
