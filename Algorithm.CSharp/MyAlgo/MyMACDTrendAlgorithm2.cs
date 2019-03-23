/*
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
using QuantConnect.Data.Market;
using QuantConnect.Indicators;
using QuantConnect.Interfaces;

namespace QuantConnect.Algorithm.CSharp.MyAlgo
{
    /// <summary>
    /// Simple indicator demonstration algorithm of MACD
    /// </summary>
    /// <meta name="tag" content="indicators" />
    /// <meta name="tag" content="indicator classes" />
    /// <meta name="tag" content="plotting indicators" />
    public class MyMACDTrendAlgorithm2 : QCAlgorithm, IRegressionAlgorithmDefinition
    {
        private DateTime _previous;
        private MovingAverageConvergenceDivergence _macd;
        private readonly string _symbol = "nifty";

        decimal oldMACD = 0;
        bool first = true;
        bool goingLong = false;
        /// <summary>
        /// Initialise the data and resolution required, as well as the cash and start-end dates for your algorithm. All algorithms must initialized.
        /// </summary>
        public override void Initialize()
        {
            SetStartDate(2019, 02, 26);
            SetEndDate(2019, 02,28 );
            SetCash(100000);
            AddSecurity(SecurityType.Equity,_symbol);

            // define our daily macd(12,26) with a 9 day signal
            _macd = MACD(_symbol, 12, 26, 14, MovingAverageType.Exponential, Resolution.Minute);
        }

        /// <summary>
        /// OnData event is the primary entry point for your algorithm. Each new data point will be pumped in here.
        /// </summary>
        /// <param name="data">TradeBars IDictionary object with your stock data</param>
        public void OnData(TradeBars data)
        {

            // "TradeBars" object holds many "TradeBar" objects: it is a dictionary indexed by the symbol:
            // 
            //  e.g.  data["MSFT"] data["GOOG"]
            //(!Portfolio.HoldStock)
            if (oldMACD == 0)
            {
                oldMACD = _macd.Signal;
                return;
            }
            if (first)
            {
                if (_macd.Signal >= oldMACD)
                {
                    goingLong = true;
                    first = false;
                    goLong(data);
                    return;
                }
                else if (_macd.Signal < oldMACD)
                {
                    goingLong = false;
                    first = false;
                    goShort(data);
                    return;
                }
            }
            else if ((_macd.Signal >= oldMACD) & goingLong == false)
            {
                goingLong = true;
                first = false;
                goLong(data);
                return;
            }
            else if ((_macd.Signal < oldMACD) & goingLong == true)
            {
                goingLong = false;
                first = false;
                goShort(data);
                return;
            }
            return;

        }

        public void goLong(TradeBars data)
        {
            Liquidate();
            do
            {
                // nothing
            } while (Portfolio.HoldStock);

            MarketOrder(_symbol, (int)Math.Floor(Portfolio.Cash / data[_symbol].Close));
        }

        public void goShort(TradeBars data)
        {
            Liquidate();
            do
            {
                // nothing
            } while (Portfolio.HoldStock);

            MarketOrder(_symbol, -1 * (int)Math.Floor(Portfolio.Cash / data[_symbol].Close));
        }


        public override void OnEndOfDay()
        {
            //Log the end of day prices:
            Liquidate(_symbol);
        }

        /// <summary>
        /// This is used by the regression test system to indicate if the open source Lean repository has the required data to run this algorithm.
        /// </summary>
        public bool CanRunLocally { get; } = true;

        /// <summary>
        /// This is used by the regression test system to indicate which languages this algorithm is written in.
        /// </summary>
        public Language[] Languages { get; } = { Language.CSharp, Language.Python };

        /// <summary>
        /// This is used by the regression test system to indicate what the expected statistics are from running the algorithm
        /// </summary>
        public Dictionary<string, string> ExpectedStatistics => new Dictionary<string, string>
        {
            {"Total Trades", "84"},
            {"Average Win", "4.78%"},
            {"Average Loss", "-4.16%"},
            {"Compounding Annual Return", "2.958%"},
            {"Drawdown", "34.800%"},
            {"Expectancy", "0.228"},
            {"Net Profit", "37.837%"},
            {"Sharpe Ratio", "0.297"},
            {"Loss Rate", "43%"},
            {"Win Rate", "57%"},
            {"Profit-Loss Ratio", "1.15"},
            {"Alpha", "0.107"},
            {"Beta", "-3.51"},
            {"Annual Standard Deviation", "0.124"},
            {"Annual Variance", "0.015"},
            {"Information Ratio", "0.136"},
            {"Tracking Error", "0.125"},
            {"Treynor Ratio", "-0.011"},
            {"Total Fees", "$443.50"}
        };
    }
}
