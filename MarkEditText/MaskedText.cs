using System;

using Android.Content;
using Android.Text;
using Android.Util;
using Android.Views;
using Android.Widget;
using Java.Lang;

namespace MaskedEditText
{
    public class MaskedEditText : EditText, ITextWatcher, TextView.IOnEditorActionListener, TextView.IOnFocusChangeListener
    {
        private string mask;
        private char maskFill, charRepresentation;
        private int[] rawToMask, maskToRaw;
        private RawText rawText;
        private bool editingBefore, editingOnChanged, editingAfter, initialized, ignore, selectionChanged;
        private char[] charsInMask;
        private int selection, lastValidMaskPosition;
        protected int maxRawLength;
        IOnFocusChangeListener focusChangeListener;

        public MaskedEditText(Context context)
            : base(context)
        {
            Initialize();
        }

        public MaskedEditText(Context context, IAttributeSet attrs)
            : base(context, attrs)
        {
            var attributes = context.ObtainStyledAttributes(attrs, Resource.Styleable.MaskedEditText);
            mask = attributes.GetString(Resource.Styleable.MaskedEditText_mask);

            var maskFiller = attributes.GetString(Resource.Styleable.MaskedEditText_mask_fill);
            maskFill = (!string.IsNullOrEmpty(maskFiller) && maskFiller.Length > 0) ? maskFiller[0] : ' ';

            var representation = attributes.GetString(Resource.Styleable.MaskedEditText_char_representation);

            charRepresentation = representation == null ? '#' : representation[0];

            SetOnEditorActionListener(this);
            CleanUp();

            Initialize();
        }

        public MaskedEditText(Context context, IAttributeSet attrs, int defStyle)
            : base(context, attrs, defStyle)
        {
            Initialize();
        }

        void Initialize()
        {
            AddTextChangedListener(this);
            SetRawInputType(InputTypes.TextFlagNoSuggestions);
        }

        private void CleanUp()
        {
            if (mask == null)
                return;
            
            initialized = false;

            generatePositionArrays();

            rawText = new RawText();
            selection = rawToMask[0];

            editingBefore = editingOnChanged = editingAfter = true;
            this.Text = HasHint ? string.Empty : mask.Replace(charRepresentation, maskFill);

            editingBefore = editingOnChanged = editingAfter = false;

            maxRawLength = maskToRaw[previousValidPosition(mask.Length - 1)] + 1;
            lastValidMaskPosition = findLastValidMaskPosition();
            initialized = true;

            OnFocusChangeListener(this);
        }

        private int findLastValidMaskPosition()
        {
            for (var i = maskToRaw.Length - 1; i >= 0; i--)
            {
                if (maskToRaw[i] != -1)
                    return i;
            }
            throw new RuntimeException("Mask contains only the representation char");
        }

        bool HasHint
        {
            get { return Hint != null; }

        }

        public string Mask
        {
            get { return mask; }
            set
            {
                mask = value;
                CleanUp();
            }
        }

        char CharRepresentation
        {
            get { return charRepresentation; }
            set
            {
                charRepresentation = value;
                CleanUp();
            }
        }

        public bool OnEditorAction(TextView v, Android.Views.InputMethods.ImeAction actionId, KeyEvent e)
        {
            return actionId == Android.Views.InputMethods.ImeAction.Next ? false : true;
        }

        public void OnFocusChange(View v, bool hasFocus)
        {
            if (focusChangeListener != null)
                focusChangeListener.OnFocusChange(v, hasFocus);

            if (hasFocus && (rawText.Length > 0 || !HasHint))
            {
                selectionChanged = false;
                this.SetSelection(lastValidPosition());
            }
        }

        void generatePositionArrays()
        {
            var aux = new int[mask.Length];
            maskToRaw = new int[mask.Length];
            var charsInMaskAux = "";

            int charIndex = 0;
            for (var i = 0; i < mask.Length; i++)
            {
                var currentChar = mask[i];
                if (currentChar == charRepresentation)
                {
                    aux[charIndex] = i;
                    maskToRaw[i] = charIndex++;
                }
                else
                {
                    var charAsString = currentChar.ToString();
                    if (!charsInMaskAux.Contains(charAsString) && !Character.IsLetter(currentChar) &&
                        !Character.IsDigit(currentChar))
                        charsInMaskAux = charsInMaskAux + charAsString;
                    maskToRaw[i] = -1;
                }
            }
            if (charsInMaskAux.IndexOf(' ') < 0)
            {
                charsInMaskAux = charsInMaskAux + " ";
            }
            charsInMask = charsInMaskAux.ToCharArray();

            rawToMask = new int[charIndex];
            for (var i = 0; i < charIndex; i++)
                rawToMask[i] = aux[i];
        }

        int erasingStart(int start)
        {
            while (start > 0 && maskToRaw[start] == -1)
                start--;
            return start;
        }

