using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security;
using System.Text;
using System.Web;

namespace SamsLiquorShop
{
    // Kind of value in an export column: decides number formatting in Excel and alignment in the PDF
    public enum ExportKind { Text, Integer, Money, Date, DateTime }

    public class ExportColumn
    {
        public string Header { get; set; }
        public ExportKind Kind { get; set; }

        public ExportColumn(string header, ExportKind kind = ExportKind.Text)
        {
            Header = header;
            Kind = kind;
        }

        public bool IsNumeric => Kind == ExportKind.Integer || Kind == ExportKind.Money;
    }

    // A table to export: a title, an optional subtitle line, columns and rows of raw values
    // (strings, numbers, DateTimes or DBNull). Built once and written as PDF or Excel.
    public class ExportTable
    {
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public List<ExportColumn> Columns { get; } = new List<ExportColumn>();
        public List<object[]> Rows { get; } = new List<object[]>();

        public ExportTable(string title, string subtitle = null)
        {
            Title = title;
            Subtitle = subtitle;
        }

        public ExportTable Col(string header, ExportKind kind = ExportKind.Text)
        {
            Columns.Add(new ExportColumn(header, kind));
            return this;
        }

        public void Add(params object[] values)
        {
            Rows.Add(values);
        }
    }

    // Writes an ExportTable to the response as a real .pdf or .xlsx file.
    // No third-party libraries: the PDF is written by hand (standard Helvetica font) and the
    // .xlsx is a zipped Office Open XML package built with System.IO.Compression.
    public static class ExportHelper
    {
        private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

        // "pdf" or "excel" (from the export buttons' CommandArgument)
        public static void Send(HttpResponse response, ExportTable table, string fileBaseName, string format)
        {
            string stamp = DateTime.Today.ToString("yyyyMMdd");
            if (string.Equals(format, "pdf", StringComparison.OrdinalIgnoreCase))
            {
                Write(response, BuildPdf(table), "application/pdf", fileBaseName + "-" + stamp + ".pdf");
            }
            else
            {
                Write(response, BuildXlsx(table), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileBaseName + "-" + stamp + ".xlsx");
            }
        }

        private static void Write(HttpResponse response, byte[] bytes, string contentType, string fileName)
        {
            response.Clear();
            response.ContentType = contentType;
            response.AddHeader("Content-Disposition", "attachment; filename=" + fileName);
            response.AddHeader("Content-Length", bytes.Length.ToString(Inv));
            response.BinaryWrite(bytes);
            response.End();
        }

        // Display text for a value, used in the PDF
        private static string Display(object value, ExportKind kind)
        {
            if (value == null || value == DBNull.Value) return "";
            switch (kind)
            {
                case ExportKind.Money:
                    return "R " + Convert.ToDecimal(value).ToString("#,##0.00", Inv);
                case ExportKind.Integer:
                    return Convert.ToDecimal(value).ToString("#,##0", Inv);
                case ExportKind.Date:
                    return value is DateTime ? ((DateTime)value).ToString("yyyy-MM-dd", Inv) : value.ToString();
                case ExportKind.DateTime:
                    return value is DateTime ? ((DateTime)value).ToString("yyyy-MM-dd HH:mm", Inv) : value.ToString();
                default:
                    return value.ToString();
            }
        }

