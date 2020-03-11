using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Org.IdentityConnectors.Framework.Spi;
using Org.IdentityConnectors.Framework.Common.Exceptions;
using System.IO;

namespace Org.IdentityConnector.CustomExchangeConnector {
    public class CustomExchangeConfiguration : AbstractConfiguration {
        [ConfigurationProperty(Required = true, Order = 1)]
        public String DomainName { get; set; }

        [ConfigurationProperty(Required = true, Order = 2)]
        public String Container { get; set; }

        [ConfigurationProperty(Required = true, Order = 3)]
        public String ExchangeServerType { get; set; }

        [ConfigurationProperty(Required = true, Order = 4)]
        public String createScript { get; set; }

        public override void Validate() {
            Console.Write(this.createScript);
            string file = "";

            if (this.createScript == null || this.createScript.Length == 0) {
                throw new ConfigurationException("Configuration property FileName cannot be null or empty");
            }

            file = this.createScript + "create.ps1";
            if (!File.Exists(file)) {
                throw new ConfigurationException("Create file " + file + " does not exist");
            }

            file = this.createScript + "disable.ps1";
            if (!File.Exists(file)) {
                throw new ConfigurationException("Disable file " + file + " does not exist");
            }

            file = this.createScript + "enable.ps1";
            if (!File.Exists(file)) {
                throw new ConfigurationException("Enable file " + file + " does not exist");
            }
        }
    }
}
