﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;

using EPPlus.Core.Extensions.Validation;

using FluentAssertions;

using OfficeOpenXml;
using OfficeOpenXml.Style;
using OfficeOpenXml.Table;

using Xunit;

namespace EPPlus.Core.Extensions.Tests
{
    public class ExcelWorksheetExtensions_Tests : TestBase
    {
        [Fact]
        public void Should_add_headers()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets["TEST5"];

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            worksheet.AddHeader(x =>
            {
                x.Style.Fill.PatternType = ExcelFillStyle.Solid;
                x.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(170, 170, 170));
            }, "Barcode", "Quantity", "UpdatedDate");

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            worksheet.Dimension.End.Row.Should().Be(5);
        }

        [Fact]
        public void Should_add_objects_with_parameters()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets["TEST5"];

            var stocks = new List<StocksNullable>
            {
                new StocksNullable
                {
                    Barcode = "barcode123",
                    Quantity = 5,
                    UpdatedDate = DateTime.MaxValue
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            worksheet.AddObjects(stocks, 5, _ => _.Barcode, _ => _.Quantity, _ => _.UpdatedDate);
            IEnumerable<StocksNullable> list = worksheet.ToList<StocksNullable>(null, configuration =>
            {
                configuration.SkipCastingErrors = false;
                configuration.HasHeaderRow = true;
            });

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            list.Count().Should().Be(4);
        }

        [Fact]
        public void Should_add_objects_with_start_row_and_column_index_without_parameters()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets["TEST5"];

            var stocks = new List<StocksNullable>
            {
                new StocksNullable
                {
                    Barcode = "barcode123",
                    Quantity = 5,
                    UpdatedDate = DateTime.MaxValue
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            worksheet.AddObjects(stocks, 5, 3);
            IEnumerable<StocksNullable> list = worksheet.ToList<StocksNullable>(null, configuration =>
            {
                configuration.SkipCastingErrors = true;
                configuration.HasHeaderRow = true;
            });

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            list.Count().Should().Be(4);
        }

        [Fact]
        public void Should_AddLine_method_work()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets["TEST5"];

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            worksheet.AddLine(5, "barcode123", 5, DateTime.UtcNow);
            IEnumerable<StocksNullable> list = worksheet.ToList<StocksNullable>(null, configuration =>
            {
                configuration.SkipCastingErrors = false;
                configuration.HasHeaderRow = true;
            });

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            list.Count().Should().Be(4);
        }

        [Fact]
        public void Should_cannot_add_objects_with_null_property_selectors()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets["TEST5"];
            var stocks = new List<StocksNullable>
            {
                new StocksNullable
                {
                    Barcode = "barcode123",
                    Quantity = 5,
                    UpdatedDate = DateTime.MaxValue
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action action = () => { worksheet.AddObjects(stocks, 5, null); };

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            action.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void Should_change_background_color_of_the_worksheet()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets[2];

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            worksheet.SetBackgroundColor(Color.Brown, ExcelFillStyle.DarkTrellis);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            worksheet.Cells.Style.Fill.PatternType.Should().Be(ExcelFillStyle.DarkTrellis);
            worksheet.Cells.Style.Fill.BackgroundColor.Rgb.Should().Be(string.Format("{0:X8}", Color.Brown.ToArgb() & 0xFFFFFFFF));
        }

        [Fact]
        public void Should_change_font_color_of_the_worksheet()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets[3];

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            worksheet.SetFontColor(Color.BlueViolet);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            worksheet.Cells.Style.Font.Color.Rgb.Should().Be(string.Format("{0:X8}", Color.BlueViolet.ToArgb() & 0xFFFFFFFF));
        }

        [Fact]
        public void Should_change_font_of_the_worksheet()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets.First();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            worksheet.SetFont(new Font("Arial", 15));

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            worksheet.Cells.Style.Font.Name.Should().Be("Arial");
            worksheet.Cells.Style.Font.Size.Should().Be(15);
        }

        [Fact]
        public void Should_check_and_throw_exception_if_column_value_is_wrong_on_specified_index()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets["TEST6"];

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action action1 = () => { worksheet.CheckAndThrowColumn(1, 3, "Barcode", "Barcode column is missing"); };

            Action action2 = () => { worksheet.CheckAndThrowColumn(1, 1, "Barcode"); };

            Action action3 = () => { worksheet.CheckAndThrowColumn(2, 14, "Barcode"); };

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            action1.Should().Throw<ExcelTableValidationException>().And.Message.Should().Be("Barcode column is missing");
            action2.Should().NotThrow();
            action3.Should().Throw<ExcelTableValidationException>();
        }

        [Fact]
        public void Should_check_if_column_value_is_null_or_empty_on_given_index()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets["TEST6"];

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            bool result1 = worksheet.CheckColumnValueIsNullOrEmpty(3, 4);
            bool result2 = worksheet.CheckColumnValueIsNullOrEmpty(2, 1);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result1.Should().BeTrue();
            result2.Should().BeFalse();
        }

        [Fact]
        public void Should_convert_to_datatable_with_headers()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            DataTable dataTable;

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            dataTable = excelPackage.Workbook.Worksheets["TEST5"].ToDataTable();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            dataTable.Should().NotBeNull($"{nameof(dataTable)} should not be NULL");
            dataTable.Rows.Count.Should().Be(3, "We have 3 records");
        }

        [Fact]
        public void Should_convert_to_datatable_without_headers()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            DataTable dataTable;

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            dataTable = excelPackage.Workbook.Worksheets["TEST5"].ToDataTable(false);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            dataTable.Should().NotBeNull($"{nameof(dataTable)} should not be NULL");
            dataTable.Rows.Count.Should().Be(4, "We have 4 records");
        }

        [Fact]
        public void Should_convert_worksheet_to_list()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            ExcelWorksheet worksheet1 = excelPackage.Workbook.Worksheets["TEST4"];
            ExcelWorksheet worksheet2 = excelPackage.Workbook.Worksheets["TEST5"];

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            List<StocksNullable> list1 = worksheet1.ToList<StocksNullable>(null, configuration =>
            {
                configuration.SkipCastingErrors = true;
                configuration.HasHeaderRow = true;
            });
            List<StocksNullable> list2 = worksheet2.ToList<StocksNullable>(null, configuration =>
            {
                configuration.SkipCastingErrors = true;
                configuration.HasHeaderRow = false;
            });

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            list1.Count().Should().Be(4, "Should have four");
            list2.Count().Should().Be(4, "Should have four");
        }

        [Fact]
        public void Should_delete_a_column_by_using_header_text()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets["TEST6"];

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            worksheet.DeleteColumn("Quantity");

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            worksheet.GetValuedDimension().End.Column.Should().Be(2);
            worksheet.Cells[1, 2, 1, 2].Text.Should().Be("UpdatedDate");
        }

        [Fact]
        public void Should_delete_columns_by_given_header_text()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets["TEST6"];

            ExcelAddressBase valuedDimension = worksheet.GetValuedDimension();

            worksheet.ChangeCellValue(1, valuedDimension.End.Column + 1, "Quantity");
            worksheet.ChangeCellValue(1, valuedDimension.End.Column + 2, "Quantity");
            worksheet.ChangeCellValue(1, valuedDimension.End.Column + 3, "Quantity");

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            worksheet.DeleteColumns("Quantity");

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            worksheet.GetValuedDimension().End.Column.Should().Be(2);
            worksheet.Cells[1, 2, 1, 2].Text.Should().Be("UpdatedDate");
        }

        [Fact]
        public void Should_found_any_formula_on_worksheet()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            ExcelWorksheet worksheet1 = excelPackage.Workbook.Worksheets["TEST5"];
            worksheet1.Cells[18, 2, 18, 2].Formula = "=SUM(B2:B4)";

            ExcelWorksheet worksheet2 = excelPackage.Workbook.Worksheets["TEST4"];

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            bool result1 = worksheet1.HasAnyFormula();
            bool result2 = worksheet2.HasAnyFormula();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result1.Should().BeTrue();
            result2.Should().BeFalse();
        }

        [Fact]
        public void Should_get_as_Excel_table_with_headers()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            ExcelWorksheet excelWorksheet = excelPackage.Workbook.Worksheets["TEST5"];

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            ExcelTable excelTable = excelWorksheet.AsExcelTable();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            List<StocksNullable> listOfStocks = excelTable.ToList<StocksNullable>();
            listOfStocks.Count.Should().Be(3);
        }

        [Fact]
        public void Should_get_as_Excel_table_without_headers()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            ExcelWorksheet excelWorksheet = excelPackage.Workbook.Worksheets["TEST5"];

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            ExcelTable excelTable = excelWorksheet.AsExcelTable(false);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            List<StocksNullable> listOfStocks = excelTable.ToList<StocksNullable>(null, configuration => { configuration.SkipCastingErrors = true; });
            listOfStocks.Count.Should().Be(4);
        }

        [Fact]
        public void Should_get_columns_of_given_row_index()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets["TEST6"];

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            IEnumerable<KeyValuePair<int, string>> results = worksheet.GetColumns(1).ToList();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            results.Count().Should().Be(3);
            results.First().Value.Should().Be("Barcode");
        }

        [Fact]
        public void Should_get_data_bounds_of_worksheet_with_headers()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            ExcelWorksheet excelWorksheet = excelPackage.Workbook.Worksheets["TEST5"];

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            ExcelAddress dataBounds = excelWorksheet.GetDataBounds();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            dataBounds.Rows.Should().Be(3);
            dataBounds.Columns.Should().Be(3);
        }

        [Fact]
        public void Should_get_data_bounds_of_worksheet_without_headers()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            ExcelWorksheet excelWorksheet = excelPackage.Workbook.Worksheets["TEST5"];

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            ExcelAddress dataBounds = excelWorksheet.GetDataBounds(false);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            dataBounds.Rows.Should().Be(4);
            dataBounds.Columns.Should().Be(3);
        }

        [Fact]
        public void Should_get_worksheet_as_enumerable()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            ExcelWorksheet worksheet1 = excelPackage.Workbook.Worksheets["TEST4"];
            ExcelWorksheet worksheet2 = excelPackage.Workbook.Worksheets["TEST5"];

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            IEnumerable<StocksNullable> list1 = worksheet1.AsEnumerable<StocksNullable>(null, configuration =>
            {
                configuration.SkipCastingErrors = true;
                configuration.HasHeaderRow = true;
            });
            IEnumerable<StocksNullable> list2 = worksheet2.AsEnumerable<StocksNullable>(null, configuration =>
            {
                configuration.SkipCastingErrors = true;
                configuration.HasHeaderRow = false;
            });

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            list1.Count().Should().Be(4, "Should have four");
            list2.Count().Should().Be(4, "Should have four");
        }

        [Fact]
        public void Should_parse_datetime_value_as_correctly_if_formatted_customly()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets["TEST6"];

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            List<StocksNullable> nullableStocks = worksheet.ToList<StocksNullable>();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            nullableStocks[0].UpdatedDate.HasValue.Should().Be(true);
            nullableStocks[0].UpdatedDate.Value.Date.Should().Be(new DateTime(2017, 08, 08));

            nullableStocks[1].UpdatedDate.HasValue.Should().Be(true);
            nullableStocks[1].UpdatedDate.Value.Should().Be(new DateTime(2016, 11, 03, 01, 30, 53));
        }

        [Fact]
        public void Should_set_horizontal_alignment_of_the_worksheet()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets[4];

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            worksheet.SetHorizontalAlignment(ExcelHorizontalAlignment.Distributed);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            worksheet.Cells.Style.HorizontalAlignment.Should().Be(ExcelHorizontalAlignment.Distributed);
        }

        [Fact]
        public void Should_set_vertical_alignment_of_the_worksheet()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets.Last();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            worksheet.SetVerticalAlignment(ExcelVerticalAlignment.Justify);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            worksheet.Cells.Style.VerticalAlignment.Should().Be(ExcelVerticalAlignment.Justify);
        }

        [Fact]
        public void Should_throw_an_exception_when_columns_of_worksheet_not_matched_with_ExcelTableAttribute()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets["TEST6"];

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action action1 = () => { worksheet.CheckHeadersAndThrow<NamedMap>(1, "The {0}.column of worksheet should be '{1}'."); };
            Action action2 = () => { worksheet.CheckHeadersAndThrow<NamedMap>(1); };
            Action action3 = () => { worksheet.CheckHeadersAndThrow<StocksNullable>(1); };
            Action action4 = () => { worksheet.CheckHeadersAndThrow<Car>(1); };

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            action1.Should().Throw<ExcelTableValidationException>().And.Message.Should().Be("The 1.column of worksheet should be 'Name'.");
            action2.Should().Throw<ExcelTableValidationException>().And.Message.Should().Be("The 1. column of worksheet should be 'Name'.");
            action3.Should().NotThrow<ExcelTableValidationException>();
            action4.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void Should_throw_exception_when_occured_casting_error()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets["TEST5"];
            List<StocksValidation> list;

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action action = () =>
            {
                list = worksheet.ToList<StocksValidation>(null, configuration =>
                {
                    configuration.SkipCastingErrors = false;
                    configuration.HasHeaderRow = true;
                });
            };

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            action.Should().Throw<ExcelTableValidationException>();
        }

        [Fact]
        public void Should_valued_dimension_be_A1C4()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets["TEST6"];

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            ExcelAddressBase valuedDimension = worksheet.GetValuedDimension();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            valuedDimension.Address.Should().Be("A1:C4");
            valuedDimension.Start.Column.Should().Be(1);
            valuedDimension.Start.Row.Should().Be(1);
            valuedDimension.End.Column.Should().Be(3);
            valuedDimension.End.Row.Should().Be(4);
        }

        [Fact]
        public void Should_valued_dimension_be_E9G13()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets["TEST4"];

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            ExcelAddressBase valuedDimension = worksheet.GetValuedDimension();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            valuedDimension.Address.Should().Be("E9:G13");
            valuedDimension.Start.Column.Should().Be(5);
            valuedDimension.Start.Row.Should().Be(9);
            valuedDimension.End.Column.Should().Be(7);
            valuedDimension.End.Row.Should().Be(13);
        }
    }
}