        // =====================================================================
        // EXCEL (.xlsx)
        // =====================================================================
        private static byte[] BuildXlsx(ExportTable t)
        {
            using (var ms = new MemoryStream())
            {
                using (var zip = new ZipArchive(ms, ZipArchiveMode.Create, true))
                {
                    AddEntry(zip, "[Content_Types].xml",
                        "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>" +
                        "<Types xmlns=\"http://schemas.openxmlformats.org/package/2006/content-types\">" +
                        "<Default Extension=\"rels\" ContentType=\"application/vnd.openxmlformats-package.relationships+xml\"/>" +
                        "<Default Extension=\"xml\" ContentType=\"application/xml\"/>" +
                        "<Override PartName=\"/xl/workbook.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml\"/>" +
                        "<Override PartName=\"/xl/worksheets/sheet1.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml\"/>" +
                        "<Override PartName=\"/xl/styles.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.styles+xml\"/>" +
                        "</Types>");

                    AddEntry(zip, "_rels/.rels",
                        "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>" +
                        "<Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\">" +
                        "<Relationship Id=\"rId1\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument\" Target=\"xl/workbook.xml\"/>" +
                        "</Relationships>");

                    string sheetName = SheetName(t.Title);
                    AddEntry(zip, "xl/workbook.xml",
                        "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>" +
                        "<workbook xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\" xmlns:r=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships\">" +
                        "<sheets><sheet name=\"" + Xml(sheetName) + "\" sheetId=\"1\" r:id=\"rId1\"/></sheets>" +
                        "<definedNames><definedName name=\"_xlnm._FilterDatabase\" localSheetId=\"0\" hidden=\"1\">" +
                        Xml("'" + sheetName.Replace("'", "''") + "'!" + FilterRef(t)) + "</definedName></definedNames>" +
                        "</workbook>");

                    AddEntry(zip, "xl/_rels/workbook.xml.rels",
                        "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>" +
                        "<Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\">" +
                        "<Relationship Id=\"rId1\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet\" Target=\"worksheets/sheet1.xml\"/>" +
                        "<Relationship Id=\"rId2\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles\" Target=\"styles.xml\"/>" +
                        "</Relationships>");

                    // Style indexes: 0 normal, 1 header, 2 money, 3 date-time, 4 date, 5 title, 6 subtitle, 7 integer
                    AddEntry(zip, "xl/styles.xml",
                        "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>" +
                        "<styleSheet xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\">" +
                        "<numFmts count=\"3\">" +
                        "<numFmt numFmtId=\"164\" formatCode=\"&quot;R&quot;\\ #,##0.00\"/>" +
                        "<numFmt numFmtId=\"165\" formatCode=\"yyyy\\-mm\\-dd\\ hh:mm\"/>" +
                        "<numFmt numFmtId=\"166\" formatCode=\"yyyy\\-mm\\-dd\"/>" +
                        "</numFmts>" +
                        "<fonts count=\"4\">" +
                        "<font><sz val=\"11\"/><name val=\"Calibri\"/></font>" +
                        "<font><b/><sz val=\"11\"/><color rgb=\"FFFFFFFF\"/><name val=\"Calibri\"/></font>" +
                        "<font><b/><sz val=\"14\"/><name val=\"Calibri\"/></font>" +
                        "<font><i/><sz val=\"10\"/><color rgb=\"FF666666\"/><name val=\"Calibri\"/></font>" +
                        "</fonts>" +
                        "<fills count=\"3\">" +
                        "<fill><patternFill patternType=\"none\"/></fill>" +
                        "<fill><patternFill patternType=\"gray125\"/></fill>" +
                        "<fill><patternFill patternType=\"solid\"><fgColor rgb=\"FF1F2430\"/><bgColor indexed=\"64\"/></patternFill></fill>" +
                        "</fills>" +
                        "<borders count=\"1\"><border><left/><right/><top/><bottom/><diagonal/></border></borders>" +
                        "<cellStyleXfs count=\"1\"><xf numFmtId=\"0\" fontId=\"0\" fillId=\"0\" borderId=\"0\"/></cellStyleXfs>" +
                        "<cellXfs count=\"8\">" +
                        "<xf numFmtId=\"0\" fontId=\"0\" fillId=\"0\" borderId=\"0\" xfId=\"0\"/>" +
                        "<xf numFmtId=\"0\" fontId=\"1\" fillId=\"2\" borderId=\"0\" xfId=\"0\" applyFont=\"1\" applyFill=\"1\"/>" +
                        "<xf numFmtId=\"164\" fontId=\"0\" fillId=\"0\" borderId=\"0\" xfId=\"0\" applyNumberFormat=\"1\"/>" +
                        "<xf numFmtId=\"165\" fontId=\"0\" fillId=\"0\" borderId=\"0\" xfId=\"0\" applyNumberFormat=\"1\"><alignment horizontal=\"left\"/></xf>" +
                        "<xf numFmtId=\"166\" fontId=\"0\" fillId=\"0\" borderId=\"0\" xfId=\"0\" applyNumberFormat=\"1\"><alignment horizontal=\"left\"/></xf>" +
                        "<xf numFmtId=\"0\" fontId=\"2\" fillId=\"0\" borderId=\"0\" xfId=\"0\" applyFont=\"1\"/>" +
                        "<xf numFmtId=\"0\" fontId=\"3\" fillId=\"0\" borderId=\"0\" xfId=\"0\" applyFont=\"1\"/>" +
                        "<xf numFmtId=\"3\" fontId=\"0\" fillId=\"0\" borderId=\"0\" xfId=\"0\" applyNumberFormat=\"1\"/>" +
                        "</cellXfs>" +
                        "<cellStyles count=\"1\"><cellStyle name=\"Normal\" xfId=\"0\" builtinId=\"0\"/></cellStyles>" +
                        "</styleSheet>");

                    AddEntry(zip, "xl/worksheets/sheet1.xml", BuildSheetXml(t));
                }
                return ms.ToArray();
            }
        }