        int fixSelection(int select)
        {
            return select > lastValidPosition() ? lastValidPosition() : nextValidPosition(select);
        }

        int nextValidPosition(int currentPosition)
        {
            while (currentPosition < lastValidMaskPosition && maskToRaw[currentPosition] == -1)
            {
                currentPosition++;
            }

            return currentPosition > lastValidMaskPosition ? lastValidMaskPosition + 1 : currentPosition;
        }

        int previousValidPosition(int currentPosition)
        {
            while (currentPosition >= 0 && maskToRaw[currentPosition] == -1)
            {
                currentPosition--;
                if (currentPosition < 0)
                    return nextValidPosition(0);
            }
            return currentPosition;
        }

        int lastValidPosition()
        {
            return rawText.Length == maxRawLength ? rawToMask[rawText.Length - 1] + 1 : nextValidPosition(rawToMask[rawText.Length]);
        }

        string makeMaskedText()
        {
            var maskedText = mask.Replace(charRepresentation, ' ').ToCharArray();
            for (int i = 0; i < rawToMask.Length; i++)
                maskedText[rawToMask[i]] = i < rawText.Length ? rawText.CharAt(i) : maskFill;

            return maskedText.ToString();
        }

        Range calculateRange(int start, int end)
        {
            var range = new Range();
            for (var i = start; i <= end && i < mask.Length; i++)
            {
                if (maskToRaw[i] != -1)
                {
                    if (range.Start == -1)
                        range.Start = maskToRaw[i];
                    range.End = maskToRaw[i];
                }
            }
            if (end == mask.Length)
                range.End = rawText.Length;
            
            if (range.Start == range.End && start < end)
            {
                int newStart = previousValidPosition(range.Start - 1);
                if (newStart < range.Start)
                    range.Start = newStart;
                
            }
            return range;
        }

        string clear(string str)
        {
            foreach (var c in charsInMask)
                str = str.Replace(Character.ToString(c), "");
            
            return str;
        }



        protected override void OnSelectionChanged(int selStart, int selEnd)
        {
            if (string.IsNullOrEmpty(mask))
            {
                base.OnSelectionChanged(selStart, selEnd);
                return;
            }
            if (initialized)
            {
                if (!selectionChanged)
                {

                    if (rawText.Length == 0 && HasHint)
                        selStart = selEnd = 0;
                    else
                    {
                        selStart = fixSelection(selStart);
                        selEnd = fixSelection(selEnd);
                    }
                    SetSelection(selStart, selEnd);
                    selectionChanged = true;
                }
                else
                {
                    if (!(HasHint && rawText.Length == 0) && selStart > rawText.Length - 1)
                        SetSelection(fixSelection(selStart), fixSelection(selEnd));
                }
            }
            base.OnSelectionChanged(selStart, selEnd);
        }

        public void OnFocusChangeListener(IOnFocusChangeListener listener)
        {
            focusChangeListener = listener;
        }

        public void OnTextChanged(Java.Lang.ICharSequence s, int start, int before, int count)
        {
            if (string.IsNullOrEmpty(mask))
            {
                return;
            }
            if (!editingOnChanged && editingBefore)
            {
                editingOnChanged = true;
                if (ignore)
                    return;
                
                if (count > 0)
                {
                    int startingPosition = maskToRaw[nextValidPosition(start)];
                    var addedString = s.SubSequence(start, start + count);
                    count = rawText.AddToString(clear(addedString), startingPosition, maxRawLength);
                    if (initialized)
                    {
                        int currentPosition = startingPosition + count < rawToMask.Length ? rawToMask[startingPosition + count] :
                            lastValidMaskPosition + 1;

                        selection = nextValidPosition(currentPosition);
                    }
                }
            }
        }

        public void AfterTextChanged(IEditable s)
        {
            if (mask == null)
            {
                return;
            }
            if (!editingAfter && editingBefore && editingOnChanged)
            {
                editingAfter = true;
                Text = rawText.Length == 0 && HasHint ? string.Empty : makeMaskedText();

                selectionChanged = false;
                SetSelection(selection);

                editingBefore = editingOnChanged = editingAfter = ignore = false;
            }
        }

        public void BeforeTextChanged(Java.Lang.ICharSequence s, int start, int count, int after)
        {
            if (string.IsNullOrEmpty(mask))
                return;
            
            if (!editingBefore)
            {
                editingBefore = true;
                ignore |= start > lastValidMaskPosition;
                
                int rangeStart = start;
                if (after == 0)
                    rangeStart = erasingStart(start);
                
                var range = calculateRange(rangeStart, start + count);

                if (range.Start != -1)
                    rawText.SubtractFromString(range);
                
                if (count > 0)
                    selection = previousValidPosition(start);
            }
        }
    }
}

