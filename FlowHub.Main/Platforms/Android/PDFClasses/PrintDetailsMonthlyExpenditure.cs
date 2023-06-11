﻿using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using FlowHub.Models;
using iText.Kernel.Colors;
using Color = iText.Kernel.Colors.Color;
using System.Collections.ObjectModel;
using FlowHub.Main.AdditionalResourcefulApiClasses;
using static FlowHub.Main.AdditionalResourcefulApiClasses.ExchangeRateAPI;
using iText.Layout.Properties;
using TextAlignment = iText.Layout.Properties.TextAlignment;
using System.Diagnostics;
using iText.Kernel.Pdf.Canvas.Draw;

namespace FlowHub.Main.PDF_Classes;

public class PrintDetailsMonthlyExpenditure
{
    public async Task SaveListDetailMonthlyPlanned(List<ExpendituresModel> expList, string userCurrency, string printDisplayCurrency, string userName, string monthYear)
    {
        ConvertedRate ObjectWithRate = new() { result = 1, date = DateTime.UtcNow };

        if (!userCurrency.Equals(printDisplayCurrency))
        {
            ExchangeRateAPI JSONWithRates = new();

            ObjectWithRate = JSONWithRates.GetConvertedRate(userCurrency, printDisplayCurrency);
        }

        string path = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
        string fileName = $"MonthlyPlanned_{monthYear}.pdf";
        string PathFile = $"{path}/{fileName}";

        string pdfTitle = $"List Of Estimated Expenditures For {monthYear}";

        await Task.Run(()=> CreatePDFDoc(expList, PathFile, userCurrency, printDisplayCurrency, ObjectWithRate.result, ObjectWithRate.date, pdfTitle, userName));
    }
    public async Task SaveListDetailMonthlyPlanned(List<List<ExpendituresModel>> expLists, string userCurrency, string printDisplayCurrency, string userName, List<string> ListOfTitles)
    {
        ConvertedRate ObjectWithRate = new() { result = 1, date = DateTime.UtcNow };

        if (!userCurrency.Equals(printDisplayCurrency))
        {
            ExchangeRateAPI JSONWithRates = new();

            ObjectWithRate = JSONWithRates.GetConvertedRate(userCurrency, printDisplayCurrency);
        }

        string path = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
        string fileName = "Report_Multiple_MonthlyPlanned.pdf";
        string PathFile = $"{path}/{fileName}";

        await Task.Run(()=> CreatePDFDocOfMultipleLists(expLists, PathFile, ListOfTitles, userName, userCurrency, printDisplayCurrency,  ObjectWithRate.result, ObjectWithRate.date));
    }

    void CreatePDFDoc(List<ExpendituresModel> expList, string pathFile, string userCurrency, string printDisplayCurrency, double rate, DateTime dateOfRateUpdate, string pdfTitle, string username)
    {
        Color HeaderColor = WebColors.GetRGBColor("DarkSlateBlue");

        using PdfWriter writer = new(pathFile);
        using PdfDocument pdf = new(writer);
        Document document = new(pdf, pageSize: iText.Kernel.Geom.PageSize.A4, immediateFlush: false);

        Paragraph header = new Paragraph(pdfTitle)
            .SetTextAlignment(TextAlignment.CENTER)
            .SetFontColor(HeaderColor)
            .SetBold()
            .SetFontSize(20);
        document.Add(header);
        document.Flush();

        document.Add(new Paragraph());
        document.Flush();

        Table table = new Table(4, false).UseAllAvailableWidth();

        table.AddHeaderCell("#")
            .SetTextAlignment(TextAlignment.CENTER);
        table.AddHeaderCell("Description")
            .SetTextAlignment(TextAlignment.CENTER);
        table.AddHeaderCell("Amount")
            .SetTextAlignment(TextAlignment.CENTER);
        table.AddHeaderCell("Comments")
            .SetTextAlignment(TextAlignment.CENTER);

        double totalOfAllExp = 0;
        for (int i = 0; i < expList.Count; i++)
        {
            ExpendituresModel item = expList[i];
            double amount = item.AmountSpent * rate;

            table.AddCell(new Paragraph($"{expList.IndexOf(item) + 1}")
                .SetTextAlignment(TextAlignment.CENTER));

            table.AddCell(new Paragraph($"{item.Reason}")
                .SetTextAlignment(TextAlignment.CENTER));

            table.AddCell(new Paragraph($"{amount:n3} {printDisplayCurrency}")
                .SetTextAlignment(TextAlignment.CENTER));
            table.AddCell(new Paragraph($"{item.Comment}")
                .SetTextAlignment(TextAlignment.CENTER));

            if (item.IncludeInReport)
            {
                totalOfAllExp += amount;
            }
        }

        document.Add(table);
        document.Flush();

        Paragraph footerText = new Paragraph($"Total Amount {totalOfAllExp:n3} {printDisplayCurrency}")
            .SetTextAlignment(TextAlignment.CENTER)
            .SetFontSize(24)
            .SetBold();

        Paragraph waterMarkText = new Paragraph($"Report Generated by FlowHub App for {username}")
            .SetTextAlignment(TextAlignment.LEFT)
            .SetFontSize(15);
        Paragraph bottomNotesText= new Paragraph($"Converted using the rate of 1 {userCurrency} = {rate:n3} {printDisplayCurrency} \nRate updated on {dateOfRateUpdate:D}")
            .SetTextAlignment(TextAlignment.LEFT)
            .SetFontSize(10);

        document.Add(new Paragraph());
        document.Flush();

        document.Add(footerText);
        document.Flush();

        document.Add(new Paragraph());
        document.Flush();

        document.Add(waterMarkText);
        document.Flush();

        document.Add(new Paragraph());
        document.Flush();

        document.Add(bottomNotesText);
        document.Flush();

        int numberPages = pdf.GetNumberOfPages();
        for (int i = 1; i <= numberPages; i++)
        {
            document.ShowTextAligned(new Paragraph(string
               .Format("Page" + i + " of " + numberPages)),
               559, 806, i, TextAlignment.LEFT,
               iText.Layout.Properties.VerticalAlignment.BOTTOM, 0);
        }

        document.Close();

        SharePdfFile(pdfTitle, pathFile);
    }