        // Title in row 1, subtitle in row 2, headers in row 4, data from row 5
        private const int HeaderRow = 4;

        private static string FilterRef(ExportTable t)
        {
            int lastRow = HeaderRow + Math.Max(t.Rows.Count, 1);
            string lastCol = ColName(t.Columns.Count - 1);
            return "$A$" + HeaderRow + ":$" + lastCol + "$" + lastRow;
        }

        private static string BuildSheetXml(ExportTable t)
        {
            var sb = new StringBuilder();
            sb.Append("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>");
            sb.Append("<worksheet xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\" xmlns:r=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships\">");

            // Freeze everything above the first data row
            sb.Append("<sheetViews><sheetView workbookViewId=\"0\"><pane ySplit=\"" + HeaderRow + "\" topLeftCell=\"A" + (HeaderRow + 1) +
                      "\" activePane=\"bottomLeft\" state=\"frozen\"/></sheetView></sheetViews>");

            // Column widths from the longest value in each column
            sb.Append("<cols>");
            for (int c = 0; c < t.Columns.Count; c++)
            {
                ExportColumn col = t.Columns[c];
                int len = col.Header.Length;
                foreach (object[] row in t.Rows)
                {
                    if (c < row.Length) len = Math.Max(len, Display(row[c], col.Kind).Length);
                }
                double width = Math.Min(60, Math.Max(8, len + 3));
                sb.Append("<col min=\"" + (c + 1) + "\" max=\"" + (c + 1) + "\" width=\"" + width.ToString(Inv) + "\" customWidth=\"1\"/>");
            }
            sb.Append("</cols>");

            sb.Append("<sheetData>");

            sb.Append("<row r=\"1\">");
            InlineString(sb, "A1", t.Title, 5);
            sb.Append("</row>");
            if (!string.IsNullOrEmpty(t.Subtitle))
            {
                sb.Append("<row r=\"2\">");
                InlineString(sb, "A2", t.Subtitle, 6);
                sb.Append("</row>");
            }

            sb.Append("<row r=\"" + HeaderRow + "\">");
            for (int c = 0; c < t.Columns.Count; c++)
            {
                InlineString(sb, ColName(c) + HeaderRow, t.Columns[c].Header, 1);
            }
            sb.Append("</row>");

            int r = HeaderRow + 1;
            foreach (object[] row in t.Rows)
            {
                sb.Append("<row r=\"" + r + "\">");
                for (int c = 0; c < t.Columns.Count; c++)
                {
                    object v = c < row.Length ? row[c] : null;
                    if (v == null || v == DBNull.Value) continue;
                    string cellRef = ColName(c) + r;
                    ExportKind kind = t.Columns[c].Kind;

                    if ((kind == ExportKind.Money || kind == ExportKind.Integer) && IsNumber(v))
                    {
                        sb.Append("<c r=\"" + cellRef + "\" s=\"" + (kind == ExportKind.Money ? 2 : 7) + "\"><v>" +
                                  Convert.ToDecimal(v).ToString(Inv) + "</v></c>");
                    }
                    else if ((kind == ExportKind.Date || kind == ExportKind.DateTime) && v is DateTime)
                    {
                        sb.Append("<c r=\"" + cellRef + "\" s=\"" + (kind == ExportKind.Date ? 4 : 3) + "\"><v>" +
                                  ((DateTime)v).ToOADate().ToString(Inv) + "</v></c>");
                    }
                    else
                    {
                        InlineString(sb, cellRef, Display(v, kind), 0);
                    }
                }
                sb.Append("</row>");
                r++;
            }

            sb.Append("</sheetData>");
            sb.Append("<autoFilter ref=\"" + FilterRef(t).Replace("$", "") + "\"/>");
            sb.Append("<pageMargins left=\"0.5\" right=\"0.5\" top=\"0.75\" bottom=\"0.75\" header=\"0.3\" footer=\"0.3\"/>");
            sb.Append("<pageSetup orientation=\"landscape\" paperSize=\"9\"/>");
            sb.Append("</worksheet>");
            return sb.ToString();
        }

