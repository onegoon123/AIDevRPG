using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// CSV 파일을 파싱하는 유틸리티 클래스
/// </summary>
public static class CSVParser
{
    /// <summary>
    /// CSV 파일을 읽어서 2차원 배열로 반환합니다
    /// </summary>
    /// <param name="filePath">CSV 파일 경로</param>
    /// <returns>파싱된 데이터</returns>
    public static string[][] ParseCSV(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Debug.LogError($"CSV 파일을 찾을 수 없습니다: {filePath}");
            return new string[0][];
        }

        List<string[]> rows = new List<string[]>();
        
        try
        {
            string[] lines = File.ReadAllLines(filePath);
            
            foreach (string line in lines)
            {
                if (string.IsNullOrEmpty(line) || line.StartsWith("#"))
                    continue; // 빈 줄이나 주석 줄 건너뛰기
                
                string[] fields = ParseCSVLine(line);
                rows.Add(fields);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"CSV 파일 읽기 오류: {e.Message}");
            return new string[0][];
        }

        return rows.ToArray();
    }

    /// <summary>
    /// CSV 라인을 파싱합니다 (쉼표로 구분, 따옴표 처리)
    /// </summary>
    /// <param name="line">CSV 라인</param>
    /// <returns>파싱된 필드들</returns>
    private static string[] ParseCSVLine(string line)
    {
        List<string> fields = new List<string>();
        bool inQuotes = false;
        string currentField = "";

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (c == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (c == ',' && !inQuotes)
            {
                fields.Add(currentField.Trim());
                currentField = "";
            }
            else
            {
                currentField += c;
            }
        }

        // 마지막 필드 추가
        fields.Add(currentField.Trim());

        return fields.ToArray();
    }

    /// <summary>
    /// Resources 폴더에서 CSV 파일을 읽습니다
    /// </summary>
    /// <param name="resourcePath">Resources 폴더 내 경로</param>
    /// <returns>파싱된 데이터</returns>
    public static string[][] ParseCSVFromResources(string resourcePath)
    {
        TextAsset csvFile = Resources.Load<TextAsset>(resourcePath);
        
        if (csvFile == null)
        {
            Debug.LogError($"Resources에서 CSV 파일을 찾을 수 없습니다: {resourcePath}");
            return new string[0][];
        }

        return ParseCSVFromText(csvFile.text);
    }

    /// <summary>
    /// 텍스트에서 CSV를 파싱합니다
    /// </summary>
    /// <param name="csvText">CSV 텍스트</param>
    /// <returns>파싱된 데이터</returns>
    public static string[][] ParseCSVFromText(string csvText)
    {
        List<string[]> rows = new List<string[]>();
        string[] lines = csvText.Split('\n');
        
        foreach (string line in lines)
        {
            if (string.IsNullOrEmpty(line) || line.StartsWith("#"))
                continue;
            
            string[] fields = ParseCSVLine(line);
            rows.Add(fields);
        }

        return rows.ToArray();
    }

    /// <summary>
    /// CSV 데이터를 Dictionary로 변환합니다
    /// </summary>
    /// <param name="csvData">CSV 데이터</param>
    /// <param name="keyColumn">키로 사용할 컬럼 인덱스</param>
    /// <returns>Dictionary 형태의 데이터</returns>
    public static Dictionary<string, string[]> ConvertToDictionary(string[][] csvData, int keyColumn = 0)
    {
        Dictionary<string, string[]> result = new Dictionary<string, string[]>();
        
        if (csvData.Length == 0) return result;
        
        // 헤더 건너뛰기 (첫 번째 행이 헤더라고 가정)
        for (int i = 1; i < csvData.Length; i++)
        {
            if (csvData[i].Length > keyColumn)
            {
                string key = csvData[i][keyColumn];
                result[key] = csvData[i];
            }
        }
        
        return result;
    }

    /// <summary>
    /// 특정 컬럼의 데이터를 가져옵니다
    /// </summary>
    /// <param name="csvData">CSV 데이터</param>
    /// <param name="columnIndex">컬럼 인덱스</param>
    /// <returns>컬럼 데이터</returns>
    public static string[] GetColumn(string[][] csvData, int columnIndex)
    {
        List<string> column = new List<string>();
        
        for (int i = 0; i < csvData.Length; i++)
        {
            if (csvData[i].Length > columnIndex)
            {
                column.Add(csvData[i][columnIndex]);
            }
        }
        
        return column.ToArray();
    }

    /// <summary>
    /// 특정 행의 데이터를 가져옵니다
    /// </summary>
    /// <param name="csvData">CSV 데이터</param>
    /// <param name="rowIndex">행 인덱스</param>
    /// <returns>행 데이터</returns>
    public static string[] GetRow(string[][] csvData, int rowIndex)
    {
        if (rowIndex >= 0 && rowIndex < csvData.Length)
        {
            return csvData[rowIndex];
        }
        
        return new string[0];
    }

    /// <summary>
    /// CSV 데이터를 디버그 출력합니다
    /// </summary>
    /// <param name="csvData">CSV 데이터</param>
    public static void DebugPrintCSV(string[][] csvData)
    {
        for (int i = 0; i < csvData.Length; i++)
        {
            string row = string.Join(", ", csvData[i]);
            Debug.Log($"Row {i}: {row}");
        }
    }
}
