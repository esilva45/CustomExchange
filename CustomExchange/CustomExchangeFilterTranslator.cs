using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Org.IdentityConnectors.Framework.Common.Objects.Filters;
using Org.IdentityConnectors.Framework.Common.Objects;

namespace Org.IdentityConnector.CustomExchangeConnector {
    public class CustomExchangeFilterTranslator : AbstractFilterTranslator<String> {

        protected override string CreateEqualsExpression(EqualsFilter filter, bool not) {
            ConnectorAttribute attr = filter.GetAttribute();

            if (attr.Name.Equals(Name.NAME)) {
                return "Name:" + attr.Value.First().ToString();
            }

            return null;
        }
    }
}