        private static void InlineString(StringBuilder sb, string cellRef, string text, int style)
        {
            sb.Append("<c r=\"" + cellRef + "\" t=\"inlineStr\"" + (style > 0 ? " s=\"" + style + "\"" : "") +
                      "><is><t xml:space=\"preserve\">" + Xml(text) + "</t></is></c>");
        }

        private static bool IsNumber(object v)
        {
            return v is int || v is long || v is short || v is decimal || v is double || v is float || v is byte;
        }

        // 0 -> A, 25 -> Z, 26 -> AA
        private static string ColName(int index)
        {
            string name = "";
            index++;
            while (index > 0)
            {
                int m = (index - 1) % 26;
                name = (char)('A' + m) + name;
                index = (index - m - 1) / 26;
            }
            return name;
        }

        // Excel sheet names: max 31 chars, none of : \ / ? * [ ]
        private static string SheetName(string title)
        {
            string s = new string((title ?? "Export").Where(ch => ":\\/?*[]".IndexOf(ch) < 0).ToArray()).Trim();
            if (s.Length == 0) s = "Export";
            return s.Length > 31 ? s.Substring(0, 31) : s;
        }

        // XML-escape and drop control characters that are not allowed in XML
        private static string Xml(string s)
        {
            if (s == null) return "";
            var clean = new StringBuilder(s.Length);
            foreach (char ch in s)
            {
                if (ch == '\t' || ch == '\n' || ch == '\r' || ch >= 0x20) clean.Append(ch);
            }
            return SecurityElement.Escape(clean.ToString());
        }

        private static void AddEntry(ZipArchive zip, string name, string content)
        {
            ZipArchiveEntry entry = zip.CreateEntry(name, CompressionLevel.Optimal);
            using (var w = new StreamWriter(entry.Open(), new UTF8Encoding(false)))
            {
                w.Write(content);
            }
        }

        // =====================================================================
        // PDF
        // =====================================================================

        // A4 landscape, in points
        private const float PageW = 842f, PageH = 595f, Margin = 36f;
        private const float FontSize = 8.5f, HeaderFontSize = 8.5f, RowH = 17f;
        private const float CellPad = 5f;

        // Helvetica glyph widths (1/1000 em) for characters 32..126
        private static readonly int[] HelveticaWidths =
        {
            278,278,355,556,556,889,667,191,333,333,389,584,278,333,278,278,
            556,556,556,556,556,556,556,556,556,556,278,278,584,584,584,556,
            1015,667,667,722,722,667,611,778,722,278,500,667,556,833,722,778,
            667,778,722,667,611,722,667,944,667,667,611,278,278,278,469,556,
            333,556,556,500,556,556,278,556,556,222,222,500,222,833,556,556,
            556,556,333,500,278,556,500,722,500,500,500,334,260,334,584
        };

        private static float TextWidth(string s, float size, bool bold)
        {
            float w = 0;
            foreach (char ch in s)
            {
                int code = ch;
                w += (code >= 32 && code <= 126) ? HelveticaWidths[code - 32] : 556;
            }
            return w / 1000f * size * (bold ? 1.08f : 1f);
        }

        private static string Fit(string s, float maxWidth, float size, bool bold)
        {
            if (TextWidth(s, size, bold) <= maxWidth) return s;
            const string ell = "...";
            while (s.Length > 0 && TextWidth(s + ell, size, bold) > maxWidth) s = s.Substring(0, s.Length - 1);
            return s.TrimEnd() + ell;
        }

