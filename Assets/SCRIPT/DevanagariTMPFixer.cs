using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Text.RegularExpressions;

[RequireComponent(typeof(TextMeshProUGUI))]
public class DevanagariTMPFixer : MonoBehaviour
{
    [TextArea]
    public string inputOriginalText;
    private TextMeshProUGUI tmpText;

    void Start()
    {
        tmpText = GetComponent<TextMeshProUGUI>();
        tmpText.text = FixDevanagari(inputOriginalText);
    }

    string FixDevanagari(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;

        // Fixes the short 'i' matra (ि) shifting issue
        // It looks for a consonant followed by the 'ि' matra and swaps them visually
        string pattern = @"([\u0915-\u0939][\u094d]?)\u093f";
        string fixedText = Regex.Replace(text, pattern, "\u093f$1");

        // Note: For complex conjuncts (half-letters), a more robust 
        // library or asset might be required depending on your font.
        return fixedText;
    }
}