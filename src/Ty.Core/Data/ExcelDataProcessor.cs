using System;
using System.Collections.Generic;
using System.Text;
using Ty.Core.Generic;

namespace Ty.Core.Data
{
    internal class ExcelDataProcessor : DataProcessor
    {
        private IServiceProvider _provider;
        public ExcelDataProcessor(IServiceProvider provider)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        protected override Dynamic Process()
        {
            Dynamic data = new Dynamic() { ["Code"] = 0, ["Data"] = null };

            try
            {
                ExcelPackage p = _provider.GetService(typeof(ExcelPackage)) as ExcelPackage;

                int tableIndex = 0;

                do
                {
                    IExcelSheet sheet = p.AddSheet("Sheet" + (++tableIndex));

                    int rowIndex = 0;
                    int rStart = 2, cStart = 1;

                    while (ReadLine())
                    {
                        for (int colIndex = 0; colIndex < FieldCount; colIndex++)
                        {
                            IExcelRange range = sheet.Range(rStart + rowIndex, cStart + colIndex);
                            range.Value = GetFieldValue(colIndex);
                        }

                        rowIndex++;
                    }
                    
                    rStart = 1;
                    for (int colIndex = 0; colIndex < FieldCount; colIndex++)
                    {
                        IExcelRange range = sheet.Range(rStart, colIndex + cStart);
                        range.Value = GetFieldName(colIndex);

                        IExcelColumn column = sheet.Column(colIndex + cStart);
                        column.SetFormat(GetFieldType(colIndex));
                        column.AutoFit();
                    }

                } while (ReadNextResult());

                data["Data"] = p.GetBytes();
                p.Dispose();
            }
            catch (Exception)
            {
                data["Code"] = 1;
            }
            return data;
        }

        protected override bool ReadParameter => false;

    }
}