        private static byte[] BuildPdf(ExportTable t)
        {
            int colCount = t.Columns.Count;
            var cells = t.Rows.Select(row => Enumerable.Range(0, colCount)
                .Select(c => Display(c < row.Length ? row[c] : null, t.Columns[c].Kind)).ToArray()).ToList();

            // Natural column widths (capped so one long column can't squeeze the rest), then scaled to the page
            float usable = PageW - 2 * Margin;
            var widths = new float[colCount];
            for (int c = 0; c < colCount; c++)
            {
                float w = TextWidth(t.Columns[c].Header, HeaderFontSize, true);
                foreach (string[] row in cells) w = Math.Max(w, TextWidth(row[c], FontSize, false));
                widths[c] = Math.Min(w, 200f) + 2 * CellPad;
            }
            float total = widths.Sum();
            float scale = usable / total;
            for (int c = 0; c < colCount; c++) widths[c] *= scale;

            // Pagination: first page has the title block
            float titleBlock = 52f;
            float tableTopFirst = PageH - Margin - titleBlock;
            float tableTopOther = PageH - Margin;
            float bottom = Margin + 18f;
            int firstPageRows = Math.Max(1, (int)((tableTopFirst - bottom) / RowH) - 1);
            int otherPageRows = Math.Max(1, (int)((tableTopOther - bottom) / RowH) - 1);

            var pages = new List<List<int>>();
            int idx = 0;
            pages.Add(new List<int>());
            while (idx < cells.Count && pages[0].Count < firstPageRows) pages[0].Add(idx++);
            while (idx < cells.Count)
            {
                var p = new List<int>();
                while (idx < cells.Count && p.Count < otherPageRows) p.Add(idx++);
                pages.Add(p);
            }

            string generated = "Generated " + DateTime.Now.ToString("d MMM yyyy, HH:mm", Inv);
            var contents = new List<string>();

            for (int pi = 0; pi < pages.Count; pi++)
            {
                var sb = new StringBuilder();
                float y = pi == 0 ? tableTopFirst : tableTopOther;

                if (pi == 0)
                {
                    float ty = PageH - Margin - 16f;
                    // Gold accent bar beside the title
                    sb.Append("0.788 0.635 0.294 rg ").Append(F(Margin)).Append(' ').Append(F(ty - 4)).Append(" 4 22 re f\n");
                    Text(sb, "F2", 16f, Margin + 12, ty, "Sam's Liquor Shop - " + t.Title, 0.1f);
                    string sub = (string.IsNullOrEmpty(t.Subtitle) ? "" : t.Subtitle + "   |   ") + generated +
                                 "   |   " + t.Rows.Count.ToString("N0", Inv) + (t.Rows.Count == 1 ? " record" : " records");
                    Text(sb, "F1", 9f, Margin + 12, ty - 17, Fit(sub, usable - 12, 9f, false), 0.4f);
                }

                // Header band
                sb.Append("0.122 0.141 0.188 rg ").Append(F(Margin)).Append(' ').Append(F(y - RowH)).Append(' ')
                  .Append(F(usable)).Append(' ').Append(F(RowH)).Append(" re f\n");
                float x = Margin;
                for (int c = 0; c < colCount; c++)
                {
                    string h = Fit(t.Columns[c].Header, widths[c] - 2 * CellPad, HeaderFontSize, true);
                    float tx = t.Columns[c].IsNumeric ? x + widths[c] - CellPad - TextWidth(h, HeaderFontSize, true) : x + CellPad;
                    TextRgb(sb, "F2", HeaderFontSize, tx, y - RowH + 5.5f, h, "1 1 1");
                    x += widths[c];
                }
                y -= RowH;

                if (pi == 0 && cells.Count == 0)
                {
                    Text(sb, "F1", FontSize, Margin + CellPad, y - RowH + 5.5f, "No records match the current filters.", 0.4f);
                }

                int n = 0;
                foreach (int ri in pages[pi])
                {
                    if (n % 2 == 1)
                    {
                        sb.Append("0.955 0.955 0.965 rg ").Append(F(Margin)).Append(' ').Append(F(y - RowH)).Append(' ')
                          .Append(F(usable)).Append(' ').Append(F(RowH)).Append(" re f\n");
                    }
                    x = Margin;
                    for (int c = 0; c < colCount; c++)
                    {
                        string v = Fit(cells[ri][c], widths[c] - 2 * CellPad, FontSize, false);
                        float tx = t.Columns[c].IsNumeric ? x + widths[c] - CellPad - TextWidth(v, FontSize, false) : x + CellPad;
                        Text(sb, "F1", FontSize, tx, y - RowH + 5.5f, v, 0.12f);
                        x += widths[c];
                    }
                    y -= RowH;
                    n++;
                }

                // Thin rule under the table, and the page footer
                sb.Append("0.8 0.8 0.82 RG 0.6 w ").Append(F(Margin)).Append(' ').Append(F(y)).Append(" m ")
                  .Append(F(Margin + usable)).Append(' ').Append(F(y)).Append(" l S\n");
                Text(sb, "F1", 8f, Margin, Margin - 2, "Sam's Liquor Shop  |  21 Coronation Road, Tongaat  |  Confidential - for internal use", 0.5f);
                string pageLabel = "Page " + (pi + 1) + " of " + pages.Count;
                Text(sb, "F1", 8f, PageW - Margin - TextWidth(pageLabel, 8f, false), Margin - 2, pageLabel, 0.5f);

                contents.Add(sb.ToString());
            }

            return AssemblePdf(contents, t.Title);
        }

