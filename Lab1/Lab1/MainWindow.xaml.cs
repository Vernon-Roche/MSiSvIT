using Microsoft.Win32;
using System;
using System.Collections;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace Lab1;


// т


public partial class MainWindow : Window
{
    string typescriptCode = @"";
    public MainWindow()
    {
        InitializeComponent();
    }

    private void OpenFileClick(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog();
        if ((bool)dialog.ShowDialog())
        {
            typescriptCode = File.ReadAllText(dialog.FileName);
            tbCode.Text = typescriptCode;
        }
    }

    public static void SortByOperatorCountNonZeroFirst(List<TokenItem> items)
    {
        items.Sort((x, y) =>
        {
            bool isXZero = !int.TryParse(x.OperatorCount, out int countX) || countX == 0;
            bool isYZero = !int.TryParse(y.OperatorCount, out int countY) || countY == 0;

            if (isXZero && isYZero)
                return string.Compare(x.Operator, y.Operator, StringComparison.Ordinal);
            if (isXZero) return 1;

            if (isYZero) return -1;

            return countY.CompareTo(countX); 
                                          
        });
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        var halstead = new Halstead();     
        var data = halstead.ParseTypeScript(typescriptCode);
        var items = new List<TokenItem>();
        var opKeys = data.Item1.Keys.ToList();
        var opdKeys = data.Item2.Keys.ToList();
        int maxRows = Math.Max(opKeys.Count, opdKeys.Count);

        int totalOp = 0;
        int totalOpd = 0;
        for (int i = 0; i < maxRows; i++)
        {
            var item = new TokenItem
            {
                Operator = i < opKeys.Count ? opKeys[i] : "",
                OperatorCount = i < opKeys.Count ? data.Item1[opKeys[i]].ToString() : "",
                Operand = i < opdKeys.Count ? opdKeys[i] : "",
                OperandCount = i < opdKeys.Count ? data.Item2[opdKeys[i]].ToString() : ""
            };
            if (item.OperatorCount.Equals("0"))
            {
                item.Operator = "";
                item.OperatorCount = "";
            }
            else
            {
                if (i < opKeys.Count && data.Item1.TryGetValue(opKeys[i], out int value))
                    totalOp += value;
            }
            if (item.OperatorCount.Equals("0"))
            {
                item.Operand = "";
                item.OperandCount = "";
            }
            else
            {
                if (i < opdKeys.Count)
                    totalOpd += data.Item2[opdKeys[i]];
            }
            if (!(item.OperatorCount.Equals("0") && item.OperatorCount.Equals("0")))
                items.Add(item);        
        }

        SortByOperatorCountNonZeroFirst(items);
        dataGrid.ItemsSource = items;

        string metricsText = "";
        var metrics = halstead.GetMetrics(typescriptCode);
        metricsText += $"  Словарь операторов: {opKeys.Count:F2}\n";
        metricsText += $"  Словарь операндов: {opdKeys.Count:F2}\n";
        metricsText += $"  Всего операторов: {totalOp:F2}\n";
        metricsText += $"  Всего операндов: {totalOpd:F2}\n";
        metricsText += $"  Словарь программы: {opdKeys.Count + opKeys.Count:F2}\n";
        metricsText += $"  Длина программы: {totalOp + totalOpd:F2}\n";
        metricsText += $"  Объём программы: {metrics.Volume:F2}\n";
        // metricsText += $"Difficulty: {metrics.Difficulty:F2}\n";
        // metricsText += $"Effort: {metrics.Effort:F2}\n";
        tbMetrics.Text = metricsText;
    }
}