using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using Ty.Core.Generic;

namespace Ty.Core.Data
{
    internal class DefaultDataProcessor : DataProcessor
    {
        protected override Dynamic Process()
        {
            Dynamic data = new Dynamic();

            do
            {
                var table = new Dynamic();
                var columns = new Dynamic();

                var isFirstRow = true;

                while (ReadLine())
                {
                    var row = new Dynamic();

                    for(int i = 0; i < FieldCount; i++)
                    {
                        var name = GetFieldName(i);
                        var val = GetDynamicValue(i);

                        row[name] = val;

                        if (isFirstRow)
                        {
                            columns.Add(new Dynamic()
                            {
                                ["Name"] = name,
                                ["Type"] = GetFieldType(i)
                            });
                        }
                    }

                    isFirstRow = false;
                    table.Add(row);
                }

                data["Tables"].Add(new Dynamic()
                {
                    ["Table"] = table,
                    ["Columns"] = columns
                });
            } while (ReadNextResult());

            data["Code"] = 0;
            return data;
        }

        protected override bool ReadParameter => true;
    }
}