        private static string F(float v)
        {
            return v.ToString("0.##", Inv);
        }

        private static void Text(StringBuilder sb, string font, float size, float x, float y, string s, float gray)
        {
            TextRgb(sb, font, size, x, y, s, F(gray) + " " + F(gray) + " " + F(gray));
        }

        private static void TextRgb(StringBuilder sb, string font, float size, float x, float y, string s, string rgb)
        {
            sb.Append("BT ").Append(rgb).Append(" rg /").Append(font).Append(' ').Append(F(size)).Append(" Tf ")
              .Append(F(x)).Append(' ').Append(F(y)).Append(" Td (").Append(PdfEscape(s)).Append(") Tj ET\n");
        }

        private static string PdfEscape(string s)
        {
            var sb = new StringBuilder();
            foreach (char ch in s)
            {
                if (ch == '\\' || ch == '(' || ch == ')') sb.Append('\\').Append(ch);
                else if (ch < 32) sb.Append(' ');
                else sb.Append(ch);
            }
            return sb.ToString();
        }

        private static byte[] AssemblePdf(List<string> pageContents, string title)
        {
            // Windows-1252 matches the fonts' WinAnsiEncoding; characters outside it become '?'
            Encoding enc = Encoding.GetEncoding(1252, new EncoderReplacementFallback("?"), new DecoderReplacementFallback("?"));

            int pageCount = pageContents.Count;
            // Object numbers: 1 catalog, 2 pages, 3 font regular, 4 font bold, 5 info, then (page, content) pairs
            var objects = new List<byte[]>();
            var kids = new StringBuilder();
            for (int i = 0; i < pageCount; i++) kids.Append(6 + i * 2).Append(" 0 R ");

            objects.Add(enc.GetBytes("<< /Type /Catalog /Pages 2 0 R >>"));
            objects.Add(enc.GetBytes("<< /Type /Pages /Kids [" + kids.ToString().Trim() + "] /Count " + pageCount + " >>"));
            objects.Add(enc.GetBytes("<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica /Encoding /WinAnsiEncoding >>"));
            objects.Add(enc.GetBytes("<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica-Bold /Encoding /WinAnsiEncoding >>"));
            objects.Add(enc.GetBytes("<< /Title (" + PdfEscape("Sam's Liquor Shop - " + title) + ") /Producer (Sam's Liquor Shop) /CreationDate (D:" +
                                     DateTime.Now.ToString("yyyyMMddHHmmss", Inv) + ") >>"));

            for (int i = 0; i < pageCount; i++)
            {
                int contentObj = 7 + i * 2;
                objects.Add(enc.GetBytes("<< /Type /Page /Parent 2 0 R /MediaBox [0 0 " + F(PageW) + " " + F(PageH) + "] " +
                                         "/Resources << /Font << /F1 3 0 R /F2 4 0 R >> >> /Contents " + contentObj + " 0 R >>"));
                byte[] stream = enc.GetBytes(pageContents[i]);
                var obj = new MemoryStream();
                byte[] head = enc.GetBytes("<< /Length " + stream.Length + " >>\nstream\n");
                obj.Write(head, 0, head.Length);
                obj.Write(stream, 0, stream.Length);
                byte[] tail = enc.GetBytes("\nendstream");
                obj.Write(tail, 0, tail.Length);
                objects.Add(obj.ToArray());
            }

            using (var ms = new MemoryStream())
            {
                Action<string> w = s => { byte[] b = enc.GetBytes(s); ms.Write(b, 0, b.Length); };
                w("%PDF-1.4\n");
                ms.Write(new byte[] { 0x25, 0xE2, 0xE3, 0xCF, 0xD3, 0x0A }, 0, 6);

                var offsets = new long[objects.Count];
                for (int i = 0; i < objects.Count; i++)
                {
                    offsets[i] = ms.Position;
                    w((i + 1) + " 0 obj\n");
                    ms.Write(objects[i], 0, objects[i].Length);
                    w("\nendobj\n");
                }

                long xref = ms.Position;
                w("xref\n0 " + (objects.Count + 1) + "\n");
                w("0000000000 65535 f \n");
                foreach (long off in offsets) w(off.ToString("0000000000", Inv) + " 00000 n \n");
                w("trailer\n<< /Size " + (objects.Count + 1) + " /Root 1 0 R /Info 5 0 R >>\nstartxref\n" + xref + "\n%%EOF\n");
                return ms.ToArray();
            }
        }
    }
}
