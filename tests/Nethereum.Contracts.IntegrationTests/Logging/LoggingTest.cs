﻿using System;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using Microsoft.Extensions.Logging;
using Moq;
using Nethereum.Hex.HexTypes;
using Nethereum.JsonRpc.Client;
using Nethereum.XUnitEthereumClients;
using Xunit;
using Xunit.Abstractions;
// ReSharper disable ConsiderUsingConfigureAwait  
// ReSharper disable AsyncConverter.ConfigureAwaitHighlighting

namespace Nethereum.Contracts.IntegrationTests.Logging
{
    [Collection(EthereumClientIntegrationFixture.ETHEREUM_CLIENT_COLLECTION_DEFAULT)]
    public class LoggingTest
    {
        private readonly EthereumClientIntegrationFixture _ethereumClientIntegrationFixture;

        public LoggingTest(EthereumClientIntegrationFixture ethereumClientIntegrationFixture)
        {
            _ethereumClientIntegrationFixture = ethereumClientIntegrationFixture;
     
        }

        [Fact]
        public async void TestLogging()
        {
            if (_ethereumClientIntegrationFixture.EthereumClient == EthereumClient.Geth)
            {
                var loggerMock = new Mock<ILogger<RpcClient>>();
               

                var contractByteCode =
                    "0x6060604052604060405190810160405280600a81526020017f4d756c7469706c6965720000000000000000000000000000000000000000000081526020015060016000509080519060200190828054600181600116156101000203166002900490600052602060002090601f016020900481019282601f1061008c57805160ff19168380011785556100bd565b828001600101855582156100bd579182015b828111156100bc57825182600050559160200191906001019061009e565b5b5090506100e891906100ca565b808211156100e457600081815060009055506001016100ca565b5090565b5050604051602080610303833981016040528080519060200190919050505b806000600050819055505b506101e2806101216000396000f360606040526000357c01000000000000000000000000000000000000000000000000000000009004806340490a901461004f57806375d0c0dc14610072578063c6888fa1146100ed5761004d565b005b61005c6004805050610119565b6040518082815260200191505060405180910390f35b61007f6004805050610141565b60405180806020018281038252838181518152602001915080519060200190808383829060006004602084601f0104600f02600301f150905090810190601f1680156100df5780820380516001836020036101000a031916815260200191505b509250505060405180910390f35b610103600480803590602001909190505061012b565b6040518082815260200191505060405180910390f35b60006000600050549050610128565b90565b60006000600050548202905061013c565b919050565b60016000508054600181600116156101000203166002900480601f0160208091040260200160405190810160405280929190818152602001828054600181600116156101000203166002900480156101da5780601f106101af576101008083540402835291602001916101da565b820191906000526020600020905b8154815290600101906020018083116101bd57829003601f168201915b50505050508156";

                var abi =
                    @"[{""constant"":true,""inputs"":[],""name"":""getMultiplier"",""outputs"":[{""name"":""d"",""type"":""uint256""}],""type"":""function""},{""constant"":true,""inputs"":[],""name"":""contractName"",""outputs"":[{""name"":"""",""type"":""string""}],""type"":""function""},{""constant"":false,""inputs"":[{""name"":""a"",""type"":""uint256""}],""name"":""multiply"",""outputs"":[{""name"":""d"",""type"":""uint256""}],""type"":""function""},{""inputs"":[{""name"":""multiplier"",""type"":""uint256""}],""type"":""constructor""}]";

                var senderAddress = EthereumClientIntegrationFixture.AccountAddress;
                var web3 = new Web3.Web3(_ethereumClientIntegrationFixture.GetWeb3().TransactionManager.Account,
                    client: new RpcClient(
                        new Uri("http://localhost:8545"), authHeaderValue: null, null, null,
                        loggerMock.Object));
                web3.TransactionManager.UseLegacyAsDefault = true;
               
                BigInteger nonce = 0;

                try
                {
                    var transactionHash2 = await web3.Eth.DeployContract.SendRequestAsync(abi, contractByteCode,
                        senderAddress, // lower gas
                        new HexBigInteger(900), 7).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                }

                //Parity is:
                //RPC Response Error: Transaction gas is too low. 
                
                loggerMock.VerifyLog(logger => logger.LogTrace("*eth_getTransactionCount*"));
                loggerMock.VerifyLog(logger => logger.LogTrace("*eth_gasPrice*"));
                loggerMock.VerifyLog(logger => logger.LogTrace("*eth_sendRawTransaction*")); 
                loggerMock.VerifyLog(logger => logger.LogError("RPC Response Error: intrinsic gas too low: gas 900, minimum needed 64412"));

            }
        }
    }
}