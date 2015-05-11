using System;
using Java.Lang;

namespace MaskedEditText
{
    public class RawText
    {
        string text = "";

        public void SubtractFromString(Range range)
        {
            var firstPart = "";
            var lastPart = "";
            if (range.Start > 0 && range.Start < text.Length)
                firstPart = text.Substring(0, range.Start);
            if (range.End >= 0 && range.End < text.Length)
                lastPart = text.Substring(range.End, text.Length);
            text = firstPart + lastPart;
        }

        public int AddToString(string newString, int start, int maxLength)
        {
            var firstPart = "";
            var lastPart = "";
            if (string.IsNullOrEmpty(newString))
                return 0;
            else if (start < 0)
                throw new IllegalArgumentException("Start position must be non-negative");
            else if (start > text.Length)
                throw new IllegalArgumentException("Start position must be less than the actual text length");

            var count = newString.Length;

            if (start > 0)
                firstPart = text.Substring(0, start);

            if (start >= 0 && start < text.Length)
                lastPart = text.Substring(start, text.Length);

            if (text.Length + newString.Length > maxLength)
            {
                count = maxLength - text.Length;
                newString = newString.Substring(0, count);
            }

            text = firstPart + newString + lastPart;
            return count;
        }

        public string Text
        {
            get { return text; }
        }

        public int Length
        {
            get { return text.Length; }
        }

        public char CharAt(int position)
        {
            return text[position];
        }
    }
}

