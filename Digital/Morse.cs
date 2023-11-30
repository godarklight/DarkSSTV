using System;
using System.Collections.Generic;
using System.Text;

namespace DarkSSTV;
class Morse
{

    private static Dictionary<char, string> encoding = new Dictionary<char, string>();
    private string dataToSend;
    private int pos = -1;
    private int charOnLeft = 0;
    private int charTotalLeft = 0;

    static Morse()
    {
        encoding['A'] = ".-";
        encoding['B'] = "-...";
        encoding['C'] = "-.-.";
        encoding['D'] = "-..";
        encoding['E'] = ".";
        encoding['F'] = "..-.";
        encoding['G'] = "--.";
        encoding['H'] = "....";
        encoding['I'] = "..";
        encoding['J'] = ".---";
        encoding['K'] = "-.-";
        encoding['L'] = ".-..";
        encoding['M'] = "--";
        encoding['N'] = "-.";
        encoding['O'] = "---";
        encoding['P'] = ".--.";
        encoding['Q'] = "--.-";
        encoding['R'] = ".-.";
        encoding['S'] = "...";
        encoding['T'] = "-";
        encoding['U'] = "..-";
        encoding['V'] = "...-";
        encoding['W'] = ".--";
        encoding['X'] = "-..-";
        encoding['Y'] = "-.--";
        encoding['Z'] = "--..";
        encoding['0'] = "-----";
        encoding['1'] = ".----";
        encoding['2'] = "..---";
        encoding['3'] = "...--";
        encoding['4'] = "....-";
        encoding['5'] = ".....";
        encoding['6'] = "-....";
        encoding['7'] = "--...";
        encoding['8'] = "---..";
        encoding['9'] = "----.";
        encoding['.'] = ".-.-.-";
        encoding[','] = "--..--";
        encoding['?'] = "..--..";
        encoding['"'] = ".----.";
        encoding['!'] = "-.-.--";
        encoding['/'] = "-..-.";
        encoding[':'] = "---...";
        encoding[';'] = "-.-.-.";
        encoding['='] = "-...-";
        encoding['+'] = ".-.-.";
        encoding['-'] = "-....-";
        encoding['_'] = "..--.-";
        encoding['\"'] = ".-..-.";
        encoding['@'] = ".--.-.";
        encoding[' '] = " ";
    }

    public Morse(string text)
    {
        string upper = text.ToUpperInvariant();
        StringBuilder sb = new StringBuilder();
        foreach (char c in upper)
        {
            if (!encoding.ContainsKey(c))
            {
                Console.WriteLine("Morse Error");
                return;
            }
            sb.Append(encoding[c]);
            sb.Append(' ');
        }
        dataToSend = sb.ToString();
    }

    public bool GetKey()
    {
        if (charTotalLeft == 0)
        {
            pos++;
            if (pos >= dataToSend.Length)
            {
                charOnLeft = 0;
                charTotalLeft = 20;
                pos = -1;
                return false;                
            }
            char read = dataToSend[pos];
            switch (read)
            {
                case '-':
                    charOnLeft = 3;
                    charTotalLeft = 4;
                    break;
                case '.':
                    charOnLeft = 1;
                    charTotalLeft = 2;
                    break;
                case ' ':
                    charOnLeft = 0;
                    charTotalLeft = 4;
                    break;

            }
        }
        bool retVal = charOnLeft > 0;
        charOnLeft--;
        charTotalLeft--;
        return retVal;
    }
}