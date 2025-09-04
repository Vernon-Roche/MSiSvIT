using System.Collections.Generic;
using System.Windows.Input;

namespace Lab1;

public class Halstead
{
    private Dictionary<string, int> _operators;
    private Dictionary<string, int> _operands;
    private HashSet<string> _ignore;

    private void Refresh()
    {
        _operators = new Dictionary<string, int>();
        _ignore = new HashSet<string>();

        string[] items = { "const", "in", "of", "function", "let", "async", " ", "\r\n" };
        foreach (var item in items)
        {
            _ignore.Add(item);
        }

        string[] operatorKeys = {
            "in", "async", "of", "function", "let", "const", "=", "*", "**", "/", "+", "-",
            "(", ")", "[", "]", "{", "}", ".", "\r\n", "=>", ",", " ", ">", ">=", "<", "<=",
            "%", "return", "let", ";", "+=", "-=", "*=", "/=", "?", "as", "!=", "!==", "==",
            "===", ":", "for", "if", "else", "while", "do", "break", "continue"
        };

        foreach (var key in operatorKeys)
        {
            _operators[key] = 0;
        }

        _operands = new Dictionary<string, int>();
    }

    private void Calculate(string text)
    {
        string prevToken = "";
        string currentToken = "";

        bool dot = false;
        bool gravis = false;

        foreach (char symbol in text)
        {
            prevToken = currentToken;
            currentToken += symbol;

            if (_operators.ContainsKey(currentToken))
            {
                continue;
            }

            if (prevToken.Length > 0 &&
                (prevToken[prevToken.Length - 1] == '`' ||
                 prevToken[prevToken.Length - 1] == '\'' ||
                 prevToken[prevToken.Length - 1] == '"'))
            {
                gravis = true;
            }

            if (prevToken.Length > 0 &&
                (prevToken[prevToken.Length - 1] == '`' ||
                 prevToken[prevToken.Length - 1] == '\'' ||
                 prevToken[prevToken.Length - 1] == '"') &&
                (symbol == ' ' || symbol == ';' || symbol == '\r' ||
                 symbol == '\n' || symbol == ']' || symbol == ',' ||
                 symbol == ')' || symbol == '}'))
            {
                gravis = false;
            }

            if (gravis)
            {
                continue;
            }

            if (_operators.ContainsKey(prevToken))
            {
                if (symbol != ' ' && new[] { "do", "as", "in", "of" }.Contains(prevToken))
                {
                    continue;
                }

                _operators[prevToken] = (_operators.ContainsKey(prevToken) ? _operators[prevToken] : 0) + 1;
                currentToken = symbol.ToString();

                if (prevToken != ".")
                {
                    dot = false;
                }
            }
            else if (_operators.ContainsKey(symbol.ToString()))
            {
                if (dot || symbol == '(')
                {
                    if (dot)
                    {
                        _operands[prevToken] = (_operands.ContainsKey(prevToken) ? _operands[prevToken] : 0) + 1;
                    }

                    if (symbol == '(')
                    {
                        string functionKey = prevToken + "()";
                        _operators[functionKey] = (_operators.ContainsKey(functionKey) ? _operators[functionKey] : 0) + 1;
                        _operators["("] = _operators["("] - 1;
                        _operators[")"] = _operators[")"] - 1;
                    }
                }
                else
                {
                    _operands[prevToken] = (_operands.ContainsKey(prevToken) ? _operands[prevToken] : 0) + 1;
                }

                currentToken = symbol.ToString();
                dot = false;
            }
        }

        if (_operators.ContainsKey(currentToken))
        {
            _operators[currentToken] = _operators[currentToken] + 1;
        }
        else if (dot)
        {
            _operators[currentToken] = (_operators.ContainsKey(currentToken) ? _operators[currentToken] : 0) + 1;
        }
        else
        {
            _operands[currentToken] = (_operands.ContainsKey(currentToken) ? _operands[currentToken] : 0) + 1;
        }
    }

