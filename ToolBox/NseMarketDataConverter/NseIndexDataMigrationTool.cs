﻿/*
 * QUANTCONNECT.COM - Democratizing Finance, Empowering Individuals.
 * Lean Algorithmic Trading Engine v2.0. Copyright 2014 QuantConnect Corporation.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using QuantConnect.Data.Market;
using QuantConnect.Util;

namespace QuantConnect.ToolBox.NseMarketDataConverter
{
    public static class NseIndexDataMigrationToolProgram
    {
        /// <summary>
        /// Supports data from http://tradingtuitions.com/intraday-1-minute-data-free-download/
        /// </summary>
        public static void NseIndexDataConverter(string sourceDirectory, string destinationDirectory)
        {
            //Document the process:
            Console.WriteLine("QuantConnect.ToolBox: NseMarketData Converter: ");
            Console.WriteLine("==============================================");
            Console.WriteLine("The NseMarketData converter transforms NseMarketData orders into the LEAN Algorithmic Trading Engine Data Format.");
            Console.WriteLine("Parameters required: --source-dir= --destination-dir= ");
            Console.WriteLine("   1> Source Directory of Unzipped NSE Data.");
            Console.WriteLine("   2> Destination Directory of LEAN Data Folder. (Typically located under Lean/Data)");
            Console.WriteLine(" ");
            Console.WriteLine("NOTE: THIS WILL OVERWRITE ANY EXISTING FILES.");
            if (sourceDirectory.IsNullOrEmpty() || destinationDirectory.IsNullOrEmpty())
            {
                Console.WriteLine("1. Source NSE source directory: ");
                sourceDirectory = (Console.ReadLine() ?? "");
                Console.WriteLine("2. Destination LEAN Data directory: ");
                destinationDirectory = (Console.ReadLine() ?? "");
            }

            //Validate the user input:
            Validate(sourceDirectory, destinationDirectory);

            //Remove the final slash to make the path building easier:
            sourceDirectory = StripFinalSlash(sourceDirectory);
            destinationDirectory = StripFinalSlash(destinationDirectory);

            //iterate over all the dates

            //Count the total files to process:
            Console.WriteLine("Counting Files..." + sourceDirectory);
            var count = 0;
            var totalCount = GetCount(sourceDirectory);
            Console.WriteLine("Processing {0} Files ...", totalCount);

            //Enumerate all files 

            foreach (var file in Directory.EnumerateFiles(sourceDirectory))
            {
                var symbol = GetSymbol(file);
                var fileContents = File.ReadAllText(file);
                string[] stringSeparators = new string[] { "\n" };
                string[] lines = fileContents.Split(stringSeparators, StringSplitOptions.None);
                var datawriter = new LeanDataWriter(Resolution.Minute, symbol, destinationDirectory);
                IList<TradeBar> fileEnum = new List<TradeBar>();
                foreach (string line in lines)
                {
                    string[] separators = new string[] { "," };
                    string[] linearray = line.Split(separators, StringSplitOptions.None);
                    if (linearray.Length > 2)
                    {
                        String newline = linearray[1] + " ";
                        newline += linearray[2];
                        newline += ":00.0000";
                        var Time = DateTime.ParseExact(newline, DateFormat.Forex, CultureInfo.InvariantCulture);
                        var open = Decimal.Parse(linearray[3]);
                        var high = Decimal.Parse(linearray[4]);
                        var low = Decimal.Parse(linearray[5]);
                        var close = Decimal.Parse(linearray[6]);
                        Int64 volume = 0;
                        if (linearray.Length > 7)
                        {
                            volume = Convert.ToInt64(linearray[7]);
                        }

                        var linedata = new TradeBar(Time, symbol, open, high, low, close, volume);
                        fileEnum.Add(linedata);
                    }
                }
                datawriter.Write(fileEnum);
                count++;
            }
            Console.ReadKey();
        }


        /// <summary>
        /// Application error: display error and then stop conversion
        /// </summary>
        /// <param name="error">Error string</param>
        private static void Error(string error)
        {
            Console.WriteLine(error);
            Console.ReadKey();
            Environment.Exit(0);
        }

        /// <summary>
        /// Get the count of the files to process
        /// </summary>
        /// <param name="sourceDirectory"></param>
        /// <returns></returns>
        private static int GetCount(string sourceDirectory)
        {
            var count = 0;
            foreach (var date in Directory.EnumerateFiles(sourceDirectory))
            {
                StripFinalSlash(date);
                count += count;
            }
            return count;
        }

        /// <summary>
        /// Remove the final slash to make path building easier
        /// </summary>
        private static string StripFinalSlash(string directory)
        {
            return directory.Trim('/', '\\');
        }

        /// <summary>
        /// Extract the symbol from the path
        /// </summary>
        private static Symbol GetSymbol(string filePath)
        {
            var splits = filePath.Split(' ');
            var file = splits[splits.Length - 1];
            file = file.Trim('.', '/', '\\').Replace(".txt", "");
            //This switch case is for the symbols names with spaces in their file names
            switch (file)
            {
                case "BANKNIFTY":
                    file = "BNF";
                    break;
                default:
                    break;
            }
            return Symbol.Create(file, SecurityType.Equity, Market.USA);
        }

        /// <summary>
        /// Validate the users input and throw error if not valid
        /// </summary>
        private static void Validate(string sourceDirectory, string destinationDirectory)
        {
            if (string.IsNullOrWhiteSpace(sourceDirectory))
            {
                Error("Error: Please enter a valid source directory.");
            }
            if (string.IsNullOrWhiteSpace(destinationDirectory))
            {
                Error("Error: Please enter a valid destination directory.");
            }
            if (!Directory.Exists(sourceDirectory))
            {
                Error("Error: Source directory does not exist.");
            }
            if (!Directory.Exists(destinationDirectory))
            {
                Error("Error: Destination directory does not exist.");
            }
        }
    }
}