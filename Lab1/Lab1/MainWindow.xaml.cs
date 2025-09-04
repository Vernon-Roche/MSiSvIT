using System;
using System.IO;
using Microsoft.Win32;
using System.Text;
using System.Windows;


namespace Lab1;


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
            items.Add(new TokenItem
            {
                Operator = i < opKeys.Count ? opKeys[i] : "",
                OperatorCount = i < opKeys.Count ? data.Item1[opKeys[i]].ToString() : "",
                Operand = i < opdKeys.Count ? opdKeys[i] : "",
                OperandCount = i < opdKeys.Count ? data.Item2[opdKeys[i]].ToString() : ""
            });
            if (i < opKeys.Count && data.Item1.TryGetValue(opKeys[i], out int value))
                totalOp += value;
            if (i < opdKeys.Count)
                totalOpd += data.Item2[opdKeys[i]];
        }

        dataGrid.ItemsSource = items;

        string metricsText = "";
        var metrics = halstead.GetMetrics(typescriptCode);
        metricsText += $"  Словарь операторов: {opKeys.Count:F2}\n";
        metricsText += $"  Словарь операндов: {opdKeys.Count:F2}\n";
        metricsText += $"  Всего операторов: {totalOp:F2}\n";
        metricsText += $"  Всего операндов: {opdKeys.Count:F2}\n";
        metricsText += $"  Словарь программы: {opdKeys.Count + opKeys.Count:F2}\n";
        metricsText += $"  Длина программы: {totalOp + totalOpd:F2}\n";
        metricsText += $"  Объём программы: {metrics.Volume:F2}\n";
        // metricsText += $"Difficulty: {metrics.Difficulty:F2}\n";
        // metricsText += $"Effort: {metrics.Effort:F2}\n";
        tbMetrics.Text = metricsText;
    }
}