    private static void RemoveZeroValues(Dictionary<string, int> dictionary)
    {
        var keysToRemove = new List<string>();
        foreach (var kvp in dictionary)
        {
            if (kvp.Value == 0)
            {
                keysToRemove.Add(kvp.Key);
            }
        }

        foreach (var key in keysToRemove)
        {
            dictionary.Remove(key);
        }
    }

    public Tuple<Dictionary<string, int>, Dictionary<string, int>> ParseTypeScript(string text)
    {
        Refresh();
        Calculate(text);

        var newOperatorsMap = _operators
            .Where(item => !_ignore.Contains(item.Key))
            .ToDictionary(item => item.Key, item => item.Value);

        _operators = newOperatorsMap;

        ReduceRepeatingOperators(_operators);

        // так надо
        RemoveZeroValues(_operators);
        // так тоже надо
        _operands.Remove("");

        return Tuple.Create(_operators, _operands);
    }

    public HalsteadMetrics GetMetrics(string text)
    {
        var result = ParseTypeScript(text);
        var operatorsDict = result.Item1;
        var operandsDict = result.Item2;

        int distinctOperators = operatorsDict.Count;
        int distinctOperands = operandsDict.Count;
        int totalOperators = operatorsDict.Values.Sum();
        int totalOperands = operandsDict.Values.Sum();

        return new HalsteadMetrics
        {
            DistinctOperators = distinctOperators,
            DistinctOperands = distinctOperands,
            TotalOperators = totalOperators,
            TotalOperands = totalOperands,
            Vocabulary = distinctOperators + distinctOperands,
            Length = totalOperators + totalOperands,
            Volume = (totalOperators + totalOperands) * Math.Log(distinctOperators + distinctOperands, 2),
            Difficulty = (distinctOperators / 2.0) * ((double)totalOperands / distinctOperands),
            Effort = ((distinctOperators / 2.0) * ((double)totalOperands / distinctOperands)) *
                     ((totalOperators + totalOperands) * Math.Log(distinctOperators + distinctOperands, 2)),
            Time = (((distinctOperators / 2.0) * ((double)totalOperands / distinctOperands)) *
                   ((totalOperators + totalOperands) * Math.Log(distinctOperators + distinctOperands, 2))) / 18.0,
            Bugs = ((totalOperators + totalOperands) * Math.Log(distinctOperators + distinctOperands, 2)) / 3000.0
        };
    }

    private void ReduceRepeatingOperators(Dictionary<string, int> operatorsDict)
    {
        if (operatorsDict.ContainsKey("else"))
        {
            int elseCounter = operatorsDict["else"];
            operatorsDict["if ... else"] = elseCounter;
            operatorsDict["if"] = operatorsDict["if"] - elseCounter;
            operatorsDict["else"] = 0;
        }

        operatorsDict["{}"] = (operatorsDict["{"] + operatorsDict["}"]) / 2;
        operatorsDict["[]"] = (operatorsDict["["] + operatorsDict["]"]) / 2;
        operatorsDict["()"] = (operatorsDict["("] + operatorsDict[")"]) / 2;

        operatorsDict["()"] = operatorsDict["()"] - operatorsDict["if"] - operatorsDict["for"];

        operatorsDict.Remove("(");
        operatorsDict.Remove(")");
        operatorsDict.Remove("{");
        operatorsDict.Remove("}");
        operatorsDict.Remove("[");
        operatorsDict.Remove("]");
    }
}

public class HalsteadMetrics
{
    public int DistinctOperators { get; set; }
    public int DistinctOperands { get; set; }
    public int TotalOperators { get; set; }
    public int TotalOperands { get; set; }
    public int Vocabulary { get; set; }
    public int Length { get; set; }
    public double Volume { get; set; }
    public double Difficulty { get; set; }
    public double Effort { get; set; }
    public double Time { get; set; }
    public double Bugs { get; set; }
}