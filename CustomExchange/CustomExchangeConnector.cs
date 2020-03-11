using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Org.IdentityConnectors.Framework.Spi;
using System.IO;
using Org.IdentityConnectors.Framework.Common.Exceptions;
using System.Security.AccessControl;
using Org.IdentityConnectors.Framework.Spi.Operations;
using Org.IdentityConnectors.Framework.Common.Objects;
using Org.IdentityConnectors.Common;
using System.Management.Automation;
using System.Collections.ObjectModel;

namespace Org.IdentityConnector.CustomExchangeConnector {
    [ConnectorClass("CustomExchangeConnectorr_DisplayNameKey", typeof(CustomExchangeConfiguration))]
    public class CustomExchangeConnector : PoolableConnector, CreateOp, SchemaOp, TestOp, DeleteOp, UpdateOp, SearchOp<String> {
        private CustomExchangeConfiguration config;

        #region Init
        public void Init(Configuration config) {
            this.config = (CustomExchangeConfiguration)config;
            File.SetAttributes(this.config.createScript, FileAttributes.Normal);
        }
        #endregion

        #region CreateOp Members
        public Uid Create(ObjectClass objClass, ICollection<ConnectorAttribute> attrs, OperationOptions options) {
            StringBuilder sb = new StringBuilder();
            ConnectorAttribute NameAttribute = ConnectorAttributeUtil.Find(Name.NAME, attrs);
            
            if (NameAttribute == null) {
                throw new ConnectorException("The name operational attribute cannot be null");
            }

            string parameter = ConnectorAttributeUtil.GetAsStringValue(NameAttribute);
            string[] login = parameter.Split('@');

            PowerShell PowerShellInstance = PowerShell.Create();
            PowerShellInstance.AddCommand(this.config.createScript + "create.ps1");
            PowerShellInstance.AddArgument(login[0]);

            Collection<PSObject> results;
            Collection<ErrorRecord> errors;

            results = PowerShellInstance.Invoke();
            errors = PowerShellInstance.Streams.Error.ReadAll();

            if (errors.Count > 0) {
                foreach (ErrorRecord error in errors) {
                    sb.AppendLine(error.ToString());
                }

                throw new ConnectorException(sb.ToString());
            } else {
                return new Uid(ConnectorAttributeUtil.GetAsStringValue(NameAttribute));
            }
        }
        #endregion

        #region DeleteOp Members
        public void Delete(ObjectClass objClass, Uid uid, OperationOptions options) { }
        #endregion

        #region UpdateOp Members
        public Uid Update(ObjectClass objclass, Uid uid, ICollection<ConnectorAttribute> replaceAttributes, OperationOptions options) {
            StringBuilder sb = new StringBuilder();
            PowerShell PowerShellInstance = PowerShell.Create();

            ConnectorAttribute StatusAttribute = ConnectorAttributeUtil.Find(OperationalAttributes.ENABLE_NAME, replaceAttributes);
            String enable = ConnectorAttributeUtil.GetAsStringValue(StatusAttribute).ToLower();

            string parameter = ConnectorAttributeUtil.GetAsStringValue(uid);
            string[] login = parameter.Split('@');

            if (enable.Equals("false")) {
                PowerShellInstance.AddCommand(this.config.createScript + "disable.ps1");
                PowerShellInstance.AddArgument(login[0]);

                Collection<PSObject> results;
                Collection<ErrorRecord> errors;

                results = PowerShellInstance.Invoke();
                errors = PowerShellInstance.Streams.Error.ReadAll();

                if (errors.Count > 0) {
                    foreach (ErrorRecord error in errors) {
                        sb.AppendLine(error.ToString());
                    }

                    throw new ConnectorException(sb.ToString());
                } else {
                    return uid;
                }
                
            } else if (enable.Equals("true")) {
                PowerShellInstance.AddCommand(this.config.createScript + "enable.ps1");
                PowerShellInstance.AddArgument(login[0]);

                Collection<PSObject> results;
                Collection<ErrorRecord> errors;

                results = PowerShellInstance.Invoke();
                errors = PowerShellInstance.Streams.Error.ReadAll();

                if (errors.Count > 0) {
                    foreach (ErrorRecord error in errors) {
                        sb.AppendLine(error.ToString());
                    }

                    throw new ConnectorException(sb.ToString());
                } else {
                    return uid;
                }
            } else {
                return uid;
            }
        }
        #endregion

        #region SearchOp<string> Members
        public Org.IdentityConnectors.Framework.Common.Objects.Filters.FilterTranslator<string> CreateFilterTranslator(ObjectClass oclass, OperationOptions options) {
            return new CustomExchangeFilterTranslator();
        }
        #endregion

        #region SchemaOp Members
        public Schema Schema() {
            SchemaBuilder schemaBuilder = new SchemaBuilder(SafeType<Connector>.Get(this));
            ICollection<ConnectorAttributeInfo> connectorAttributeInfos = new List<ConnectorAttributeInfo>();
            connectorAttributeInfos.Add(ConnectorAttributeInfoBuilder.Build("User Logon Name"));
            schemaBuilder.DefineObjectClass(ObjectClass.ACCOUNT_NAME, connectorAttributeInfos);
            return schemaBuilder.Build();
        }
        #endregion

        #region TestOp Members
        public void Test() {
            Console.Write("Tested connection!");
        }
        #endregion

        public void ExecuteQuery(ObjectClass oclass, string query, ResultsHandler handler, OperationOptions options) { }

        #region CheckAlive
        public void CheckAlive() {
            string file = "";

            file = this.config.createScript + "create.ps1";
            if (!File.Exists(file)) {
                throw new ConnectorException("Create file " + file + " does not exist");
            }

            file = this.config.createScript + "disable.ps1";
            if (!File.Exists(file)) {
                throw new ConnectorException("Disable file " + file + " does not exist");
            }

            file = this.config.createScript + "enable.ps1";
            if (!File.Exists(file)) {
                throw new ConnectorException("Enable file " + file + " does not exist");
            }
        }
        #endregion

        #region Dispose
        public void Dispose() {
            //chill :)
        }

        private void SubmitConnectorObject(String result, ResultsHandler handler) {
            ConnectorObjectBuilder cob = new ConnectorObjectBuilder();
            String[] resultSplit = result.Split(new char[] { '$' });
            ICollection<ConnectorAttribute> attrs = new List<ConnectorAttribute>();

            foreach (String str in resultSplit) {
                ConnectorAttributeBuilder cab = new ConnectorAttributeBuilder();
                cab.AddValue(str.Split(new char[] { ':' })[1]);

                if (str.StartsWith("Name")) {
                    cob.SetName(Name.NAME);
                    cob.SetUid(str.Split(new char[] { ':' })[1]);
                    cab.Name = Name.NAME;
                } else {
                    cab.Name = str.Split(new char[] { ':' })[0];
                }

                attrs.Add(cab.Build());
            }

            cob.AddAttributes(attrs);
            handler(cob.Build());
        }
        
        #endregion
    }
}