    void CreatePDFDocOfMultipleLists(List<List<ExpendituresModel>> expLists, string pathFile, List<string> ListOfTitles, string username, string userCurrency, string printDisplayCurrency, double rate, DateTime dateOfRateUpdate)
    {
        Color HeaderColor = WebColors.GetRGBColor("DarkSlateBlue");

        PdfWriter writer = new(pathFile);
        PdfDocument pdf = new(writer);
        Document document = new(pdf, pageSize: iText.Kernel.Geom.PageSize.A4, immediateFlush: false);

        double FinalTotal = 0;
        for (int i = 0; i < expLists.Count; i++)
        {
            Paragraph header = new Paragraph($"Expenditures for {ListOfTitles[i]}")
            .SetTextAlignment(TextAlignment.CENTER)
            .SetFontColor(HeaderColor)
            .SetBold()
            .SetFontSize(25);
            document.Add(header);
            document.Flush();

            document.Add(new Paragraph());
            document.Flush();

            Table table = new Table(4, false).UseAllAvailableWidth();

            table.AddHeaderCell("#")
                .SetTextAlignment(TextAlignment.CENTER);
            table.AddHeaderCell("Description")
                .SetTextAlignment(TextAlignment.CENTER);
            table.AddHeaderCell("Amount")
                .SetTextAlignment(TextAlignment.CENTER);
            table.AddHeaderCell("Comments")
                .SetTextAlignment(TextAlignment.CENTER);

            double totalOfAllExp = 0;
            var ExpList = expLists[i];
            for (int j = 0; j < ExpList.Count; j++)
            {
                ExpendituresModel item = ExpList[j];
                double amount = item.AmountSpent * rate;

                table.AddCell(new Paragraph($"{ExpList.IndexOf(item) + 1}")
                    .SetTextAlignment(TextAlignment.CENTER));

                table.AddCell(new Paragraph($"{item.Reason}")
                    .SetTextAlignment(TextAlignment.CENTER));

                table.AddCell(new Paragraph($"{amount:n3} {printDisplayCurrency}")
                    .SetTextAlignment(TextAlignment.CENTER));
                table.AddCell(new Paragraph($"{item.Comment}")
                    .SetTextAlignment(TextAlignment.CENTER));

                if (item.IncludeInReport)
                {
                    totalOfAllExp += amount;
                    FinalTotal += amount;
                }
            }

            document.Add(table);
            document.Flush();

            Paragraph footerText = new Paragraph($"Total Amount {totalOfAllExp:n3} {printDisplayCurrency}")
                .SetTextAlignment(TextAlignment.LEFT)
                .SetFontSize(15);
            document.Add(new Paragraph());
            document.Flush();

            document.Add(footerText);
            document.Flush();
        }

        Paragraph FinalTotalParagraph = new Paragraph($"Grand Total : {FinalTotal:n3} {printDisplayCurrency}")
            .SetTextAlignment(TextAlignment.CENTER)
            .SetFontSize(20)
            .SetBold();

        Paragraph waterMarkText = new Paragraph($"Report Generated by FlowHub App for {username}")
            .SetTextAlignment(TextAlignment.LEFT)
            .SetFontSize(15);
        Paragraph bottomNotesText= new Paragraph($"Converted using the rate of 1 {userCurrency} = {rate:n3} {printDisplayCurrency} \nRate updated on {dateOfRateUpdate:D}")
            .SetTextAlignment(TextAlignment.LEFT)
            .SetFontSize(10);

        LineSeparator ls = new LineSeparator(new SolidLine());
        document.Add(ls);
        document.Flush();

        document.Add(FinalTotalParagraph);
        document.Flush();

        document.Add(new Paragraph());
        document.Flush();

        document.Add(waterMarkText);
        document.Flush();

        document.Add(new Paragraph());
        document.Flush();

        document.Add(bottomNotesText);
        document.Flush();

        int numberPages = pdf.GetNumberOfPages();
        for (int i = 1; i <= numberPages; i++)
        {
            document.ShowTextAligned(new Paragraph(string
               .Format("Page" + i + " of " + numberPages)),
               559, 806, i, TextAlignment.LEFT,
               iText.Layout.Properties.VerticalAlignment.BOTTOM, 0);
        }

        document.Close();

        SharePdfFile("Report of Multiple Months", pathFile);
    }

    async void SharePdfFile(string PdfTitle, string PathFile)
    {
        await Share.Default.RequestAsync(new ShareFileRequest
        {
            Title = PdfTitle,
            File = new ShareFile(PathFile)
        });
    }
}
